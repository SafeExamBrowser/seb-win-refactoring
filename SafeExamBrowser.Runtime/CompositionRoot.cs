/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Behaviour.OperationModel;
using SafeExamBrowser.Core.Communication.Hosts;
using SafeExamBrowser.Core.Communication.Proxies;
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
		private AppConfig appConfig;
		private ILogger logger;
		private ISystemInfo systemInfo;

		internal IRuntimeController RuntimeController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			const int TEN_SECONDS = 10000;

			var args = Environment.GetCommandLineArgs();
			var configuration = new ConfigurationRepository();
			var nativeMethods = new NativeMethods();

			logger = new Logger();
			appConfig = configuration.AppConfig;
			systemInfo = new SystemInfo();

			InitializeLogging();

			var text = new Text(logger);
			var messageBox = new MessageBox(text);
			var uiFactory = new UserInterfaceFactory(text);
			var desktop = new Desktop(new ModuleLogger(logger, typeof(Desktop)));
			var processFactory = new ProcessFactory(desktop, new ModuleLogger(logger, typeof(ProcessFactory)));
			var proxyFactory = new ProxyFactory(new ProxyObjectFactory(), logger);
			var resourceLoader = new ResourceLoader();
			var runtimeHost = new RuntimeHost(appConfig.RuntimeAddress, configuration, new HostObjectFactory(), new ModuleLogger(logger, typeof(RuntimeHost)));
			var serviceProxy = new ServiceProxy(appConfig.ServiceAddress, new ProxyObjectFactory(), new ModuleLogger(logger, typeof(ServiceProxy)));

			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IOperation>();

			bootstrapOperations.Enqueue(new I18nOperation(logger, text));
			bootstrapOperations.Enqueue(new CommunicationOperation(runtimeHost, logger));

			sessionOperations.Enqueue(new ConfigurationOperation(appConfig, configuration, logger, messageBox, resourceLoader, runtimeHost, text, uiFactory, args));
			sessionOperations.Enqueue(new SessionInitializationOperation(configuration, logger, runtimeHost));
			sessionOperations.Enqueue(new ServiceOperation(configuration, logger, serviceProxy, text));
			sessionOperations.Enqueue(new ClientTerminationOperation(configuration, logger, processFactory, proxyFactory, runtimeHost, TEN_SECONDS));
			sessionOperations.Enqueue(new KioskModeOperation(logger, configuration));
			sessionOperations.Enqueue(new ClientOperation(configuration, logger, processFactory, proxyFactory, runtimeHost, TEN_SECONDS));

			var bootstrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new OperationSequence(logger, sessionOperations);

			RuntimeController = new RuntimeController(appConfig, configuration, logger, messageBox, bootstrapSequence, sessionSequence, runtimeHost, serviceProxy, shutdown, uiFactory);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"/* {appConfig.ProgramTitle}, Version {appConfig.ProgramVersion}");
			logger.Log($"/* {appConfig.ProgramCopyright}");
			logger.Log($"/* ");
			logger.Log($"/* Please visit https://www.github.com/SafeExamBrowser for more information.");
			logger.Log(string.Empty);
			logger.Log($"# Application started at {appConfig.ApplicationStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log($"# Running on {systemInfo.OperatingSystemInfo}");
			logger.Log($"# Runtime-ID: {appConfig.RuntimeId}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), appConfig.RuntimeLogFile);

			logFileWriter.Initialize();
			logger.Subscribe(logFileWriter);
		}
	}
}
