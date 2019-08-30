/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Browser;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Client.Communication;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Notifications;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Monitoring.Keyboard;
using SafeExamBrowser.Monitoring.Mouse;
using SafeExamBrowser.Monitoring.Processes;
using SafeExamBrowser.Monitoring.Windows;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.SystemComponents.Audio;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Keyboard;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.WindowsApi;
using SafeExamBrowser.WindowsApi.Contracts;
using Desktop = SafeExamBrowser.UserInterface.Desktop;
using Mobile = SafeExamBrowser.UserInterface.Mobile;

namespace SafeExamBrowser.Client
{
	internal class CompositionRoot
	{
		private Guid authenticationToken;
		private ClientConfiguration configuration;
		private string logFilePath;
		private LogLevel logLevel;
		private string runtimeHostUri;
		private UserInterfaceMode uiMode;

		private IActionCenter actionCenter;
		private IBrowserApplication browser;
		private IClientHost clientHost;
		private ILogger logger;
		private IMessageBox messageBox;
		private IProcessMonitor processMonitor;
		private INativeMethods nativeMethods;
		private IRuntimeProxy runtimeProxy;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
		private ITerminationActivator terminationActivator;
		private IText text;
		private ITextResource textResource;
		private IUserInterfaceFactory uiFactory;
		private IWindowMonitor windowMonitor;

		internal IClientController ClientController { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			ValidateCommandLineArguments();

			configuration = new ClientConfiguration();
			logger = new Logger();
			nativeMethods = new NativeMethods();
			systemInfo = new SystemInfo();

			InitializeLogging();
			InitializeText();

			actionCenter = BuildActionCenter();
			messageBox = BuildMessageBox();
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, nameof(ProcessMonitor)), nativeMethods);
			uiFactory = BuildUserInterfaceFactory();
			runtimeProxy = new RuntimeProxy(runtimeHostUri, new ProxyObjectFactory(), new ModuleLogger(logger, nameof(RuntimeProxy)), Interlocutor.Client);
			taskbar = BuildTaskbar();
			terminationActivator = new TerminationActivator(new ModuleLogger(logger, nameof(TerminationActivator)));
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, nameof(WindowMonitor)), nativeMethods);

			var displayMonitor = new DisplayMonitor(new ModuleLogger(logger, nameof(DisplayMonitor)), nativeMethods, systemInfo);
			var explorerShell = new ExplorerShell(new ModuleLogger(logger, nameof(ExplorerShell)), nativeMethods);
			var hashAlgorithm = new HashAlgorithm();

			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text, textResource));
			operations.Enqueue(new RuntimeConnectionOperation(logger, runtimeProxy, authenticationToken));
			operations.Enqueue(new ConfigurationOperation(configuration, logger, runtimeProxy));
			operations.Enqueue(new DelegateOperation(UpdateAppConfig));
			operations.Enqueue(new LazyInitializationOperation(BuildClientHostOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildClientHostDisconnectionOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildKeyboardInterceptorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildMouseInterceptorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildWindowMonitorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildProcessMonitorOperation));
			operations.Enqueue(new DisplayMonitorOperation(displayMonitor, logger, taskbar));
			operations.Enqueue(new LazyInitializationOperation(BuildShellOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildBrowserOperation));
			operations.Enqueue(new ClipboardOperation(logger, nativeMethods));
			operations.Enqueue(new DelegateOperation(UpdateClientControllerDependencies));

			var sequence = new OperationSequence(logger, operations);

			ClientController = new ClientController(
				actionCenter,
				displayMonitor,
				explorerShell,
				hashAlgorithm,
				logger,
				messageBox,
				sequence,
				processMonitor,
				runtimeProxy,
				shutdown,
				taskbar,
				terminationActivator,
				text,
				uiFactory,
				windowMonitor);
		}

		internal void LogStartupInformation()
		{
			logger.Log($"# New client instance started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log(string.Empty);
		}

		internal void LogShutdownInformation()
		{
			logger?.Log($"# Client instance terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private void ValidateCommandLineArguments()
		{
			var args = Environment.GetCommandLineArgs();
			var hasFive = args?.Length >= 5;

			if (hasFive)
			{
				var hasLogfilePath = Uri.TryCreate(args[1], UriKind.Absolute, out Uri filePath) && filePath.IsFile;
				var hasLogLevel = Enum.TryParse(args[2], out LogLevel level);
				var hasHostUri = Uri.TryCreate(args[3], UriKind.Absolute, out Uri hostUri) && hostUri.IsWellFormedOriginalString();
				var hasAuthenticationToken = Guid.TryParse(args[4], out Guid token);

				if (hasLogfilePath && hasLogLevel && hasHostUri && hasAuthenticationToken)
				{
					logFilePath = args[1];
					logLevel = level;
					runtimeHostUri = args[3];
					authenticationToken = token;
					uiMode = args.Length == 6 && Enum.TryParse(args[5], out uiMode) ? uiMode : UserInterfaceMode.Desktop;

					return;
				}
			}

			throw new ArgumentException("Invalid arguments! Required: SafeExamBrowser.Client.exe <logfile path> <log level> <host URI> <token>");
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), logFilePath);

			logFileWriter.Initialize();
			logger.LogLevel = logLevel;
			logger.Subscribe(logFileWriter);
		}

		private void InitializeText()
		{
			var location = Assembly.GetAssembly(typeof(XmlTextResource)).Location;
			var path = $@"{Path.GetDirectoryName(location)}\Text.xml";

			text = new Text(logger);
			textResource = new XmlTextResource(path);
		}

		private IOperation BuildBrowserOperation()
		{
			var moduleLogger = new ModuleLogger(logger, "BrowserController");
			var browser = new BrowserApplication(configuration.AppConfig, configuration.Settings.Browser, messageBox, moduleLogger, text, uiFactory);
			var browserInfo = new BrowserApplicationInfo();
			var operation = new BrowserOperation(actionCenter, browser, logger, taskbar, uiFactory);

			this.browser = browser;

			return operation;
		}

		private IOperation BuildClientHostOperation()
		{
			const int FIVE_SECONDS = 5000;
			var processId = Process.GetCurrentProcess().Id;
			var factory = new HostObjectFactory();
			var host = new ClientHost(configuration.AppConfig.ClientAddress, factory, new ModuleLogger(logger, nameof(ClientHost)), processId, FIVE_SECONDS);
			var operation = new CommunicationHostOperation(host, logger);

			clientHost = host;
			clientHost.AuthenticationToken = authenticationToken;

			return operation;
		}

		private IOperation BuildClientHostDisconnectionOperation()
		{
			var timeout_ms = 5000;
			var operation = new ClientHostDisconnectionOperation(clientHost, logger, timeout_ms);

			return operation;
		}

		private IOperation BuildKeyboardInterceptorOperation()
		{
			var keyboardInterceptor = new KeyboardInterceptor(configuration.Settings.Keyboard, new ModuleLogger(logger, nameof(KeyboardInterceptor)));
			var operation = new KeyboardInterceptorOperation(keyboardInterceptor, logger, nativeMethods);

			return operation;
		}

		private IOperation BuildMouseInterceptorOperation()
		{
			var mouseInterceptor = new MouseInterceptor(new ModuleLogger(logger, nameof(MouseInterceptor)), configuration.Settings.Mouse);
			var operation = new MouseInterceptorOperation(logger, mouseInterceptor, nativeMethods);

			return operation;
		}

		private IOperation BuildProcessMonitorOperation()
		{
			return new ProcessMonitorOperation(logger, processMonitor, configuration.Settings);
		}

		private IOperation BuildShellOperation()
		{
			var aboutInfo = new AboutNotificationInfo(text);
			var aboutController = new AboutNotificationController(configuration.AppConfig, uiFactory);
			var audio = new Audio(configuration.Settings.Audio, new ModuleLogger(logger, nameof(Audio)), text);
			var keyboard = new Keyboard(new ModuleLogger(logger, nameof(Keyboard)), text);
			var logInfo = new LogNotificationInfo(text);
			var logController = new LogNotificationController(logger, uiFactory);
			var powerSupply = new PowerSupply(new ModuleLogger(logger, nameof(PowerSupply)), text);
			var wirelessNetwork = new WirelessNetwork(new ModuleLogger(logger, nameof(WirelessNetwork)), text);
			var activators = new IActionCenterActivator[]
			{
				new KeyboardActivator(new ModuleLogger(logger, nameof(KeyboardActivator))),
				new TouchActivator(new ModuleLogger(logger, nameof(TouchActivator)))
			};
			var operation = new ShellOperation(
				actionCenter,
				activators,
				configuration.Settings.ActionCenter,
				audio,
				logger,
				aboutInfo,
				aboutController,
				keyboard,
				logInfo,
				logController,
				// TODO 
				//powerSupply,
				//wirelessNetwork,
				systemInfo,
				taskbar,
				configuration.Settings.Taskbar,
				terminationActivator,
				text,
				uiFactory);

			return operation;
		}

		private IOperation BuildWindowMonitorOperation()
		{
			return new WindowMonitorOperation(configuration.Settings.KioskMode, logger, windowMonitor);
		}

		private IActionCenter BuildActionCenter()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.ActionCenter();
				default:
					return new Desktop.ActionCenter();
			}
		}

		private IMessageBox BuildMessageBox()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.MessageBox(text);
				default:
					return new Desktop.MessageBox(text);
			}
		}

		private ITaskbar BuildTaskbar()
		{
			switch (uiMode)
			{
				case UserInterfaceMode.Mobile:
					return new Mobile.Taskbar(new ModuleLogger(logger, nameof(Mobile.Taskbar)));
				default:
					return new Desktop.Taskbar(new ModuleLogger(logger, nameof(Desktop.Taskbar)));
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
			ClientController.AppConfig = configuration.AppConfig;
		}

		private void UpdateClientControllerDependencies()
		{
			ClientController.Browser = browser;
			ClientController.ClientHost = clientHost;
			ClientController.SessionId = configuration.SessionId;
			ClientController.Settings = configuration.Settings;
		}
	}
}
