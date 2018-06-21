/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
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
		private IConfigurationRepository repository;
		private ILogger logger;
		private IMessageBox messageBox;
		private IText text;
		private RuntimeInfo runtimeInfo;
		private string[] commandLineArgs;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public ConfigurationOperation(
			IConfigurationRepository repository,
			ILogger logger,
			IMessageBox messageBox,
			RuntimeInfo runtimeInfo,
			IText text,
			string[] commandLineArgs)
		{
			this.repository = repository;
			this.logger = logger;
			this.messageBox = messageBox;
			this.commandLineArgs = commandLineArgs;
			this.runtimeInfo = runtimeInfo;
			this.text = text;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeConfiguration);

			var isValidUri = TryInitializeSettingsUri(out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Loading settings from '{uri.AbsolutePath}'...");

				var result = LoadSettings(uri);

				if (result == OperationResult.Success && repository.CurrentSettings.ConfigurationMode == ConfigurationMode.ConfigureClient)
				{
					var abort = IsConfigurationSufficient();

					logger.Info($"The user chose to {(abort ? "abort" : "continue")} after successful client configuration.");

					if (abort)
					{
						return OperationResult.Aborted;
					}
				}

				LogOperationResult(result);

				return result;
			}

			logger.Info("No valid settings resource specified nor found in PROGRAMDATA or APPDATA - loading default settings...");
			repository.LoadDefaultSettings();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			logger.Info("Initializing new application configuration...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeConfiguration);

			var isValidUri = TryValidateSettingsUri(repository.ReconfigurationFilePath, out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Loading settings from '{uri.AbsolutePath}'...");

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
				status = repository.LoadSettings(uri, settingsPassword, adminPassword);

				if (status == LoadStatus.InvalidData || status == LoadStatus.Success)
				{
					break;
				}
				else if (status == LoadStatus.AdminPasswordNeeded || status == LoadStatus.SettingsPasswordNeeded)
				{
					var isAdmin = status == LoadStatus.AdminPasswordNeeded;
					var success = isAdmin ? TryGetAdminPassword(out adminPassword) : TryGetSettingsPassword(out settingsPassword);

					if (success)
					{
						adminAttempts += isAdmin ? 1 : 0;
						settingsAttempts += isAdmin ? 0 : 1;
					}
					else
					{
						return OperationResult.Aborted;
					}
				}
			}

			if (status == LoadStatus.InvalidData)
			{
				if (IsHtmlPage(uri))
				{
					repository.LoadDefaultSettings();
					repository.CurrentSettings.Browser.StartUrl = uri.AbsoluteUri;
					logger.Info($"The specified URI '{uri.AbsoluteUri}' appears to point to a HTML page, setting it as startup URL.");

					return OperationResult.Success;
				}

				logger.Error($"The specified settings resource '{uri.AbsoluteUri}' is invalid!");
			}

			return status == LoadStatus.Success ? OperationResult.Success : OperationResult.Failed;
		}

		private bool IsHtmlPage(Uri uri)
		{
			// TODO
			return false;
		}

		private bool TryGetAdminPassword(out string password)
		{
			password = default(string);

			// TODO

			return true;
		}

		private bool TryGetSettingsPassword(out string password)
		{
			password = default(string);

			// TODO

			return true;
		}

		private bool TryInitializeSettingsUri(out Uri uri)
		{
			var path = string.Empty;
			var isValidUri = false;
			var programDataSettings = Path.Combine(runtimeInfo.ProgramDataFolder, runtimeInfo.DefaultSettingsFileName);
			var appDataSettings = Path.Combine(runtimeInfo.AppDataFolder, runtimeInfo.DefaultSettingsFileName);

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
