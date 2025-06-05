/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal abstract class ConfigurationBaseOperation : SessionOperation
	{
		protected readonly IConfigurationRepository repository;
		protected readonly IUserInterfaceFactory uiFactory;

		protected string AppDataFilePath => Context.Next.AppConfig.AppDataFilePath;
		protected string ProgramDataFilePath => Context.Next.AppConfig.ProgramDataFilePath;

		public ConfigurationBaseOperation(
			Dependencies dependencies,
			IConfigurationRepository repository,
			IUserInterfaceFactory uiFactory) : base(dependencies)
		{
			this.repository = repository;
			this.uiFactory = uiFactory;
		}

		protected LoadStatus? TryLoadSettings(Uri uri, UriSource source, out PasswordParameters passwordParams, out AppSettings settings, string currentPassword = default)
		{
			passwordParams = new PasswordParameters { Password = string.Empty, IsHash = true };

			var status = repository.TryLoadSettings(uri, out settings, passwordParams);

			if (status == LoadStatus.PasswordNeeded && currentPassword != default)
			{
				passwordParams.Password = currentPassword;
				passwordParams.IsHash = true;

				status = repository.TryLoadSettings(uri, out settings, passwordParams);
			}

			for (var attempts = 0; attempts < 5 && status == LoadStatus.PasswordNeeded; attempts++)
			{
				var isLocalConfig = source == UriSource.AppData || source == UriSource.ProgramData;
				var purpose = isLocalConfig ? PasswordRequestPurpose.LocalSettings : PasswordRequestPurpose.Settings;
				var success = TryGetPassword(purpose, out var password);

				if (success)
				{
					passwordParams.Password = password;
					passwordParams.IsHash = false;
				}
				else
				{
					return null;
				}

				status = repository.TryLoadSettings(uri, out settings, passwordParams);
			}

			return status;
		}

		protected bool TryGetPassword(PasswordRequestPurpose purpose, out string password)
		{
			var success = false;

			if (ClientBridge.IsRequired())
			{
				ClientBridge.TryGetPassword(purpose, out password);
			}
			else
			{
				var (message, title) = GetTextKeysFor(purpose);
				var dialog = uiFactory.CreatePasswordDialog(Text.Get(message), Text.Get(title));
				var result = dialog.Show(RuntimeWindow);

				password = result.Password;
				success = result.Success;
			}

			return success;
		}

		private (TextKey message, TextKey title) GetTextKeysFor(PasswordRequestPurpose purpose)
		{
			var message = default(TextKey);
			var title = default(TextKey);

			switch (purpose)
			{
				case PasswordRequestPurpose.LocalAdministrator:
					message = TextKey.PasswordDialog_LocalAdminPasswordRequired;
					title = TextKey.PasswordDialog_LocalAdminPasswordRequiredTitle;
					break;
				case PasswordRequestPurpose.LocalSettings:
					message = TextKey.PasswordDialog_LocalSettingsPasswordRequired;
					title = TextKey.PasswordDialog_LocalSettingsPasswordRequiredTitle;
					break;
				case PasswordRequestPurpose.Settings:
					message = TextKey.PasswordDialog_SettingsPasswordRequired;
					title = TextKey.PasswordDialog_SettingsPasswordRequiredTitle;
					break;
			}

			return (message, title);
		}

		protected enum UriSource
		{
			Undefined,
			AppData,
			CommandLine,
			ProgramData,
			Reconfiguration,
			Server
		}
	}
}
