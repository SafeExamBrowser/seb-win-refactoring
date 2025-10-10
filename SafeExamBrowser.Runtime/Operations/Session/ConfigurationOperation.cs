/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class ConfigurationOperation : ConfigurationBaseOperation
	{
		private readonly string[] commandLineArgs;
		private readonly IFileSystem fileSystem;
		private readonly IHashAlgorithm hashAlgorithm;

		public override event StatusChangedEventHandler StatusChanged;

		public ConfigurationOperation(
			string[] commandLineArgs,
			Dependencies dependencies,
			IFileSystem fileSystem,
			IHashAlgorithm hashAlgorithm,
			IConfigurationRepository repository,
			IUserInterfaceFactory uiFactory) : base(dependencies, repository, uiFactory)
		{
			this.commandLineArgs = commandLineArgs;
			this.fileSystem = fileSystem;
			this.hashAlgorithm = hashAlgorithm;
		}

		public override OperationResult Perform()
		{
			Logger.Info("Initializing application configuration...");
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
			Logger.Info("Initializing new application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

			var result = OperationResult.Failed;
			var isValidUri = TryValidateSettingsUri(Context.ReconfigurationFilePath, out var uri);

			if (isValidUri)
			{
				result = LoadSettingsForReconfiguration(uri);
			}
			else
			{
				Logger.Warn($"The resource specified for reconfiguration does not exist or is not valid!");
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
			Logger.Info("No valid configuration resource specified and no local client configuration found - loading default settings...");
			Context.Next.Settings = repository.LoadDefaultSettings();

			return OperationResult.Success;
		}

		private OperationResult LoadSettingsForStartup(Uri uri, UriSource source)
		{
			var currentPassword = default(string);
			var passwordParams = default(PasswordParameters);
			var result = OperationResult.Aborted;
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
				result = DetermineLoadResult(uri, source, settings, status.Value, passwordParams, currentPassword);
			}

			return result;
		}

		private OperationResult LoadSettingsForReconfiguration(Uri uri)
		{
			var currentPassword = Context.Current.Settings.Security.AdminPasswordHash;
			var result = OperationResult.Aborted;
			var source = UriSource.Reconfiguration;
			var status = TryLoadSettings(uri, source, out var passwordParams, out var settings, currentPassword);

			if (status.HasValue)
			{
				result = DetermineLoadResult(uri, source, settings, status.Value, passwordParams, currentPassword);
			}

			if (result == OperationResult.Success && Context.Current.IsBrowserResource)
			{
				HandleReconfigurationByBrowserResource();
			}

			fileSystem.Delete(uri.LocalPath);
			Logger.Info($"Deleted temporary configuration file '{uri}'.");

			return result;
		}

		private OperationResult DetermineLoadResult(Uri uri, UriSource source, AppSettings settings, LoadStatus status, PasswordParameters passwordParams, string currentPassword = default)
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
			Context.Next.IsBrowserResource = true;
			Context.Next.Settings.Applications.Blacklist.Clear();
			Context.Next.Settings.Applications.Whitelist.Clear();
			Context.Next.Settings.Browser.DeleteCacheOnShutdown = false;
			Context.Next.Settings.Browser.DeleteCookiesOnShutdown = false;
			Context.Next.Settings.Browser.StartUrl = uri.AbsoluteUri;
			Context.Next.Settings.Display.AllowedDisplays = 10;
			Context.Next.Settings.Display.IgnoreError = true;
			Context.Next.Settings.Display.InternalDisplayOnly = false;
			Context.Next.Settings.Security.AllowReconfiguration = true;
			Context.Next.Settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Allow;
			Context.Next.Settings.Service.IgnoreService = true;
			Context.Next.Settings.UserInterface.ActionCenter.EnableActionCenter = false;

			Logger.Info($"The configuration resource needs authentication or is a webpage, using '{uri}' as start URL for the browser.");

			return OperationResult.Success;
		}

		private OperationResult HandleClientConfiguration(Uri uri, PasswordParameters passwordParams, string currentPassword = default)
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

		private void HandleReconfigurationByBrowserResource()
		{
			Context.Next.Settings.Browser.DeleteCookiesOnStartup = false;
			Logger.Info("Some browser settings were overridden in order to retain a potential LMS / web application session.");
		}

		private void HandleStartUrlQuery(Uri uri, UriSource source)
		{
			if (source == UriSource.Reconfiguration && Uri.TryCreate(Context.ReconfigurationUrl, UriKind.Absolute, out var reconfigurationUri))
			{
				uri = reconfigurationUri;
			}

			if (uri != default && uri.Query.LastIndexOf('?') > 0)
			{
				Context.Next.Settings.Browser.StartUrlQuery = uri.Query.Substring(uri.Query.LastIndexOf('?'));
			}
		}

		private bool? TryConfigureClient(Uri uri, PasswordParameters passwordParams, string currentPassword = default)
		{
			var mustAuthenticate = IsRequiredToAuthenticateForClientConfiguration(passwordParams, currentPassword);
			var success = new bool?(true);

			Logger.Info("Starting client configuration...");

			if (mustAuthenticate)
			{
				success = AuthenticateForClientConfiguration(currentPassword);
			}
			else
			{
				Logger.Info("Authentication is not required.");
			}

			if (success == true)
			{
				var status = repository.ConfigureClientWith(uri, passwordParams);

				success = status == SaveStatus.Success;

				if (success == true)
				{
					Logger.Info("Client configuration was successful.");
				}
				else
				{
					Logger.Error($"Client configuration failed with status '{status}'!");
					ShowMessageBox(TextKey.MessageBox_ClientConfigurationError, TextKey.MessageBox_ClientConfigurationErrorTitle, icon: MessageBoxIcon.Error);
				}
			}

			return success;
		}

		private bool IsRequiredToAuthenticateForClientConfiguration(PasswordParameters passwordParams, string currentPassword = default)
		{
			var mustAuthenticate = currentPassword != default;

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
			var authenticated = default(bool?);

			for (var attempts = 0; attempts < 5 && authenticated != true; attempts++)
			{
				var success = TryGetPassword(PasswordRequestPurpose.LocalAdministrator, out var password);

				if (success)
				{
					authenticated = currentPassword.Equals(hashAlgorithm.GenerateHashFor(password), StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					authenticated = default;
					break;
				}
			}

			if (authenticated == true)
			{
				Logger.Info("Authentication was successful.");
			}

			if (authenticated == false)
			{
				Logger.Info("Authentication has failed!");
				ShowMessageBox(TextKey.MessageBox_InvalidPasswordError, TextKey.MessageBox_InvalidPasswordErrorTitle, icon: MessageBoxIcon.Error);
			}

			if (authenticated == default)
			{
				Logger.Info("Authentication was aborted.");
			}

			return authenticated;
		}

		private bool AbortAfterClientConfiguration()
		{
			var message = TextKey.MessageBox_ClientConfigurationQuestion;
			var title = TextKey.MessageBox_ClientConfigurationQuestionTitle;
			var result = ShowMessageBox(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question);
			var abort = result == MessageBoxResult.Yes;

			Logger.Info($"The user chose to {(abort ? "abort" : "continue")} startup after successful client configuration.");

			return abort;
		}

		private void ShowFailureMessage(LoadStatus status, Uri uri)
		{
			var error = MessageBoxIcon.Error;
			var message = default(TextKey);
			var placeholders = new Dictionary<string, string>();
			var title = default(TextKey);

			switch (status)
			{
				case LoadStatus.PasswordNeeded:
					message = TextKey.MessageBox_InvalidPasswordError;
					title = TextKey.MessageBox_InvalidPasswordErrorTitle;
					break;
				case LoadStatus.InvalidData:
					message = TextKey.MessageBox_InvalidConfigurationData;
					placeholders["%%URI%%"] = uri.ToString();
					title = TextKey.MessageBox_InvalidConfigurationDataTitle;
					break;
				case LoadStatus.NotSupported:
					message = TextKey.MessageBox_NotSupportedConfigurationResource;
					placeholders["%%URI%%"] = uri.ToString();
					title = TextKey.MessageBox_NotSupportedConfigurationResourceTitle;
					break;
				case LoadStatus.UnexpectedError:
					message = TextKey.MessageBox_UnexpectedConfigurationError;
					placeholders["%%URI%%"] = uri.ToString();
					title = TextKey.MessageBox_UnexpectedConfigurationErrorTitle;
					break;
			}

			ShowMessageBox(message, title, icon: error, messagePlaceholders: placeholders);
		}

		private bool TryInitializeSettingsUri(out Uri uri, out UriSource source)
		{
			var isValidUri = false;

			uri = default;
			source = default;

			if (commandLineArgs?.Length > 1)
			{
				isValidUri = Uri.TryCreate(commandLineArgs[1], UriKind.Absolute, out uri);
				source = UriSource.CommandLine;
				Logger.Info($"Found command-line argument for configuration resource: '{uri}', the URI is {(isValidUri ? "valid" : "invalid")}.");
			}

			if (!isValidUri && File.Exists(ProgramDataFilePath))
			{
				isValidUri = Uri.TryCreate(ProgramDataFilePath, UriKind.Absolute, out uri);
				source = UriSource.ProgramData;
				Logger.Info($"Found configuration file in program data directory: '{uri}'.");
			}

			if (!isValidUri && File.Exists(AppDataFilePath))
			{
				isValidUri = Uri.TryCreate(AppDataFilePath, UriKind.Absolute, out uri);
				source = UriSource.AppData;
				Logger.Info($"Found configuration file in app data directory: '{uri}'.");
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
					Logger.Info("The configuration was aborted by the user.");
					break;
				case OperationResult.Failed:
					Logger.Warn("The configuration has failed!");
					break;
				case OperationResult.Success:
					Logger.Info("The configuration was successful.");
					break;
			}
		}
	}
}
