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
using SafeExamBrowser.Core.Communication;
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
		private ILogger logger;
		private RuntimeInfo runtimeInfo;
		private ISystemInfo systemInfo;

		internal IRuntimeController RuntimeController { get; private set; }
		internal Queue<IOperation> StartupOperations { get; private set; }
		internal RuntimeWindow RuntimeWindow { get; private set; }

		internal void BuildObjectGraph()
		{
			var args = Environment.GetCommandLineArgs();
			var nativeMethods = new NativeMethods();
			var settingsRepository = new SettingsRepository();
			var uiFactory = new UserInterfaceFactory();

			logger = new Logger();
			runtimeInfo = new RuntimeInfo();
			systemInfo = new SystemInfo();

			InitializeRuntimeInfo();
			InitializeLogging();

			var text = new Text(logger);
			var serviceProxy = new ServiceProxy(new ModuleLogger(logger, typeof(ServiceProxy)), "net.pipe://localhost/safeexambrowser/service");
			var shutdownController = new ShutdownController(logger, runtimeInfo, text, uiFactory);
			var startupController = new StartupController(logger, runtimeInfo, systemInfo, text, uiFactory);

			RuntimeWindow = new RuntimeWindow(new DefaultLogFormatter(), runtimeInfo, text);
			RuntimeController = new RuntimeController(serviceProxy, new ModuleLogger(logger, typeof(RuntimeController)), RuntimeWindow, settingsRepository, shutdownController, startupController);

			logger.Subscribe(RuntimeWindow);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new I18nOperation(logger, text));
			StartupOperations.Enqueue(new ConfigurationOperation(logger, runtimeInfo, settingsRepository, text, uiFactory, args));
			StartupOperations.Enqueue(new ServiceOperation(logger, serviceProxy, settingsRepository, text));
			StartupOperations.Enqueue(new KioskModeOperation());
		}

		internal void LogStartupInformation()
		{
			var titleLine = $"/* {runtimeInfo.ProgramTitle}, Version {runtimeInfo.ProgramVersion}{Environment.NewLine}";
			var copyrightLine = $"/* {runtimeInfo.ProgramCopyright}{Environment.NewLine}";
			var emptyLine = $"/* {Environment.NewLine}";
			var githubLine = $"/* Please visit https://www.github.com/SafeExamBrowser for more information.";

			logger.Log($"{titleLine}{copyrightLine}{emptyLine}{githubLine}");
			logger.Log(string.Empty);
			logger.Log($"# Application started at {runtimeInfo.ApplicationStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log($"# Running on {systemInfo.OperatingSystemInfo}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"{Environment.NewLine}# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private void InitializeRuntimeInfo()
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

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), runtimeInfo.RuntimeLogFile);

			logFileWriter.Initialize();
			logger.Subscribe(logFileWriter);
		}
	}
}
