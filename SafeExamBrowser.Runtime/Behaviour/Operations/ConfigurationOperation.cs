/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ConfigurationOperation : IOperation
	{
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private ISettingsRepository repository;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private string[] commandLineArgs;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public ConfigurationOperation(
			ILogger logger,
			IRuntimeInfo runtimeInfo,
			ISettingsRepository repository,
			IText text,
			IUserInterfaceFactory uiFactory,
			string[] commandLineArgs)
		{
			this.logger = logger;
			this.commandLineArgs = commandLineArgs;
			this.repository = repository;
			this.runtimeInfo = runtimeInfo;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void Perform()
		{
			logger.Info("Initializing application configuration...");
			ProgressIndicator?.UpdateText(TextKey.SplashScreen_InitializeConfiguration);

			ISettings settings;
			var isValidUri = TryGetSettingsUri(out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Loading configuration from '{uri.AbsolutePath}'...");
				settings = repository.Load(uri);

				if (settings.ConfigurationMode == ConfigurationMode.ConfigureClient && UserWantsToAbortStartup())
				{
					Abort = true;
					logger.Info($"The user chose to {(Abort ? "abort" : "continue")} the application startup after successful client configuration.");
				}
			}
			else
			{
				logger.Info("No valid settings file specified nor found in PROGRAMDATA or APPDATA - loading default settings...");
				settings = repository.LoadDefaults();
			}
		}

		public void Repeat()
		{
			// TODO: How will the new settings be retrieved? Uri passed to the repository? If yes, how does the Uri get here?!
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

		private bool UserWantsToAbortStartup()
		{
			var message = text.Get(TextKey.MessageBox_ConfigureClientSuccess);
			var title = text.Get(TextKey.MessageBox_ConfigureClientSuccessTitle);
			var abort = uiFactory.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question);

			return abort == MessageBoxResult.Yes;
		}
	}
}
