/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ConfigurationOperation : ConfigurationBaseOperation
	{
		private readonly IFileSystem fileSystem;
		private readonly IHashAlgorithm hashAlgorithm;
		private readonly ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ConfigurationOperation(
			string[] commandLineArgs,
			IConfigurationRepository configuration,
			IFileSystem fileSystem,
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			SessionContext sessionContext) : base(commandLineArgs, configuration, sessionContext)
		{
			this.fileSystem = fileSystem;
			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

			var result = OperationResult.Failed;
			var isValidUri = TryInitializeSettingsUri(out var uri, out var source);

			if (isValidUri)
			{
				result = LoadSettingsForStartup(uri, source);
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
			var isValidUri = TryValidateSettingsUri(Context.ReconfigurationFilePath, out var uri);

			if (isValidUri)
			{
				result = LoadSettingsForReconfiguration(uri);
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

		protected override void InvokeActionRequired(ActionRequiredEventArgs args)
		{
			ActionRequired?.Invoke(args);
		}

		private OperationResult LoadDefaultSettings()
		{
			logger.Info("No valid configuration resource specified and no local client configuration found - loading default settings...");
			Context.Next.Settings = configuration.LoadDefaultSettings();

			return OperationResult.Success;
		}

		private OperationResult LoadSettingsForStartup(Uri uri, UriSource source)
		{
			var currentPassword = default(string);
			var passwordParams = default(PasswordParameters);
			var settings = default(AppSettings);
			var status = default(LoadStatus?);

			if (source == UriSource.CommandLine)
			{
				var hasAppDataFile = File.Exists(AppDataFilePath);
				var hasProgramDataFile = File.Exists(ProgramDataFilePath);

				if (hasProgramDataFile)
				{
					status = TryLoadSettings(new Uri(ProgramDataFilePath, UriKind.Absolute), UriSource.ProgramData, out _, out settings);
				}
				else if (hasAppDataFile)
				{
					status = TryLoadSettings(new Uri(AppDataFilePath, UriKind.Absolute), UriSource.AppData, out _, out settings);
				}

				if ((!hasProgramDataFile && !hasAppDataFile) || status == LoadStatus.Success)
				{
					currentPassword = settings?.Security.AdminPasswordHash;
					status = TryLoadSettings(uri, source, out passwordParams, out settings, currentPassword);
				}
			}
			else
			{
				status = TryLoadSettings(uri, source, out passwordParams, out settings);
			}

			if (status.HasValue)
			{
				return DetermineLoadResult(uri, source, settings, status.Value, passwordParams, currentPassword);
			}
			else
			{
				return OperationResult.Aborted;
			}
		}

		private OperationResult LoadSettingsForReconfiguration(Uri uri)
		{
			var currentPassword = Context.Current.Settings.Security.AdminPasswordHash;
			var source = UriSource.Reconfiguration;
			var status = TryLoadSettings(uri, source, out var passwordParams, out var settings, currentPassword);
			var result = OperationResult.Failed;

			if (status.HasValue)
			{
				result = DetermineLoadResult(uri, source, settings, status.Value, passwordParams, currentPassword);
			}
			else
			{
				result = OperationResult.Aborted;
			}

			fileSystem.Delete(uri.LocalPath);
			logger.Info($"Deleted temporary configuration file '{uri}'.");

			return result;
		}

		private OperationResult DetermineLoadResult(Uri uri, UriSource source, AppSettings settings, LoadStatus status, PasswordParameters passwordParams, string currentPassword = default(string))
		{
			var result = OperationResult.Failed;

			if (status == LoadStatus.LoadWithBrowser || status == LoadStatus.Success)
			{
				var isNewConfiguration = source == UriSource.CommandLine || source == UriSource.Reconfiguration;

				Context.Next.Settings = settings;

				if (status == LoadStatus.LoadWithBrowser)
				{
					result = HandleBrowserResource(uri);
				}
				else if (isNewConfiguration && settings.ConfigurationMode == ConfigurationMode.ConfigureClient)
				{
					result = HandleClientConfiguration(uri, passwordParams, currentPassword);
				}
				else
				{
					result = OperationResult.Success;
				}

				HandleStartUrlQuery(uri, source);
			}
			else
			{
				ShowFailureMessage(status, uri);
			}

			return result;
		}

		private OperationResult HandleBrowserResource(Uri uri)
		{
			Context.Next.Settings.Applications.Blacklist.Clear();
			Context.Next.Settings.Applications.Whitelist.Clear();
			Context.Next.Settings.Display.AllowedDisplays = 10;
			Context.Next.Settings.Display.IgnoreError = true;
			Context.Next.Settings.Display.InternalDisplayOnly = false;
			Context.Next.Settings.Browser.DeleteCacheOnShutdown = false;
			Context.Next.Settings.Browser.DeleteCookiesOnShutdown = false;
			Context.Next.Settings.Browser.StartUrl = uri.AbsoluteUri;
			Context.Next.Settings.Security.AllowReconfiguration = true;
			Context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Allow;
			Context.Next.Settings.Service.IgnoreService = true;

			logger.Info($"The configuration resource needs authentication or is a webpage, using '{uri}' as start URL for the browser.");

			return OperationResult.Success;
		}

		private OperationResult HandleClientConfiguration(Uri uri, PasswordParameters passwordParams, string currentPassword = default(string))
		{
			var isFirstSession = Context.Current == null;
			var success = TryConfigureClient(uri, passwordParams, currentPassword);
			var result = OperationResult.Failed;

			if (!success.HasValue || (success == true && isFirstSession && AbortAfterClientConfiguration()))
			{
				result = OperationResult.Aborted;
			}
			else if (success == true)
			{
				result = OperationResult.Success;
			}

			return result;
		}

		private void HandleStartUrlQuery(Uri uri, UriSource source)
		{
			if (source == UriSource.Reconfiguration && Uri.TryCreate(Context.ReconfigurationUrl, UriKind.Absolute, out var reconfigurationUri))
			{
				uri = reconfigurationUri;
			}

			if (uri != default(Uri) && uri.Query.LastIndexOf('?') > 0)
			{
				Context.Next.Settings.Browser.StartUrlQuery = uri.Query.Substring(uri.Query.LastIndexOf('?'));
			}
		}

		private bool? TryConfigureClient(Uri uri, PasswordParameters passwordParams, string currentPassword = default(string))
		{
			var mustAuthenticate = IsRequiredToAuthenticateForClientConfiguration(passwordParams, currentPassword);

			logger.Info("Starting client configuration...");

			if (mustAuthenticate)
			{
				var authenticated = AuthenticateForClientConfiguration(currentPassword);

				if (authenticated == true)
				{
					logger.Info("Authentication was successful.");
				}

				if (authenticated == false)
				{
					logger.Info("Authentication has failed!");
					ActionRequired?.Invoke(new InvalidPasswordMessageArgs());

					return false;
				}

				if (!authenticated.HasValue)
				{
					logger.Info("Authentication was aborted.");

					return null;
				}
			}
			else
			{
				logger.Info("Authentication is not required.");
			}

			var status = configuration.ConfigureClientWith(uri, passwordParams);
			var success = status == SaveStatus.Success;

			if (success)
			{
				logger.Info("Client configuration was successful.");
			}
			else
			{
				logger.Error($"Client configuration failed with status '{status}'!");
				ActionRequired?.Invoke(new ClientConfigurationErrorMessageArgs());
			}

			return success;
		}

		private bool IsRequiredToAuthenticateForClientConfiguration(PasswordParameters passwordParams, string currentPassword = default(string))
		{
			var mustAuthenticate = currentPassword != default(string);

			if (mustAuthenticate)
			{
				var nextPassword = Context.Next.Settings.Security.AdminPasswordHash;
				var hasSettingsPassword = passwordParams.Password != null;
				var sameAdminPassword = currentPassword.Equals(nextPassword, StringComparison.OrdinalIgnoreCase);

				if (sameAdminPassword)
				{
					mustAuthenticate = false;
				}
				else if (hasSettingsPassword)
				{
					var settingsPassword = passwordParams.IsHash ? passwordParams.Password : hashAlgorithm.GenerateHashFor(passwordParams.Password);
					var knowsAdminPassword = currentPassword.Equals(settingsPassword, StringComparison.OrdinalIgnoreCase);

					mustAuthenticate = !knowsAdminPassword;
				}
			}

			return mustAuthenticate;
		}

		private bool? AuthenticateForClientConfiguration(string currentPassword)
		{
			var authenticated = false;

			for (int attempts = 0; attempts < 5 && !authenticated; attempts++)
			{
				var success = TryGetPassword(PasswordRequestPurpose.LocalAdministrator, out var password);

				if (success)
				{
					authenticated = currentPassword.Equals(hashAlgorithm.GenerateHashFor(password), StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					return null;
				}
			}

			return authenticated;
		}

		private bool AbortAfterClientConfiguration()
		{
			var args = new ConfigurationCompletedEventArgs();

			ActionRequired?.Invoke(args);
			logger.Info($"The user chose to {(args.AbortStartup ? "abort" : "continue")} startup after successful client configuration.");

			return args.AbortStartup;
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

		private bool TryInitializeSettingsUri(out Uri uri, out UriSource source)
		{
			var isValidUri = false;

			uri = null;
			source = default(UriSource);

			if (commandLineArgs?.Length > 1)
			{
				isValidUri = Uri.TryCreate(commandLineArgs[1], UriKind.Absolute, out uri);
				source = UriSource.CommandLine;
				logger.Info($"Found command-line argument for configuration resource: '{uri}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(ProgramDataFilePath))
			{
				isValidUri = Uri.TryCreate(ProgramDataFilePath, UriKind.Absolute, out uri);
				source = UriSource.ProgramData;
				logger.Info($"Found configuration file in program data directory: '{uri}'.");
			}

			if (!isValidUri && File.Exists(AppDataFilePath))
			{
				isValidUri = Uri.TryCreate(AppDataFilePath, UriKind.Absolute, out uri);
				source = UriSource.AppData;
				logger.Info($"Found configuration file in app data directory: '{uri}'.");
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
