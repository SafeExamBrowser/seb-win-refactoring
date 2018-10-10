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
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.UserInterface.Classic;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Runtime
{
	internal class CompositionRoot
	{
		private AppConfig appConfig;
		private ILogger logger;
		private ISystemInfo systemInfo;
		private IText text;
		private ITextResource textResource;

		internal IRuntimeController RuntimeController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			const int FIVE_SECONDS = 5000;
			const int FIFTEEN_SECONDS = 15000;

			var args = Environment.GetCommandLineArgs();
			var configuration = BuildConfigurationRepository();
			var nativeMethods = new NativeMethods();

			logger = new Logger();
			appConfig = configuration.AppConfig;
			systemInfo = new SystemInfo();

			InitializeLogging();
			InitializeText();

			var messageBox = new MessageBox(text);
			var desktopFactory = new DesktopFactory(new ModuleLogger(logger, nameof(DesktopFactory)));
			var explorerShell = new ExplorerShell(new ModuleLogger(logger, nameof(ExplorerShell)), nativeMethods);
			var processFactory = new ProcessFactory(new ModuleLogger(logger, nameof(ProcessFactory)));
			var proxyFactory = new ProxyFactory(new ProxyObjectFactory(), logger);
			var resourceLoader = new ResourceLoader();
			var runtimeHost = new RuntimeHost(appConfig.RuntimeAddress, configuration, new HostObjectFactory(), new ModuleLogger(logger, nameof(RuntimeHost)), FIVE_SECONDS);
			var serviceProxy = new ServiceProxy(appConfig.ServiceAddress, new ProxyObjectFactory(), new ModuleLogger(logger, nameof(ServiceProxy)));
			var uiFactory = new UserInterfaceFactory(text);

			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IRepeatableOperation>();

			bootstrapOperations.Enqueue(new I18nOperation(logger, text, textResource));
			bootstrapOperations.Enqueue(new CommunicationHostOperation(runtimeHost, logger));

			sessionOperations.Enqueue(new ConfigurationOperation(appConfig, configuration, logger, resourceLoader, args));
			sessionOperations.Enqueue(new ClientTerminationOperation(configuration, logger, processFactory, proxyFactory, runtimeHost, FIFTEEN_SECONDS));
			sessionOperations.Enqueue(new KioskModeTerminationOperation(configuration, desktopFactory, explorerShell, logger, processFactory));
			sessionOperations.Enqueue(new SessionInitializationOperation(configuration, logger, runtimeHost));
			sessionOperations.Enqueue(new ServiceOperation(configuration, logger, serviceProxy));
			sessionOperations.Enqueue(new KioskModeOperation(configuration, desktopFactory, explorerShell, logger, processFactory));
			sessionOperations.Enqueue(new ClientOperation(configuration, logger, processFactory, proxyFactory, runtimeHost, FIFTEEN_SECONDS));

			var bootstrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new RepeatableOperationSequence(logger, sessionOperations);

			RuntimeController = new RuntimeController(appConfig, configuration, logger, messageBox, bootstrapSequence, sessionSequence, runtimeHost, serviceProxy, shutdown, text, uiFactory);
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

		private ConfigurationRepository BuildConfigurationRepository()
		{
			var executable = Assembly.GetExecutingAssembly();
			var programCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			var programTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			var programVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			var repository = new ConfigurationRepository(executable.Location, programCopyright, programTitle, programVersion);

			return repository;
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), appConfig.RuntimeLogFile);

			logFileWriter.Initialize();
			logger.Subscribe(logFileWriter);
		}

		private void InitializeText()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text.xml";

			text = new Text(logger);
			textResource = new XmlTextResource(path);
		}
	}
}
