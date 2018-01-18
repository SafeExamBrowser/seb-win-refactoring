/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Configuration.Settings;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.Runtime.Behaviour;
using SafeExamBrowser.Runtime.Behaviour.Operations;
using SafeExamBrowser.UserInterface.Classic;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Runtime
{
	internal class CompositionRoot
	{
		internal IShutdownController ShutdownController { get; private set; }
		internal IStartupController StartupController { get; private set; }
		internal Queue<IOperation> StartupOperations { get; private set; }

		internal void BuildObjectGraph()
		{
			var args = Environment.GetCommandLineArgs();
			var logger = new Logger();
			var nativeMethods = new NativeMethods();
			var runtimeInfo = new RuntimeInfo();
			var settingsRepository = new SettingsRepository();
			var systemInfo = new SystemInfo();
			var uiFactory = new UserInterfaceFactory();

			Initialize(runtimeInfo);
			Initialize(logger, runtimeInfo);

			var text = new Text(logger);
			var runtimeController = new RuntimeController(new ModuleLogger(logger, typeof(RuntimeController)));

			ShutdownController = new ShutdownController(logger, runtimeInfo, text, uiFactory);
			StartupController = new StartupController(logger, runtimeInfo, systemInfo, text, uiFactory);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new I18nOperation(logger, text));
			StartupOperations.Enqueue(new ConfigurationOperation(logger, runtimeController, runtimeInfo, settingsRepository, args));
			//StartupOperations.Enqueue(new KioskModeOperation());
			StartupOperations.Enqueue(new RuntimeControllerOperation(runtimeController, logger));
		}

		private void Initialize(RuntimeInfo runtimeInfo)
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var executable = Assembly.GetEntryAssembly();
			var startTime = DateTime.Now;
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			runtimeInfo.ApplicationStartTime = startTime;
			runtimeInfo.AppDataFolder = appDataFolder;
			runtimeInfo.BrowserCachePath = Path.Combine(appDataFolder, "Cache");
			runtimeInfo.BrowserLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Browser.txt");
			runtimeInfo.ClientLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Client.txt");
			runtimeInfo.DefaultSettingsFileName = "SebClientSettings.seb";
			runtimeInfo.ProgramCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			runtimeInfo.ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser));
			runtimeInfo.ProgramTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			runtimeInfo.ProgramVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			runtimeInfo.RuntimeLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.txt");
		}

		private void Initialize(ILogger logger, IRuntimeInfo runtimeInfo)
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), runtimeInfo.RuntimeLogFile);

			logFileWriter.Initialize();
			logger.Subscribe(logFileWriter);
		}
	}
}
