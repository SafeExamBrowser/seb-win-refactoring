/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
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
		private IHashAlgorithm hashAlgorithm;
		private ILogger logger;

		private string AppDataFile
		{
			get { return Path.Combine(Context.Next.AppConfig.AppDataFolder, Context.Next.AppConfig.DefaultSettingsFileName); }
		}

		private string ProgramDataFile
		{
			get { return Path.Combine(Context.Next.AppConfig.ProgramDataFolder, Context.Next.AppConfig.DefaultSettingsFileName); }
		}

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ConfigurationOperation(
			string[] commandLineArgs,
			IConfigurationRepository configuration,
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.commandLineArgs = commandLineArgs;
			this.configuration = configuration;
			this.hashAlgorithm = hashAlgorithm;
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
			var passwordParams = new PasswordParameters { Password = string.Empty, IsHash = true };
			var status = configuration.TryLoadSettings(uri, out var settings, passwordParams);

			if (status == LoadStatus.PasswordNeeded && Context.Current?.Settings.AdminPasswordHash != null)
			{
				passwordParams.Password = Context.Current.Settings.AdminPasswordHash;
				passwordParams.IsHash = true;

				status = configuration.TryLoadSettings(uri, out settings, passwordParams);
			}

			for (int attempts = 0; attempts < 5 && status == LoadStatus.PasswordNeeded; attempts++)
			{
				var success = TryGetPassword(PasswordRequestPurpose.Settings, out var password);

				if (success)
				{
					passwordParams.Password = password;
					passwordParams.IsHash = false;
				}
				else
				{
					return OperationResult.Aborted;
				}

				status = configuration.TryLoadSettings(uri, out settings, passwordParams);
			}

			Context.Next.Settings = settings;

			return HandleLoadResult(uri, settings, status, passwordParams);
		}

		private OperationResult HandleLoadResult(Uri uri, Settings settings, LoadStatus status, PasswordParameters password)
		{
			if (status == LoadStatus.LoadWithBrowser)
			{
				return HandleBrowserResource(uri);
			}

			if (status == LoadStatus.Success && settings.ConfigurationMode == ConfigurationMode.ConfigureClient)
			{
				return HandleClientConfiguration(uri, password);
			}

			if (status == LoadStatus.Success)
			{
				return OperationResult.Success;
			}

			ShowFailureMessage(status, uri);

			return OperationResult.Failed;
		}

		private OperationResult HandleBrowserResource(Uri uri)
		{
			Context.Next.Settings.Browser.StartUrl = uri.AbsoluteUri;
			logger.Info($"The configuration resource needs authentication or is a webpage, using '{uri}' as startup URL for the browser.");

			return OperationResult.Success;
		}

		private OperationResult HandleClientConfiguration(Uri resource, PasswordParameters password)
		{
			var isAppDataFile = Path.GetFullPath(resource.AbsolutePath).Equals(AppDataFile, StringComparison.OrdinalIgnoreCase);
			var isProgramDataFile = Path.GetFullPath(resource.AbsolutePath).Equals(ProgramDataFile, StringComparison.OrdinalIgnoreCase);
			var isFirstSession = Context.Current == null;

			if (!isAppDataFile && !isProgramDataFile)
			{
				var requiresAuthentication = IsAuthenticationRequiredForClientConfiguration(password);

				logger.Info("Starting client configuration...");

				if (requiresAuthentication)
				{
					var result = HandleClientConfigurationAuthentication();

					if (result != OperationResult.Success)
					{
						return result;
					}
				}
				else
				{
					logger.Info("Authentication is not required.");
				}

				var status = configuration.ConfigureClientWith(resource, password);

				if (status == SaveStatus.Success)
				{
					logger.Info("Client configuration was successful.");
				}
				else
				{
					logger.Error($"Client configuration failed with status '{status}'!");
					ActionRequired?.Invoke(new ClientConfigurationErrorMessageArgs());

					return OperationResult.Failed;
				}

				if (isFirstSession)
				{
					var result = HandleClientConfigurationOnStartup();

					if (result != OperationResult.Success)
					{
						return result;
					}
				}
			}

			return OperationResult.Success;
		}

		private bool IsAuthenticationRequiredForClientConfiguration(PasswordParameters password)
		{
			var requiresAuthentication = Context.Current?.Settings.AdminPasswordHash != null;

			if (requiresAuthentication)
			{
				var currentPassword = Context.Current.Settings.AdminPasswordHash;
				var nextPassword = Context.Next.Settings.AdminPasswordHash;
				var hasSettingsPassword = password.Password != null;
				var sameAdminPassword = currentPassword.Equals(nextPassword, StringComparison.OrdinalIgnoreCase);

				requiresAuthentication = !sameAdminPassword;

				if (requiresAuthentication && hasSettingsPassword)
				{
					var settingsPassword = password.IsHash ? password.Password : hashAlgorithm.GenerateHashFor(password.Password);
					var knowsAdminPassword = currentPassword.Equals(settingsPassword, StringComparison.OrdinalIgnoreCase);

					requiresAuthentication = !knowsAdminPassword;
				}
			}

			return requiresAuthentication;
		}

		private OperationResult HandleClientConfigurationAuthentication()
		{
			var currentPassword = Context.Current.Settings.AdminPasswordHash;
			var isSamePassword = false;

			for (int attempts = 0; attempts < 5 && !isSamePassword; attempts++)
			{
				var success = TryGetPassword(PasswordRequestPurpose.Administrator, out var password);

				if (success)
				{
					isSamePassword = currentPassword.Equals(hashAlgorithm.GenerateHashFor(password), StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					logger.Info("Authentication was aborted.");

					return OperationResult.Aborted;
				}
			}

			if (isSamePassword)
			{
				logger.Info("Authentication was successful.");

				return OperationResult.Success;
			}

			logger.Info("Authentication has failed!");
			ActionRequired?.Invoke(new InvalidPasswordMessageArgs());

			return OperationResult.Failed;
		}

		private OperationResult HandleClientConfigurationOnStartup()
		{
			var args = new ConfigurationCompletedEventArgs();

			ActionRequired?.Invoke(args);
			logger.Info($"The user chose to {(args.AbortStartup ? "abort" : "continue")} startup after successful client configuration.");

			if (args.AbortStartup)
			{
				return OperationResult.Aborted;
			}

			return OperationResult.Success;
		}

		private void ShowFailureMessage(LoadStatus status, Uri uri)
		{
			switch (status)
			{
				case LoadStatus.PasswordNeeded:
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

		private bool TryGetPassword(PasswordRequestPurpose purpose, out string password)
		{
			var args = new PasswordRequiredEventArgs { Purpose = purpose };

			ActionRequired?.Invoke(args);
			password = args.Password;

			return args.Success;
		}

		private bool TryInitializeSettingsUri(out Uri uri)
		{
			var path = default(string);
			var isValidUri = false;

			uri = null;

			if (commandLineArgs?.Length > 1)
			{
				path = commandLineArgs[1];
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found command-line argument for configuration resource: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(ProgramDataFile))
			{
				path = ProgramDataFile;
				isValidUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
				logger.Info($"Found configuration file in PROGRAMDATA: '{path}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(AppDataFile))
			{
				path = AppDataFile;
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
