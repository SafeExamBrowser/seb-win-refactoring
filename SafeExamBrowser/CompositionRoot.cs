/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */


using System.Collections.Generic;
using System.IO;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Configuration.Settings;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Runtime;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.UserInterface.Classic;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		private ILogger logger;
		private INativeMethods nativeMethods;
		private IRuntimeController runtimeController;
		private ISettings settings;
		private ISystemInfo systemInfo;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		internal IShutdownController ShutdownController { get; private set; }
		internal IStartupController StartupController { get; private set; }
		internal Queue<IOperation> StartupOperations { get; private set; }
		internal SplashScreen SplashScreen { get; private set; }

		internal void BuildObjectGraph()
		{
			nativeMethods = new NativeMethods();
			settings = new SettingsRepository().LoadDefaults();
			systemInfo = new SystemInfo();
			uiFactory = new UserInterfaceFactory();

			InitializeLogger();

			text = new Text(logger);

			runtimeController = new RuntimeController(new ModuleLogger(logger, typeof(RuntimeController)));
			ShutdownController = new ShutdownController(logger, settings, text, uiFactory);
			StartupController = new StartupController(logger, settings, systemInfo, text, uiFactory);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new I18nOperation(logger, text));
			StartupOperations.Enqueue(new RuntimeControllerOperation(runtimeController, logger));
		}

		private void InitializeLogger()
		{
			var logFolder = Path.GetDirectoryName(settings.Logging.RuntimeLogFile);

			if (!Directory.Exists(logFolder))
			{
				Directory.CreateDirectory(logFolder);
			}

			logger = new Logger();
			logger.Subscribe(new LogFileWriter(new DefaultLogFormatter(), settings.Logging.RuntimeLogFile));
		}
	}
}
