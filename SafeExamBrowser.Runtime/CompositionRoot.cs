/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Windows;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.Communication;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.Runtime.Behaviour;
using SafeExamBrowser.Runtime.Behaviour.Operations;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.UserInterface.Classic;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Runtime
{
	internal class CompositionRoot
	{
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private ISystemInfo systemInfo;

		internal IRuntimeController RuntimeController { get; private set; }

		internal void BuildObjectGraph()
		{
			var args = Environment.GetCommandLineArgs();
			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IOperation>();
			var nativeMethods = new NativeMethods();
			var configuration = new ConfigurationRepository();

			logger = new Logger();
			runtimeInfo = configuration.RuntimeInfo;
			systemInfo = new SystemInfo();

			InitializeLogging();

			var text = new Text(logger);
			var uiFactory = new UserInterfaceFactory(text);
			var runtimeHost = new RuntimeHost(runtimeInfo.RuntimeAddress, new ModuleLogger(logger, typeof(RuntimeHost)));
			var serviceProxy = new ServiceProxy(runtimeInfo.ServiceAddress, new ModuleLogger(logger, typeof(ServiceProxy)));

			bootstrapOperations.Enqueue(new I18nOperation(logger, text));
			bootstrapOperations.Enqueue(new CommunicationOperation(runtimeHost, logger));

			sessionOperations.Enqueue(new ConfigurationOperation(configuration, logger, runtimeInfo, text, uiFactory, args));
			sessionOperations.Enqueue(new ServiceOperation(configuration, logger, serviceProxy, text));
			sessionOperations.Enqueue(new KioskModeOperation(logger, configuration));

			var boostrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new OperationSequence(logger, sessionOperations);

			RuntimeController = new RuntimeController(configuration, logger, boostrapSequence, sessionSequence, runtimeHost, runtimeInfo, serviceProxy, Application.Current.Shutdown, uiFactory);
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
			logger?.Log($"# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), runtimeInfo.RuntimeLogFile);

			logFileWriter.Initialize();
			logger.Subscribe(logFileWriter);
		}
	}
}
