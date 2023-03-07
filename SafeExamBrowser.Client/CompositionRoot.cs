/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SafeExamBrowser.Applications;
using SafeExamBrowser.Browser;
using SafeExamBrowser.Client.Communication;
using SafeExamBrowser.Client.Notifications;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Configuration.Integrity;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Applications;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Monitoring.Keyboard;
using SafeExamBrowser.Monitoring.Mouse;
using SafeExamBrowser.Monitoring.System;
using SafeExamBrowser.Proctoring;
using SafeExamBrowser.Server;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.UserInterface;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.SystemComponents.Audio;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.SystemComponents.Keyboard;
using SafeExamBrowser.SystemComponents.Network;
using SafeExamBrowser.SystemComponents.PowerSupply;
using SafeExamBrowser.SystemComponents.Registry;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Activators;
using SafeExamBrowser.WindowsApi;
using SafeExamBrowser.WindowsApi.Contracts;
using Desktop = SafeExamBrowser.UserInterface.Desktop;
using Mobile = SafeExamBrowser.UserInterface.Mobile;

namespace SafeExamBrowser.Client
{
	internal class CompositionRoot
	{
		private const int TWO_SECONDS = 2000;
		private const int FIVE_SECONDS = 5000;

		private Guid authenticationToken;
		private ClientContext context;
		private string logFilePath;
		private LogLevel logLevel;
		private string runtimeHostUri;
		private UserInterfaceMode uiMode;

		private IActionCenter actionCenter;
		private ILogger logger;
		private IMessageBox messageBox;
		private INativeMethods nativeMethods;
		private INetworkAdapter networkAdapter;
		private IPowerSupply powerSupply;
		private IRuntimeProxy runtimeProxy;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
		private ITaskview taskview;
		private IUserInfo userInfo;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		internal ClientController ClientController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			ValidateCommandLineArguments();

			InitializeLogging();
			InitializeText();

			uiFactory = BuildUserInterfaceFactory();

			actionCenter = uiFactory.CreateActionCenter();
			context = new ClientContext();
			messageBox = BuildMessageBox();
			nativeMethods = new NativeMethods();
			networkAdapter = new NetworkAdapter(ModuleLogger(nameof(NetworkAdapter)), nativeMethods);
			powerSupply = new PowerSupply(ModuleLogger(nameof(PowerSupply)));
			runtimeProxy = new RuntimeProxy(runtimeHostUri, new ProxyObjectFactory(), ModuleLogger(nameof(RuntimeProxy)), Interlocutor.Client);
			systemInfo = new SystemInfo();
			taskbar = uiFactory.CreateTaskbar(ModuleLogger("Taskbar"));
			taskview = uiFactory.CreateTaskview();
			userInfo = new UserInfo(ModuleLogger(nameof(UserInfo)));

			var processFactory = new ProcessFactory(ModuleLogger(nameof(ProcessFactory)));
			var applicationMonitor = new ApplicationMonitor(TWO_SECONDS, ModuleLogger(nameof(ApplicationMonitor)), nativeMethods, processFactory);
			var applicationFactory = new ApplicationFactory(applicationMonitor, ModuleLogger(nameof(ApplicationFactory)), nativeMethods, processFactory);
			var displayMonitor = new DisplayMonitor(ModuleLogger(nameof(DisplayMonitor)), nativeMethods, systemInfo);
			var explorerShell = new ExplorerShell(ModuleLogger(nameof(ExplorerShell)), nativeMethods);
			var fileSystemDialog = BuildFileSystemDialog();
			var hashAlgorithm = new HashAlgorithm();
			var registry = new Registry(ModuleLogger(nameof(Registry)));
			var splashScreen = uiFactory.CreateSplashScreen();
			var systemMonitor = new SystemMonitor(ModuleLogger(nameof(SystemMonitor)));

			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text));
			operations.Enqueue(new RuntimeConnectionOperation(context, logger, runtimeProxy, authenticationToken));
			operations.Enqueue(new ConfigurationOperation(context, logger, runtimeProxy));
			operations.Enqueue(new DelegateOperation(UpdateAppConfig));
			operations.Enqueue(new DelegateOperation(BuildIntegrityModule));
			operations.Enqueue(new LazyInitializationOperation(BuildClientHostOperation));
			operations.Enqueue(new ClientHostDisconnectionOperation(context, logger, FIVE_SECONDS));
			operations.Enqueue(new LazyInitializationOperation(BuildKeyboardInterceptorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildMouseInterceptorOperation));
			operations.Enqueue(new ApplicationOperation(context, applicationFactory, applicationMonitor, logger, text));
			operations.Enqueue(new DisplayMonitorOperation(context, displayMonitor, logger, taskbar));
			operations.Enqueue(new SystemMonitorOperation(context, systemMonitor, logger));
			operations.Enqueue(new LazyInitializationOperation(BuildShellOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildBrowserOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildServerOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildProctoringOperation));
			operations.Enqueue(new ClipboardOperation(context, logger, nativeMethods));

			var sequence = new OperationSequence(logger, operations);

			ClientController = new ClientController(
				actionCenter,
				applicationMonitor,
				context,
				displayMonitor,
				explorerShell,
				fileSystemDialog,
				hashAlgorithm,
				logger,
				messageBox,
				sequence,
				registry,
				runtimeProxy,
				shutdown,
				splashScreen,
				systemMonitor,
				taskbar,
				text,
				uiFactory);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# New client instance started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"# Client instance terminated at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
		}

		private void ValidateCommandLineArguments()
		{
			var args = Environment.GetCommandLineArgs();
			var hasFive = args?.Length >= 5;
			var valid = false;

			if (hasFive)
			{
				var logFilePath = Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
				var hasLogFilePath = Uri.TryCreate(logFilePath, UriKind.Absolute, out var filePath) && filePath.IsFile;
				var hasLogLevel = Enum.TryParse(args[2], out LogLevel logLevel);
				var hasHostUri = Uri.TryCreate(args[3], UriKind.Absolute, out var runtimeHostUri) && runtimeHostUri.IsWellFormedOriginalString();
				var hasAuthenticationToken = Guid.TryParse(args[4], out var authenticationToken);

				if (hasLogFilePath && hasLogLevel && hasHostUri && hasAuthenticationToken)
				{
					this.authenticationToken = authenticationToken;
					this.logFilePath = logFilePath;
					this.logLevel = logLevel;
					this.runtimeHostUri = args[3];
					this.uiMode = args.Length == 6 && Enum.TryParse(args[5], out uiMode) ? uiMode : UserInterfaceMode.Desktop;

					valid = true;
				}
			}

			if (!valid)
			{
				throw new ArgumentException("Invalid arguments! Required: SafeExamBrowser.Client.exe <logfile path> <log level> <host URI> <token>");
			}
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), logFilePath);

			logFileWriter.Initialize();

			logger = new Logger();
			logger.LogLevel = logLevel;
			logger.Subscribe(logFileWriter);
		}

		private void InitializeText()
		{
			text = new Text(ModuleLogger(nameof(Text)));
		}

		private IOperation BuildBrowserOperation()
		{
			var fileSystemDialog = BuildFileSystemDialog();
			var keyGenerator = new KeyGenerator(context.AppConfig, context.IntegrityModule, ModuleLogger(nameof(KeyGenerator)));
			var moduleLogger = ModuleLogger(nameof(BrowserApplication));
			var browser = new BrowserApplication(
				context.AppConfig,
				context.Settings.Browser,
				fileSystemDialog,
				new HashAlgorithm(),
				keyGenerator,
				messageBox,
				moduleLogger,
				nativeMethods,
				context.Settings.SessionMode,
				text,
				uiFactory);
			var operation = new BrowserOperation(actionCenter, context, logger, taskbar, taskview, uiFactory);

			context.Browser = browser;

			return operation;
		}

		private IOperation BuildClientHostOperation()
		{
			var processId = Process.GetCurrentProcess().Id;
			var factory = new HostObjectFactory();
			var clientHost = new ClientHost(context.AppConfig.ClientAddress, factory, ModuleLogger(nameof(ClientHost)), processId, FIVE_SECONDS);
			var operation = new CommunicationHostOperation(clientHost, logger);

			context.ClientHost = clientHost;
			context.ClientHost.AuthenticationToken = authenticationToken;

			return operation;
		}

		private void BuildIntegrityModule()
		{
			context.IntegrityModule = new IntegrityModule(context.AppConfig, ModuleLogger(nameof(IntegrityModule)));
		}

		private IOperation BuildKeyboardInterceptorOperation()
		{
			var keyboardInterceptor = new KeyboardInterceptor(ModuleLogger(nameof(KeyboardInterceptor)), nativeMethods, context.Settings.Keyboard);
			var operation = new KeyboardInterceptorOperation(context, keyboardInterceptor, logger);

			return operation;
		}

		private IOperation BuildMouseInterceptorOperation()
		{
			var mouseInterceptor = new MouseInterceptor(ModuleLogger(nameof(MouseInterceptor)), nativeMethods, context.Settings.Mouse);
			var operation = new MouseInterceptorOperation(context, logger, mouseInterceptor);

			return operation;
		}

		private IOperation BuildProctoringOperation()
		{
			var controller = new ProctoringController(context.AppConfig, new FileSystem(), ModuleLogger(nameof(ProctoringController)), context.Server, text, uiFactory);
			var operation = new ProctoringOperation(actionCenter, context, controller, logger, controller, taskbar, uiFactory);

			return operation;
		}

		private IOperation BuildServerOperation()
		{
			var keyGenerator = new KeyGenerator(context.AppConfig, context.IntegrityModule, ModuleLogger(nameof(KeyGenerator)));
			var server = new ServerProxy(context.AppConfig, keyGenerator, ModuleLogger(nameof(ServerProxy)), systemInfo, userInfo, powerSupply, networkAdapter);
			var operation = new ServerOperation(context, logger, server);

			context.Server = server;

			return operation;
		}

		private IOperation BuildShellOperation()
		{
			var aboutNotification = new AboutNotification(context.AppConfig, text, uiFactory);
			var audio = new Audio(context.Settings.Audio, ModuleLogger(nameof(Audio)));
			var keyboard = new Keyboard(ModuleLogger(nameof(Keyboard)));
			var logNotification = new LogNotification(logger, text, uiFactory);
			var operation = new ShellOperation(
				actionCenter,
				audio,
				aboutNotification,
				context,
				keyboard,
				logger,
				logNotification,
				networkAdapter,
				powerSupply,
				systemInfo,
				taskbar,
				taskview,
				text,
				uiFactory);

			context.Activators.Add(new ActionCenterKeyboardActivator(ModuleLogger(nameof(ActionCenterKeyboardActivator)), nativeMethods));
			context.Activators.Add(new ActionCenterTouchActivator(ModuleLogger(nameof(ActionCenterTouchActivator)), nativeMethods));
			context.Activators.Add(new TaskbarKeyboardActivator(ModuleLogger(nameof(TaskbarKeyboardActivator)), nativeMethods));
			context.Activators.Add(new TaskviewKeyboardActivator(ModuleLogger(nameof(TaskviewKeyboardActivator)), nativeMethods));
			context.Activators.Add(new TerminationActivator(ModuleLogger(nameof(TerminationActivator)), nativeMethods));

			return operation;
		}

		private IFileSystemDialog BuildFileSystemDialog()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.FileSystemDialogFactory(text);
				default:
					return new Desktop.FileSystemDialogFactory(text);
			}
		}

		private IMessageBox BuildMessageBox()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.MessageBoxFactory(text);
				default:
					return new Desktop.MessageBoxFactory(text);
			}
		}

		private IUserInterfaceFactory BuildUserInterfaceFactory()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.UserInterfaceFactory(text);
				default:
					return new Desktop.UserInterfaceFactory(text);
			}
		}

		private void UpdateAppConfig()
		{
			ClientController.UpdateAppConfig();
		}

		private IModuleLogger ModuleLogger(string moduleInfo)
		{
			return new ModuleLogger(logger, moduleInfo);
		}
	}
}
