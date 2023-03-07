/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Runtime.Operations
{
	internal abstract class ConfigurationBaseOperation : SessionOperation
	{
		protected string[] commandLineArgs;
		protected IConfigurationRepository configuration;

		protected string AppDataFilePath => Context.Next.AppConfig.AppDataFilePath;
		protected string ProgramDataFilePath => Context.Next.AppConfig.ProgramDataFilePath;

		public ConfigurationBaseOperation(string[] commandLineArgs, IConfigurationRepository configuration, SessionContext context) : base(context)
		{
			this.commandLineArgs = commandLineArgs;
			this.configuration = configuration;
		}

		protected abstract void InvokeActionRequired(ActionRequiredEventArgs args);

		protected LoadStatus? TryLoadSettings(Uri uri, UriSource source, out PasswordParameters passwordParams, out AppSettings settings, string currentPassword = default(string))
		{
			passwordParams = new PasswordParameters { Password = string.Empty, IsHash = true };

			var status = configuration.TryLoadSettings(uri, out settings, passwordParams);

			if (status == LoadStatus.PasswordNeeded && currentPassword != default(string))
			{
				passwordParams.Password = currentPassword;
				passwordParams.IsHash = true;

				status = configuration.TryLoadSettings(uri, out settings, passwordParams);
			}

			for (int attempts = 0; attempts < 5 && status == LoadStatus.PasswordNeeded; attempts++)
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

				status = configuration.TryLoadSettings(uri, out settings, passwordParams);
			}

			return status;
		}

		protected bool TryGetPassword(PasswordRequestPurpose purpose, out string password)
		{
			var args = new PasswordRequiredEventArgs { Purpose = purpose };

			InvokeActionRequired(args);
			password = args.Password;

			return args.Success;
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
