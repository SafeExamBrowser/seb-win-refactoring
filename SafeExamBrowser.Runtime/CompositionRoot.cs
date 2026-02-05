/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Configuration.DataCompression;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Configuration.DataResources;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.Core.ResponsibilityModel;
using SafeExamBrowser.I18n;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Integrity;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Monitoring.System;
using SafeExamBrowser.Runtime.Communication;
using SafeExamBrowser.Runtime.Operations.Bootstrap;
using SafeExamBrowser.Runtime.Operations.Session;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.Server;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.SystemComponents.Registry;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Desktop;
using SafeExamBrowser.WindowsApi;
using SafeExamBrowser.WindowsApi.Desktops;
using SafeExamBrowser.WindowsApi.Processes;

namespace SafeExamBrowser.Runtime
{
	internal class CompositionRoot
	{
		private const int FIVE_SECONDS = 5000;
		private const int THIRTY_SECONDS = 30000;

		private AppConfig appConfig;
		private IConfigurationRepository repository;
		private ILogger logger;
		private ISystemInfo systemInfo;
		private IText text;

		internal RuntimeController RuntimeController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			logger = new Logger();

			InitializeConfiguration();
			InitializeLogging();
			InitializeText();

			var uiFactory = new UserInterfaceFactory(text);

			var context = new RuntimeContext();
			var integrityModule = new IntegrityModule(appConfig, ModuleLogger(nameof(IntegrityModule)));
			var messageBox = new MessageBoxFactory(text);
			var registry = new Registry(ModuleLogger(nameof(Registry)));
			var runtimeHost = new RuntimeHost(appConfig.RuntimeAddress, new HostObjectFactory(), ModuleLogger(nameof(RuntimeHost)), FIVE_SECONDS);
			var runtimeWindow = uiFactory.CreateRuntimeWindow(appConfig);
			var serviceProxy = new ServiceProxy(appConfig.ServiceAddress, new ProxyObjectFactory(), ModuleLogger(nameof(ServiceProxy)), Interlocutor.Runtime);
			var splashScreen = uiFactory.CreateSplashScreen(appConfig);

			systemInfo = new SystemInfo(registry);

			var bootstrapSequence = BuildBootstrapOperations(integrityModule, runtimeHost, splashScreen);
			var sessionSequence = BuildSessionOperations(integrityModule, messageBox, registry, runtimeHost, runtimeWindow, serviceProxy, context, uiFactory);
			var responsibilities = BuildResponsibilities(messageBox, runtimeHost, runtimeWindow, serviceProxy, context, sessionSequence, shutdown, splashScreen);

			context.Responsibilities = responsibilities;

			RuntimeController = new RuntimeController(logger, bootstrapSequence, responsibilities, context, runtimeWindow, splashScreen);
		}

		private BootstrapOperationSequence BuildBootstrapOperations(IIntegrityModule integrityModule, IRuntimeHost runtimeHost, ISplashScreen splashScreen)
		{
			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text));
			operations.Enqueue(new CommunicationHostOperation(runtimeHost, logger));
			operations.Enqueue(new ApplicationIntegrityOperation(integrityModule, logger));

			return new BootstrapOperationSequence(logger, operations, splashScreen);
		}

		private ResponsibilityCollection<RuntimeTask> BuildResponsibilities(
			IMessageBox messageBox,
			IRuntimeHost runtimeHost,
			IRuntimeWindow runtimeWindow,
			IServiceProxy serviceProxy,
			RuntimeContext runtimeContext,
			SessionOperationSequence sessionSequence,
			Action shutdown,
			ISplashScreen splashScreen)
		{
			var responsibilities = new Queue<RuntimeResponsibility>();

			responsibilities.Enqueue(new ClientResponsibility(ModuleLogger(nameof(ClientResponsibility)), messageBox, runtimeContext, runtimeWindow, shutdown));
			responsibilities.Enqueue(new CommunicationResponsibility(ModuleLogger(nameof(CommunicationResponsibility)), runtimeContext, runtimeHost, shutdown));
			responsibilities.Enqueue(new ErrorMessageResponsibility(appConfig, ModuleLogger(nameof(ErrorMessageResponsibility)), messageBox, runtimeContext, splashScreen, text));
			responsibilities.Enqueue(new ServiceResponsibility(ModuleLogger(nameof(ServiceResponsibility)), messageBox, runtimeContext, runtimeWindow, serviceProxy, shutdown));
			responsibilities.Enqueue(new SessionResponsibility(appConfig, ModuleLogger(nameof(SessionResponsibility)), messageBox, runtimeContext, runtimeWindow, sessionSequence, shutdown, text));

			return new ResponsibilityCollection<RuntimeTask>(logger, responsibilities);
		}

		private SessionOperationSequence BuildSessionOperations(
			IIntegrityModule integrityModule,
			IMessageBox messageBox,
			IRegistry registry,
			IRuntimeHost runtimeHost,
			IRuntimeWindow runtimeWindow,
			IServiceProxy serviceProxy,
			RuntimeContext runtimeContext,
			IUserInterfaceFactory uiFactory)
		{
			var args = Environment.GetCommandLineArgs();
			var fileSystem = new FileSystem();
			var nativeMethods = new NativeMethods();
			var operations = new Queue<SessionOperation>();
			var userInfo = new UserInfo(ModuleLogger(nameof(UserInfo)));

			var clientBridge = new ClientBridge(runtimeHost, runtimeContext);
			var dependencies = new Dependencies(clientBridge, logger, messageBox, runtimeWindow, runtimeContext, text);
			var desktopFactory = new DesktopFactory(ModuleLogger(nameof(DesktopFactory)));
			var desktopMonitor = new DesktopMonitor(ModuleLogger(nameof(DesktopMonitor)));
			var displayMonitor = new DisplayMonitor(ModuleLogger(nameof(DisplayMonitor)), nativeMethods, systemInfo);
			var explorerShell = new ExplorerShell(ModuleLogger(nameof(ExplorerShell)), nativeMethods);
			var keyGenerator = new KeyGenerator(appConfig, integrityModule, ModuleLogger(nameof(KeyGenerator)));
			var processFactory = new ProcessFactory(ModuleLogger(nameof(ProcessFactory)));
			var proxyFactory = new ProxyFactory(new ProxyObjectFactory(), ModuleLogger(nameof(ProxyFactory)));
			var remoteSessionDetector = new RemoteSessionDetector(ModuleLogger(nameof(RemoteSessionDetector)));
			var sentinel = new SystemSentinel(ModuleLogger(nameof(SystemSentinel)), nativeMethods, registry);
			var server = new ServerProxy(appConfig, keyGenerator, ModuleLogger(nameof(ServerProxy)), systemInfo, userInfo);
			var virtualMachineDetector = new VirtualMachineDetector(integrityModule, ModuleLogger(nameof(VirtualMachineDetector)), registry, systemInfo);

			operations.Enqueue(new SessionInitializationOperation(dependencies, fileSystem, repository));
			operations.Enqueue(new ConfigurationOperation(args, dependencies, new FileSystem(), new HashAlgorithm(), repository, uiFactory));
			operations.Enqueue(new ServerOperation(dependencies, fileSystem, repository, server, uiFactory));
			operations.Enqueue(new VersionRestrictionOperation(dependencies));
			operations.Enqueue(new DisclaimerOperation(dependencies));
			operations.Enqueue(new RemoteSessionOperation(dependencies, remoteSessionDetector));
			operations.Enqueue(new SessionIntegrityOperation(dependencies, sentinel));
			operations.Enqueue(new VirtualMachineOperation(dependencies, virtualMachineDetector));
			operations.Enqueue(new DisplayMonitorOperation(dependencies, displayMonitor));
			operations.Enqueue(new ServiceOperation(dependencies, runtimeHost, serviceProxy, THIRTY_SECONDS, userInfo));
			operations.Enqueue(new ClientTerminationOperation(dependencies, processFactory, proxyFactory, runtimeHost, THIRTY_SECONDS));
			operations.Enqueue(new KioskModeOperation(dependencies, desktopFactory, desktopMonitor, explorerShell, processFactory));
			operations.Enqueue(new ClientOperation(dependencies, processFactory, proxyFactory, runtimeHost, THIRTY_SECONDS));
			operations.Enqueue(new SessionActivationOperation(dependencies));

			return new SessionOperationSequence(logger, operations, runtimeWindow);
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

			repository = new ConfigurationRepository(certificateStore, repositoryLogger);
			appConfig = repository.InitializeAppConfig();

			repository.Register(new BinaryParser(
				compressor,
				new HashAlgorithm(),
				ModuleLogger(nameof(BinaryParser)),
				passwordEncryption,
				publicKeyEncryption,
				symmetricEncryption, xmlParser));
			repository.Register(new BinarySerializer(
				compressor,
				ModuleLogger(nameof(BinarySerializer)),
				passwordEncryption,
				publicKeyEncryption,
				symmetricEncryption,
				xmlSerializer));
			repository.Register(new XmlParser(compressor, ModuleLogger(nameof(XmlParser))));
			repository.Register(new XmlSerializer(ModuleLogger(nameof(XmlSerializer))));
			repository.Register(new FileResourceLoader(ModuleLogger(nameof(FileResourceLoader))));
			repository.Register(new FileResourceSaver(ModuleLogger(nameof(FileResourceSaver))));
			repository.Register(new NetworkResourceLoader(appConfig, new ModuleLogger(logger, nameof(NetworkResourceLoader))));
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
