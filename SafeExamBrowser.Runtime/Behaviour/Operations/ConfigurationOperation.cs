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
		private RuntimeInfo runtimeInfo;
		private IText text;
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

			Settings settings;
			var isValidUri = TryGetSettingsUri(out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Loading configuration from '{uri.AbsolutePath}'...");
				settings = repository.LoadSettings(uri);

				if (settings.ConfigurationMode == ConfigurationMode.ConfigureClient)
				{
					var abort = IsConfigurationSufficient();

					logger.Info($"The user chose to {(abort ? "abort" : "continue")} after successful client configuration.");

					if (abort)
					{
						return OperationResult.Aborted;
					}
				}
			}
			else
			{
				logger.Info("No valid settings file specified nor found in PROGRAMDATA or APPDATA - loading default settings...");
				settings = repository.LoadDefaultSettings();
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			// TODO: How will the new settings be retrieved? Uri passed to the repository? If yes, how does the Uri get here?!
			//		-> IDEA: Use configuration repository as container?
			//		-> IDEA: Introduce IRepeatParams or alike?

			return OperationResult.Success;
		}

		public void Revert()
		{
			// Nothing to do here...
		}

		private bool TryGetSettingsUri(out Uri uri)
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

		private bool IsConfigurationSufficient()
		{
			var message = text.Get(TextKey.MessageBox_ClientConfigurationQuestion);
			var title = text.Get(TextKey.MessageBox_ClientConfigurationQuestionTitle);
			var abort = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question);

			return abort == MessageBoxResult.Yes;
		}
	}
}
