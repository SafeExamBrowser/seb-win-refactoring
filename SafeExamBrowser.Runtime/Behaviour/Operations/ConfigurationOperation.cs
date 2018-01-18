/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Runtime;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ConfigurationOperation : IOperation
	{
		private ILogger logger;
		private IRuntimeController controller;
		private IRuntimeInfo runtimeInfo;
		private ISettingsRepository repository;
		private string[] commandLineArgs;

		public ISplashScreen SplashScreen { private get; set; }

		public ConfigurationOperation(
			ILogger logger,
			IRuntimeController controller,
			IRuntimeInfo runtimeInfo,
			ISettingsRepository repository,
			string[] commandLineArgs)
		{
			this.logger = logger;
			this.controller = controller;
			this.commandLineArgs = commandLineArgs;
			this.repository = repository;
			this.runtimeInfo = runtimeInfo;
		}

		public void Perform()
		{
			logger.Info("Initializing application configuration...");
			SplashScreen.UpdateText(TextKey.SplashScreen_InitializeConfiguration);

			var isValidUri = TryGetSettingsUri(out Uri uri);

			if (isValidUri)
			{
				logger.Info($"Loading configuration from '{uri.AbsolutePath}'...");
				controller.Settings = repository.Load(uri);
			}
			else
			{
				logger.Info("No valid settings file specified nor found in PROGRAMDATA or APPDATA - loading default settings...");
				controller.Settings = repository.LoadDefaults();
			}

			// TODO: Allow user to quit if in Configure Client mode - callback to terminate WPF application?
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
	}
}
