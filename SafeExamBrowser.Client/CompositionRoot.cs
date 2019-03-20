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
using SafeExamBrowser.Client.Communication;
using SafeExamBrowser.Client.Notifications;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.Core.Operations;
using SafeExamBrowser.I18n;
using SafeExamBrowser.Logging;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Monitoring.Keyboard;
using SafeExamBrowser.Monitoring.Mouse;
using SafeExamBrowser.Monitoring.Processes;
using SafeExamBrowser.Monitoring.Windows;
using SafeExamBrowser.SystemComponents;
using SafeExamBrowser.UserInterface.Desktop;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Client
{
	internal class CompositionRoot
	{
		private ClientConfiguration configuration;
		private string logFilePath;
		private LogLevel logLevel;
		private string runtimeHostUri;
		private Guid startupToken;

		private IActionCenter actionCenter;
		private IBrowserApplicationController browserController;
		private IClientHost clientHost;
		private ILogger logger;
		private IMessageBox messageBox;
		private IProcessMonitor processMonitor;
		private INativeMethods nativeMethods;
		private IRuntimeProxy runtimeProxy;
		private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
		private ISystemInfo systemInfo;
		private ITaskbar taskbar;
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

			actionCenter = new ActionCenter();
			keyboardLayout = new KeyboardLayout(new ModuleLogger(logger, nameof(KeyboardLayout)), text);
			messageBox = new MessageBox(text);
			powerSupply = new PowerSupply(new ModuleLogger(logger, nameof(PowerSupply)), text);
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, nameof(ProcessMonitor)), nativeMethods);
			uiFactory = new UserInterfaceFactory(text);
			runtimeProxy = new RuntimeProxy(runtimeHostUri, new ProxyObjectFactory(), new ModuleLogger(logger, nameof(RuntimeProxy)));
			taskbar = new Taskbar(new ModuleLogger(logger, nameof(Taskbar)));
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, nameof(WindowMonitor)), nativeMethods);
			wirelessNetwork = new WirelessNetwork(new ModuleLogger(logger, nameof(WirelessNetwork)), text);

			var displayMonitor = new DisplayMonitor(new ModuleLogger(logger, nameof(DisplayMonitor)), nativeMethods, systemInfo);
			var explorerShell = new ExplorerShell(new ModuleLogger(logger, nameof(ExplorerShell)), nativeMethods);
			var hashAlgorithm = new HashAlgorithm();

			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text, textResource));
			operations.Enqueue(new RuntimeConnectionOperation(logger, runtimeProxy, startupToken));
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
			var hasFive = args?.Length == 5;

			if (hasFive)
			{
				var hasLogfilePath = Uri.TryCreate(args[1], UriKind.Absolute, out Uri filePath) && filePath.IsFile;
				var hasLogLevel = Enum.TryParse(args[2], out LogLevel level);
				var hasHostUri = Uri.TryCreate(args[3], UriKind.Absolute, out Uri hostUri) && hostUri.IsWellFormedOriginalString();
				var hasToken = Guid.TryParse(args[4], out Guid token);

				if (hasLogfilePath && hasLogLevel && hasHostUri && hasToken)
				{
					logFilePath = args[1];
					logLevel = level;
					runtimeHostUri = args[3];
					startupToken = token;

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
			var browserController = new BrowserApplicationController(configuration.AppConfig, configuration.Settings.Browser, messageBox, moduleLogger, text, uiFactory);
			var browserInfo = new BrowserApplicationInfo();
			var operation = new BrowserOperation(actionCenter, browserController, browserInfo, logger, taskbar, uiFactory);

			this.browserController = browserController;

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
			clientHost.StartupToken = startupToken;

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
			var logInfo = new LogNotificationInfo(text);
			var logController = new LogNotificationController(logger, uiFactory);
			var activators = new IActionCenterActivator[]
			{
				new KeyboardActivator(new ModuleLogger(logger, nameof(KeyboardActivator))),
				new TouchActivator(new ModuleLogger(logger, nameof(TouchActivator)))
			};
			var operation = new ShellOperation(
				actionCenter,
				activators,
				configuration.Settings.ActionCenter,
				logger,
				aboutInfo,
				aboutController,
				logInfo,
				logController,
				keyboardLayout,
				powerSupply,
				wirelessNetwork,
				systemInfo,
				taskbar,
				configuration.Settings.Taskbar,
				text,
				uiFactory);

			return operation;
		}

		private IOperation BuildWindowMonitorOperation()
		{
			return new WindowMonitorOperation(configuration.Settings.KioskMode, logger, windowMonitor);
		}

		private void UpdateAppConfig()
		{
			ClientController.AppConfig = configuration.AppConfig;
		}

		private void UpdateClientControllerDependencies()
		{
			ClientController.Browser = browserController;
			ClientController.ClientHost = clientHost;
			ClientController.SessionId = configuration.SessionId;
			ClientController.Settings = configuration.Settings;
		}
	}
}
