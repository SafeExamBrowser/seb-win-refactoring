/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Configuration.DataCompression;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Configuration.DataResources;
using SafeExamBrowser.Configuration.Integrity;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations;
using SafeExamBrowser.Server;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Registry;
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

		internal RuntimeController RuntimeController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			const int FIVE_SECONDS = 5000;
			const int THIRTY_SECONDS = 30000;

			logger = new Logger();
			systemInfo = new SystemInfo();

			InitializeConfiguration();
			InitializeLogging();
			InitializeText();

			var nativeMethods = new NativeMethods();
			var uiFactory = new UserInterfaceFactory(text);
			var userInfo = new UserInfo(ModuleLogger(nameof(UserInfo)));

			var args = Environment.GetCommandLineArgs();
			var integrityModule = new IntegrityModule(appConfig, ModuleLogger(nameof(IntegrityModule)));
			var desktopFactory = new DesktopFactory(ModuleLogger(nameof(DesktopFactory)));
			var desktopMonitor = new DesktopMonitor(ModuleLogger(nameof(DesktopMonitor)));
			var displayMonitor = new DisplayMonitor(ModuleLogger(nameof(DisplayMonitor)), nativeMethods, systemInfo);
			var explorerShell = new ExplorerShell(ModuleLogger(nameof(ExplorerShell)), nativeMethods);
			var fileSystem = new FileSystem();
			var keyGenerator = new KeyGenerator(appConfig, integrityModule, ModuleLogger(nameof(KeyGenerator)));
			var messageBox = new MessageBoxFactory(text);
			var processFactory = new ProcessFactory(ModuleLogger(nameof(ProcessFactory)));
			var proxyFactory = new ProxyFactory(new ProxyObjectFactory(), ModuleLogger(nameof(ProxyFactory)));
			var registry = new Registry(ModuleLogger(nameof(Registry)));
			var remoteSessionDetector = new RemoteSessionDetector(ModuleLogger(nameof(RemoteSessionDetector)));
			var runtimeHost = new RuntimeHost(appConfig.RuntimeAddress, new HostObjectFactory(), ModuleLogger(nameof(RuntimeHost)), FIVE_SECONDS);
			var runtimeWindow = uiFactory.CreateRuntimeWindow(appConfig);
			var server = new ServerProxy(appConfig, keyGenerator, ModuleLogger(nameof(ServerProxy)), systemInfo, userInfo);
			var serviceProxy = new ServiceProxy(appConfig.ServiceAddress, new ProxyObjectFactory(), ModuleLogger(nameof(ServiceProxy)), Interlocutor.Runtime);
			var sessionContext = new SessionContext();
			var splashScreen = uiFactory.CreateSplashScreen(appConfig);
			var vmDetector = new VirtualMachineDetector(ModuleLogger(nameof(VirtualMachineDetector)), systemInfo);

			var bootstrapOperations = new Queue<IOperation>();
			var sessionOperations = new Queue<IRepeatableOperation>();

			bootstrapOperations.Enqueue(new I18nOperation(logger, text));
			bootstrapOperations.Enqueue(new CommunicationHostOperation(runtimeHost, logger));
			bootstrapOperations.Enqueue(new ApplicationIntegrityOperation(integrityModule, logger));

			sessionOperations.Enqueue(new SessionInitializationOperation(configuration, fileSystem, logger, runtimeHost, sessionContext));
			sessionOperations.Enqueue(new ConfigurationOperation(args, configuration, new FileSystem(), new HashAlgorithm(), logger, sessionContext));
			sessionOperations.Enqueue(new ServerOperation(args, configuration, fileSystem, logger, sessionContext, server));
			sessionOperations.Enqueue(new DisclaimerOperation(logger, sessionContext));
			sessionOperations.Enqueue(new RemoteSessionOperation(remoteSessionDetector, logger, sessionContext));
			sessionOperations.Enqueue(new SessionIntegrityOperation(logger, registry, sessionContext));
			sessionOperations.Enqueue(new VirtualMachineOperation(vmDetector, logger, sessionContext));
			sessionOperations.Enqueue(new DisplayMonitorOperation(displayMonitor, logger, sessionContext, text));
			sessionOperations.Enqueue(new ServiceOperation(logger, runtimeHost, serviceProxy, sessionContext, THIRTY_SECONDS, userInfo));
			sessionOperations.Enqueue(new ClientTerminationOperation(logger, processFactory, proxyFactory, runtimeHost, sessionContext, THIRTY_SECONDS));
			sessionOperations.Enqueue(new ProctoringWorkaroundOperation(logger, sessionContext));
			sessionOperations.Enqueue(new KioskModeOperation(desktopFactory, desktopMonitor, explorerShell, logger, processFactory, sessionContext));
			sessionOperations.Enqueue(new ClientOperation(logger, processFactory, proxyFactory, runtimeHost, sessionContext, THIRTY_SECONDS));
			sessionOperations.Enqueue(new SessionActivationOperation(logger, sessionContext));

			var bootstrapSequence = new OperationSequence(logger, bootstrapOperations);
			var sessionSequence = new RepeatableOperationSequence(logger, sessionOperations);

			RuntimeController = new RuntimeController(
				appConfig,
				logger,
				messageBox,
				bootstrapSequence,
				sessionSequence,
				runtimeHost,
				runtimeWindow,
				serviceProxy,
				sessionContext,
				shutdown,
				splashScreen,
				text,
				uiFactory);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"/* {appConfig.ProgramTitle}, Version {appConfig.ProgramInformationalVersion}, Build {appConfig.ProgramBuildVersion}");
			logger.Log($"/* {appConfig.ProgramCopyright}");
			logger.Log($"/* ");
			logger.Log($"/* Please visit https://www.github.com/SafeExamBrowser for more information.");
			logger.Log(string.Empty);
			logger.Log($"# Application started at {appConfig.ApplicationStartTime:yyyy-MM-dd HH:mm:ss.fff}");
			logger.Log($"# Running on {systemInfo.OperatingSystemInfo}");
			logger.Log($"# Computer '{systemInfo.Name}' is a {systemInfo.Model} manufactured by {systemInfo.Manufacturer}");
			logger.Log($"# Runtime-ID: {appConfig.RuntimeId}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"# Application terminated at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
		}

		private void InitializeConfiguration()
		{
			var certificateStore = new CertificateStore(ModuleLogger(nameof(CertificateStore)));
			var compressor = new GZipCompressor(ModuleLogger(nameof(GZipCompressor)));
			var passwordEncryption = new PasswordEncryption(ModuleLogger(nameof(PasswordEncryption)));
			var publicKeyEncryption = new PublicKeyEncryption(certificateStore, ModuleLogger(nameof(PublicKeyEncryption)));
			var symmetricEncryption = new PublicKeySymmetricEncryption(certificateStore, ModuleLogger(nameof(PublicKeySymmetricEncryption)), passwordEncryption);
			var repositoryLogger = ModuleLogger(nameof(ConfigurationRepository));
			var xmlParser = new XmlParser(compressor, ModuleLogger(nameof(XmlParser)));
			var xmlSerializer = new XmlSerializer(ModuleLogger(nameof(XmlSerializer)));

			configuration = new ConfigurationRepository(certificateStore, repositoryLogger);
			appConfig = configuration.InitializeAppConfig();

			configuration.Register(new BinaryParser(
				compressor,
				new HashAlgorithm(),
				ModuleLogger(nameof(BinaryParser)),
				passwordEncryption,
				publicKeyEncryption,
				symmetricEncryption, xmlParser));
			configuration.Register(new BinarySerializer(
				compressor,
				ModuleLogger(nameof(BinarySerializer)),
				passwordEncryption,
				publicKeyEncryption,
				symmetricEncryption,
				xmlSerializer));
			configuration.Register(new XmlParser(compressor, ModuleLogger(nameof(XmlParser))));
			configuration.Register(new XmlSerializer(ModuleLogger(nameof(XmlSerializer))));
			configuration.Register(new FileResourceLoader(ModuleLogger(nameof(FileResourceLoader))));
			configuration.Register(new FileResourceSaver(ModuleLogger(nameof(FileResourceSaver))));
			configuration.Register(new NetworkResourceLoader(appConfig, new ModuleLogger(logger, nameof(NetworkResourceLoader))));
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), appConfig.RuntimeLogFilePath);

			logFileWriter.Initialize();
			logger.LogLevel = LogLevel.Debug;
			logger.Subscribe(logFileWriter);
		}

		private void InitializeText()
		{
			text = new Text(ModuleLogger(nameof(Text)));
		}

		private IModuleLogger ModuleLogger(string moduleInfo)
		{
			return new ModuleLogger(logger, moduleInfo);
		}
	}
}
