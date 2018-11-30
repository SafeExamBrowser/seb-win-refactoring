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
			this.logger = logger;
			this.configuration = configuration;
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
				HandleClientConfiguration(ref result, uri);
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
				HandleClientConfiguration(ref result, uri);
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
			var status = configuration.TryLoadSettings(uri, out Settings settings, Context.Current?.Settings?.AdminPasswordHash, true);

			for (var attempts = 0; attempts < 5 && status == LoadStatus.PasswordNeeded; attempts++)
			{
				var result = TryGetPassword();

				if (!result.Success)
				{
					return OperationResult.Aborted;
				}

				status = configuration.TryLoadSettings(uri, out settings, result.Password);
			}

			if (status == LoadStatus.Success)
			{
				Context.Next.Settings = settings;
			}
			else
			{
				ShowFailureMessage(status, uri);
			}

			return status == LoadStatus.Success ? OperationResult.Success : OperationResult.Failed;
		}

		private void ShowFailureMessage(LoadStatus status, Uri uri)
		{
			switch (status)
			{
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

		private PasswordRequiredEventArgs TryGetPassword()
		{
			var purpose = PasswordRequestPurpose.Settings;
			var args = new PasswordRequiredEventArgs { Purpose = purpose };

			ActionRequired?.Invoke(args);

			return args;
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

		private void HandleClientConfiguration(ref OperationResult result, Uri uri)
		{
			var configureMode = Context.Next.Settings?.ConfigurationMode == ConfigurationMode.ConfigureClient;
			var loadWithBrowser = Context.Next.Settings?.Browser.StartUrl == uri.AbsoluteUri;
			var successful = result == OperationResult.Success;

			if (successful && configureMode && !loadWithBrowser)
			{
				var args = new ConfigurationCompletedEventArgs();
				var filePath = Path.Combine(Context.Next.AppConfig.AppDataFolder, Context.Next.AppConfig.DefaultSettingsFileName);

				// TODO: Save / overwrite configuration file in APPDATA directory!
				// -> Check whether current and new admin passwords are the same! If not, current needs to be verified before overwriting!
				//		-> Special case: If current admin password is same as new settings password, verification is not necessary!
				// -> Default settings password appears to be string.Empty for local client configuration
				// -> Any (new?) certificates need to be imported and REMOVED from the settings before the data is saved!
				// -> DO NOT transform settings, just simply copy the given configuration file to %APPDATA%\SebClientSettings.seb !!!
				//configuration.SaveSettings(Context.Next.Settings, filePath);

				// TODO: If the client configuration happens while the application is already running, the new configuration should first
				// be loaded and then the user should have the option to terminate!
				// -> Introduce flag in Context, e.g. AskForTermination?
				ActionRequired?.Invoke(args);

				logger.Info($"The user chose to {(args.AbortStartup ? "abort" : "continue")} after successful client configuration.");

				if (args.AbortStartup)
				{
					result = OperationResult.Aborted;
				}
			}
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
