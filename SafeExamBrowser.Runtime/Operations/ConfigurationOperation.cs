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
		private IResourceLoader resourceLoader;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ConfigurationOperation(
			string[] commandLineArgs,
			IConfigurationRepository configuration,
			ILogger logger,
			IResourceLoader resourceLoader,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.commandLineArgs = commandLineArgs;
			this.logger = logger;
			this.configuration = configuration;
			this.resourceLoader = resourceLoader;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

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

		public override OperationResult Repeat()
		{
			logger.Info("Initializing new application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

			var isValidUri = TryValidateSettingsUri(Context.ReconfigurationFilePath, out Uri uri);

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

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult LoadSettings(Uri uri)
		{
			var adminPassword = default(string);
			var settingsPassword = default(string);
			var settings = default(Settings);
			var status = default(LoadStatus);

			for (int adminAttempts = 0, settingsAttempts = 0; adminAttempts < 5 && settingsAttempts < 5;)
			{
				status = configuration.TryLoadSettings(uri, out settings, adminPassword, settingsPassword);

				if (status == LoadStatus.AdminPasswordNeeded || status == LoadStatus.SettingsPasswordNeeded)
				{
					var result = TryGetPassword(status);

					if (!result.Success)
					{
						return OperationResult.Aborted;
					}

					adminAttempts += status == LoadStatus.AdminPasswordNeeded ? 1 : 0;
					adminPassword = status == LoadStatus.AdminPasswordNeeded ? result.Password : adminPassword;
					settingsAttempts += status == LoadStatus.SettingsPasswordNeeded ? 1 : 0;
					settingsPassword = status == LoadStatus.SettingsPasswordNeeded ? result.Password : settingsPassword;
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

			if (status == LoadStatus.Success)
			{
				Context.Next.Settings = settings;

				return OperationResult.Success;
			}


			return OperationResult.Failed;
		}

		private PasswordRequiredEventArgs TryGetPassword(LoadStatus status)
		{
			var purpose = status == LoadStatus.AdminPasswordNeeded ? PasswordRequestPurpose.Administrator : PasswordRequestPurpose.Settings;
			var args = new PasswordRequiredEventArgs { Purpose = purpose };

			ActionRequired?.Invoke(args);

			return args;
		}

		private void HandleInvalidData(ref LoadStatus status, Uri uri)
		{
			if (resourceLoader.IsHtmlResource(uri))
			{
				configuration.LoadDefaultSettings();
				Context.Next.Settings.Browser.StartUrl = uri.AbsoluteUri;
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
			var programDataSettings = Path.Combine(Context.Next.AppConfig.ProgramDataFolder, Context.Next.AppConfig.DefaultSettingsFileName);
			var appDataSettings = Path.Combine(Context.Next.AppConfig.AppDataFolder, Context.Next.AppConfig.DefaultSettingsFileName);

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
			if (result == OperationResult.Success && Context.Next.Settings.ConfigurationMode == ConfigurationMode.ConfigureClient)
			{
				var args = new ConfigurationCompletedEventArgs();

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
