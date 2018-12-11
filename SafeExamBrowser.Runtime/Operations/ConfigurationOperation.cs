/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Runtime.Operations.Events;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ConfigurationOperation : SessionOperation
	{
		private string[] commandLineArgs;
		private IConfigurationRepository configuration;
		private ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ConfigurationOperation(
			string[] commandLineArgs,
			IConfigurationRepository configuration,
			ILogger logger,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.commandLineArgs = commandLineArgs;
			this.configuration = configuration;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

			var result = OperationResult.Failed;
			var isValidUri = TryInitializeSettingsUri(out Uri uri);

			if (isValidUri)
			{
				result = LoadSettings(uri);
			}
			else
			{
				result = LoadDefaultSettings();
			}

			LogOperationResult(result);

			return result;
		}

		public override OperationResult Repeat()
		{
			logger.Info("Initializing new application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

			var result = OperationResult.Failed;
			var isValidUri = TryValidateSettingsUri(Context.ReconfigurationFilePath, out Uri uri);

			if (isValidUri)
			{
				result = LoadSettings(uri);
			}
			else
			{
				logger.Warn($"The resource specified for reconfiguration does not exist or is not valid!");
			}

			LogOperationResult(result);

			return result;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult LoadDefaultSettings()
		{
			logger.Info("No valid configuration resource specified nor found in PROGRAMDATA or APPDATA - loading default settings...");
			Context.Next.Settings = configuration.LoadDefaultSettings();

			return OperationResult.Success;
		}

		private OperationResult LoadSettings(Uri uri)
		{
			var settings = default(Settings);
			var status = default(LoadStatus);
			var passwordInfo = new PasswordInfo { AdminPasswordHash = Context.Current?.Settings?.AdminPasswordHash };

			for (int adminAttempts = 0, settingsAttempts = 0; adminAttempts < 5 && settingsAttempts < 5;)
			{
				status = configuration.TryLoadSettings(uri, passwordInfo, out settings);

				if (status != LoadStatus.AdminPasswordNeeded && status != LoadStatus.SettingsPasswordNeeded)
				{
					break;
				}

				var success = TryGetPassword(status, passwordInfo);

				if (!success)
				{
					return OperationResult.Aborted;
				}

				adminAttempts += status == LoadStatus.AdminPasswordNeeded ? 1 : 0;
				settingsAttempts += status == LoadStatus.SettingsPasswordNeeded ? 1 : 0;
			}

			Context.Next.Settings = settings;

			if (status == LoadStatus.Success)
			{
				return OperationResult.Success;
			}

			if (status == LoadStatus.SuccessConfigureClient)
			{
				return HandleClientConfiguration();
			}

			ShowFailureMessage(status, uri);

			return OperationResult.Failed;
		}

		private void ShowFailureMessage(LoadStatus status, Uri uri)
		{
			switch (status)
			{
				case LoadStatus.AdminPasswordNeeded:
				case LoadStatus.SettingsPasswordNeeded:
					ActionRequired?.Invoke(new InvalidPasswordMessageArgs());
					break;
				case LoadStatus.InvalidData:
					ActionRequired?.Invoke(new InvalidDataMessageArgs(uri.ToString()));
					break;
				case LoadStatus.NotSupported:
					ActionRequired?.Invoke(new NotSupportedMessageArgs(uri.ToString()));
					break;
				case LoadStatus.UnexpectedError:
					ActionRequired?.Invoke(new UnexpectedErrorMessageArgs(uri.ToString()));
					break;
			}
		}

		private bool TryGetPassword(LoadStatus status, PasswordInfo passwordInfo)
		{
			var purpose = status == LoadStatus.AdminPasswordNeeded ? PasswordRequestPurpose.Administrator : PasswordRequestPurpose.Settings;
			var args = new PasswordRequiredEventArgs { Purpose = purpose };

			ActionRequired?.Invoke(args);

			if (purpose == PasswordRequestPurpose.Administrator)
			{
				passwordInfo.AdminPassword = args.Password;
			}
			else
			{
				passwordInfo.SettingsPassword = args.Password;
			}

			return args.Success;
		}

		private bool TryInitializeSettingsUri(out Uri uri)
		{
			var path = string.Empty;
			var isValidUri = false;
			var programDataSettings = Path.Combine(Context.Next.AppConfig.ProgramDataFolder, Context.Next.AppConfig.DefaultSettingsFileName);
			var appDataSettings = Path.Combine(Context.Next.AppConfig.AppDataFolder, Context.Next.AppConfig.DefaultSettingsFileName);

			uri = null;

			if (commandLineArgs?.Length > 1)
			{
				path = commandLineArgs[1];
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found command-line argument for configuration resource: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(programDataSettings))
			{
				path = programDataSettings;
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found configuration file in PROGRAMDATA: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(appDataSettings))
			{
				path = appDataSettings;
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found configuration file in APPDATA: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
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

		private OperationResult HandleClientConfiguration()
		{
			var firstSession = Context.Current == null;

			if (firstSession)
			{
				var args = new ConfigurationCompletedEventArgs();
					
				ActionRequired?.Invoke(args);
				logger.Info($"The user chose to {(args.AbortStartup ? "abort" : "continue")} after successful client configuration.");

				if (args.AbortStartup)
				{
					return OperationResult.Aborted;
				}
			}
			else
			{
				// TODO: If the client configuration happens while the application is already running, the new configuration should first
				// be loaded and then the user should have the option to terminate!
				// -> Introduce flag in Context, e.g. AskForTermination?
			}

			return OperationResult.Success;
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
