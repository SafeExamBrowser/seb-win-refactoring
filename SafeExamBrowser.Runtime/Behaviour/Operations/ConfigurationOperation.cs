/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Threading;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ConfigurationOperation : IOperation
	{
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IMessageBox messageBox;
		private IResourceLoader resourceLoader;
		private IRuntimeHost runtimeHost;
		private AppConfig appConfig;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private string[] commandLineArgs;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public ConfigurationOperation(
			AppConfig appConfig,
			IConfigurationRepository configuration,
			ILogger logger,
			IMessageBox messageBox,
			IResourceLoader resourceLoader,
			IRuntimeHost runtimeHost,
			IText text,
			IUserInterfaceFactory uiFactory,
			string[] commandLineArgs)
		{
			this.appConfig = appConfig;
			this.logger = logger;
			this.messageBox = messageBox;
			this.configuration = configuration;
			this.resourceLoader = resourceLoader;
			this.runtimeHost = runtimeHost;
			this.text = text;
			this.uiFactory = uiFactory;
			this.commandLineArgs = commandLineArgs;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeConfiguration);

			var isValidUri = TryInitializeSettingsUri(out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Attempting to load settings from '{uri.AbsolutePath}'...");

				var result = LoadSettings(uri);

				HandleClientConfiguration(ref result);
				LogOperationResult(result);

				return result;
			}

			logger.Info("No valid settings resource specified nor found in PROGRAMDATA or APPDATA - loading default settings...");
			configuration.LoadDefaultSettings();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			logger.Info("Initializing new application configuration...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeConfiguration);

			var isValidUri = TryValidateSettingsUri(configuration.ReconfigurationFilePath, out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Attempting to load settings from '{uri.AbsolutePath}'...");

				var result = LoadSettings(uri);

				LogOperationResult(result);

				return result;
			}

			logger.Warn($"The resource specified for reconfiguration does not exist or is not a file!");

			return OperationResult.Failed;
		}

		public void Revert()
		{
			// Nothing to do here...
		}

		private OperationResult LoadSettings(Uri uri)
		{
			var adminPassword = default(string);
			var settingsPassword = default(string);
			var status = default(LoadStatus);

			for (int adminAttempts = 0, settingsAttempts = 0; adminAttempts < 5 && settingsAttempts < 5;)
			{
				status = configuration.LoadSettings(uri, adminPassword, settingsPassword);

				if (status == LoadStatus.AdminPasswordNeeded || status == LoadStatus.SettingsPasswordNeeded)
				{
					var purpose = status == LoadStatus.AdminPasswordNeeded ? PasswordRequestPurpose.Administrator : PasswordRequestPurpose.Settings;
					var aborted = !TryGetPassword(purpose, out string password);

					adminAttempts += purpose == PasswordRequestPurpose.Administrator ? 1 : 0;
					adminPassword = purpose == PasswordRequestPurpose.Administrator ? password : adminPassword;
					settingsAttempts += purpose == PasswordRequestPurpose.Settings ? 1 : 0;
					settingsPassword = purpose == PasswordRequestPurpose.Settings ? password : settingsPassword;

					if (aborted)
					{
						return OperationResult.Aborted;
					}
				}
				else
				{
					break;
				}
			}

			if (status == LoadStatus.InvalidData)
			{
				HandleInvalidData(ref status, uri);
			}

			return status == LoadStatus.Success ? OperationResult.Success : OperationResult.Failed;
		}

		private bool TryGetPassword(PasswordRequestPurpose purpose, out string password)
		{
			var isStartup = configuration.CurrentSession == null;
			var isRunningOnDefaultDesktop = configuration.CurrentSettings?.KioskMode == KioskMode.DisableExplorerShell;

			if (isStartup || isRunningOnDefaultDesktop)
			{
				return TryGetPasswordViaDialog(purpose, out password);
			}
			else
			{
				return TryGetPasswordViaClient(purpose, out password);
			}
		}

		private bool TryGetPasswordViaDialog(PasswordRequestPurpose purpose, out string password)
		{
			var isAdmin = purpose == PasswordRequestPurpose.Administrator;
			var message = isAdmin ? TextKey.PasswordDialog_AdminPasswordRequired : TextKey.PasswordDialog_SettingsPasswordRequired;
			var title = isAdmin ? TextKey.PasswordDialog_AdminPasswordRequiredTitle : TextKey.PasswordDialog_SettingsPasswordRequiredTitle;
			var dialog = uiFactory.CreatePasswordDialog(text.Get(message), text.Get(title));
			var result = dialog.Show();

			if (result.Success)
			{
				password = result.Password;
			}
			else
			{
				password = default(string);
			}

			return result.Success;
		}

		private bool TryGetPasswordViaClient(PasswordRequestPurpose purpose, out string password)
		{
			var requestId = Guid.NewGuid();
			var response = default(PasswordReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<PasswordReplyEventArgs>((args) =>
			{
				if (args.RequestId == requestId)
				{
					response = args;
					responseEvent.Set();
				}
			});

			runtimeHost.PasswordReceived += responseEventHandler;
			configuration.CurrentSession.ClientProxy.RequestPassword(purpose, requestId);
			responseEvent.WaitOne();
			runtimeHost.PasswordReceived -= responseEventHandler;

			if (response.Success)
			{
				password = response.Password;
			}
			else
			{
				password = default(string);
			}

			return response.Success;
		}

		private void HandleInvalidData(ref LoadStatus status, Uri uri)
		{
			if (resourceLoader.IsHtmlResource(uri))
			{
				configuration.LoadDefaultSettings();
				configuration.CurrentSettings.Browser.StartUrl = uri.AbsoluteUri;
				logger.Info($"The specified URI '{uri.AbsoluteUri}' appears to point to a HTML resource, setting it as startup URL.");

				status = LoadStatus.Success;
			}
			else
			{
				logger.Error($"The specified settings resource '{uri.AbsoluteUri}' is invalid!");
			}
		}

		private bool TryInitializeSettingsUri(out Uri uri)
		{
			var path = string.Empty;
			var isValidUri = false;
			var programDataSettings = Path.Combine(appConfig.ProgramDataFolder, appConfig.DefaultSettingsFileName);
			var appDataSettings = Path.Combine(appConfig.AppDataFolder, appConfig.DefaultSettingsFileName);

			uri = null;

			if (commandLineArgs?.Length > 1)
			{
				path = commandLineArgs[1];
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found command-line argument for settings file: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(programDataSettings))
			{
				path = programDataSettings;
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found settings file in PROGRAMDATA: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(appDataSettings))
			{
				path = appDataSettings;
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found settings file in APPDATA: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			return isValidUri;
		}

		private bool TryValidateSettingsUri(string path, out Uri uri)
		{
			var isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);

			isValidUri &= uri != null && uri.IsFile;
			isValidUri &= File.Exists(path);

			return isValidUri;
		}

		private void HandleClientConfiguration(ref OperationResult result)
		{
			if (result == OperationResult.Success && configuration.CurrentSettings.ConfigurationMode == ConfigurationMode.ConfigureClient)
			{
				var abort = IsConfigurationSufficient();

				logger.Info($"The user chose to {(abort ? "abort" : "continue")} after successful client configuration.");

				if (abort)
				{
					result = OperationResult.Aborted;
				}
			}
		}

		private bool IsConfigurationSufficient()
		{
			var message = text.Get(TextKey.MessageBox_ClientConfigurationQuestion);
			var title = text.Get(TextKey.MessageBox_ClientConfigurationQuestionTitle);
			var abort = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question);

			return abort == MessageBoxResult.Yes;
		}

		private void LogOperationResult(OperationResult result)
		{
			switch (result)
			{
				case OperationResult.Aborted:
					logger.Info("The configuration was aborted by the user.");
					break;
				case OperationResult.Failed:
					logger.Warn("The configuration has failed!");
					break;
				case OperationResult.Success:
					logger.Info("The configuration was successful.");
					break;
			}
		}
	}
}
