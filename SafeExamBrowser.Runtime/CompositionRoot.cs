/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Configuration.DataCompression;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Configuration.DataResources;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Runtime;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.UserInterface.Desktop;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Runtime
{
	internal class CompositionRoot
	{
		private AppConfig appConfig;
		private IConfigurationRepository configuration;
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
			var nativeMethods = new NativeMethods();

			logger = new Logger();
			systemInfo = new SystemInfo();

			InitializeConfiguration();
			InitializeLogging();
			InitializeText();

			var messageBox = new MessageBox(text);
			var desktopFactory = new DesktopFactory(ModuleLogger(nameof(DesktopFactory)));
			var explorerShell = new ExplorerShell(ModuleLogger(nameof(ExplorerShell)), nativeMethods);
			var processFactory = new ProcessFactory(ModuleLogger(nameof(ProcessFactory)));
			var proxyFactory = new ProxyFactory(new ProxyObjectFactory(), logger);
			var runtimeHost = new RuntimeHost(appConfig.RuntimeAddress, new HostObjectFactory(), ModuleLogger(nameof(RuntimeHost)), FIVE_SECONDS);
			var serviceProxy = new ServiceProxy(appConfig.ServiceAddress, new ProxyObjectFactory(), ModuleLogger(nameof(ServiceProxy)));
			var sessionContext = new SessionContext();
			var uiFactory = new UserInterfaceFactory(text);

			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IRepeatableOperation>();

			bootstrapOperations.Enqueue(new I18nOperation(logger, text, textResource));
			bootstrapOperations.Enqueue(new CommunicationHostOperation(runtimeHost, logger));

			sessionOperations.Enqueue(new SessionInitializationOperation(configuration, logger, runtimeHost, sessionContext));
			sessionOperations.Enqueue(new ConfigurationOperation(args, configuration, new HashAlgorithm(), logger, sessionContext));
			sessionOperations.Enqueue(new ClientTerminationOperation(logger, processFactory, proxyFactory, runtimeHost, sessionContext, FIFTEEN_SECONDS));
			sessionOperations.Enqueue(new KioskModeTerminationOperation(desktopFactory, explorerShell, logger, processFactory, sessionContext));
			sessionOperations.Enqueue(new ServiceOperation(logger, serviceProxy, sessionContext));
			sessionOperations.Enqueue(new KioskModeOperation(desktopFactory, explorerShell, logger, processFactory, sessionContext));
			sessionOperations.Enqueue(new ClientOperation(logger, processFactory, proxyFactory, runtimeHost, sessionContext, FIFTEEN_SECONDS));
			sessionOperations.Enqueue(new SessionActivationOperation(logger, sessionContext));

			var bootstrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new RepeatableOperationSequence(logger, sessionOperations);

			RuntimeController = new RuntimeController(appConfig, logger, messageBox, bootstrapSequence, sessionSequence, runtimeHost, serviceProxy, sessionContext, shutdown, text, uiFactory);
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

		private void InitializeConfiguration()
		{
			var executable = Assembly.GetExecutingAssembly();
			var programCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			var programTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			var programVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			var compressor = new GZipCompressor(ModuleLogger(nameof(GZipCompressor)));
			var passwordEncryption = new PasswordEncryption(ModuleLogger(nameof(PasswordEncryption)));
			var publicKeyEncryption = new PublicKeyEncryption(new CertificateStore(), ModuleLogger(nameof(PublicKeyEncryption)));
			var symmetricInnerEncryption = new PasswordEncryption(ModuleLogger(nameof(PasswordEncryption)));
			var symmetricEncryption = new PublicKeySymmetricEncryption(new CertificateStore(), ModuleLogger(nameof(PublicKeySymmetricEncryption)), symmetricInnerEncryption);
			var repositoryLogger = ModuleLogger(nameof(ConfigurationRepository));
			var xmlParser = new XmlParser(ModuleLogger(nameof(XmlParser)));

			configuration = new ConfigurationRepository(new HashAlgorithm(), repositoryLogger, executable.Location, programCopyright, programTitle, programVersion);
			appConfig = configuration.InitializeAppConfig();

			configuration.Register(new BinaryParser(compressor, new HashAlgorithm(), ModuleLogger(nameof(BinaryParser)), passwordEncryption, publicKeyEncryption, symmetricEncryption, xmlParser));
			configuration.Register(new BinarySerializer(compressor, ModuleLogger(nameof(BinarySerializer))));
			configuration.Register(new XmlParser(ModuleLogger(nameof(XmlParser))));
			configuration.Register(new XmlSerializer(ModuleLogger(nameof(XmlSerializer))));
			configuration.Register(new FileResourceLoader(ModuleLogger(nameof(FileResourceLoader))));
			configuration.Register(new FileResourceSaver(ModuleLogger(nameof(FileResourceSaver))));
			configuration.Register(new NetworkResourceLoader(appConfig, new ModuleLogger(logger, nameof(NetworkResourceLoader))));
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), appConfig.RuntimeLogFile);

			logFileWriter.Initialize();
			logger.LogLevel = LogLevel.Debug;
			logger.Subscribe(logFileWriter);
		}

		private void InitializeText()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text.xml";

			text = new Text(logger);
			textResource = new XmlTextResource(path);
		}

		private IModuleLogger ModuleLogger(string moduleInfo)
		{
			return new ModuleLogger(logger, moduleInfo);
		}
	}
}
