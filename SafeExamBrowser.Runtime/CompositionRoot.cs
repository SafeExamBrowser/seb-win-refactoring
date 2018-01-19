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
		internal ILogger Logger { get; private set; }
		internal RuntimeInfo RuntimeInfo { get; private set; }
		internal IShutdownController ShutdownController { get; private set; }
		internal IStartupController StartupController { get; private set; }
		internal ISystemInfo SystemInfo { get; private set; }
		internal Queue<IOperation> StartupOperations { get; private set; }

		internal void BuildObjectGraph()
		{
			var args = Environment.GetCommandLineArgs();
			var nativeMethods = new NativeMethods();
			var settingsRepository = new SettingsRepository();
			var uiFactory = new UserInterfaceFactory();

			Logger = new Logger();
			RuntimeInfo = new RuntimeInfo();
			SystemInfo = new SystemInfo();

			InitializeRuntimeInfo();
			InitializeLogging();

			var text = new Text(Logger);
			var runtimeController = new RuntimeController(new ModuleLogger(Logger, typeof(RuntimeController)));

			ShutdownController = new ShutdownController(Logger, RuntimeInfo, text, uiFactory);
			StartupController = new StartupController(Logger, RuntimeInfo, SystemInfo, text, uiFactory);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new I18nOperation(Logger, text));
			StartupOperations.Enqueue(new ConfigurationOperation(Logger, runtimeController, RuntimeInfo, settingsRepository, text, uiFactory, args));
			//StartupOperations.Enqueue(new KioskModeOperation());
			StartupOperations.Enqueue(new RuntimeControllerOperation(runtimeController, Logger));
		}

		private void InitializeRuntimeInfo()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var executable = Assembly.GetEntryAssembly();
			var startTime = DateTime.Now;
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			RuntimeInfo.ApplicationStartTime = startTime;
			RuntimeInfo.AppDataFolder = appDataFolder;
			RuntimeInfo.BrowserCachePath = Path.Combine(appDataFolder, "Cache");
			RuntimeInfo.BrowserLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Browser.txt");
			RuntimeInfo.ClientLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Client.txt");
			RuntimeInfo.DefaultSettingsFileName = "SebClientSettings.seb";
			RuntimeInfo.ProgramCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			RuntimeInfo.ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser));
			RuntimeInfo.ProgramTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			RuntimeInfo.ProgramVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			RuntimeInfo.RuntimeLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.txt");
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), RuntimeInfo.RuntimeLogFile);

			logFileWriter.Initialize();
			Logger.Subscribe(logFileWriter);
		}
	}
}
