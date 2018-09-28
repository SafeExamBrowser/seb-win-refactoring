/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
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
using SafeExamBrowser.UserInterface.Classic;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Client
{
	internal class CompositionRoot
	{
		private string logFilePath;
		private string runtimeHostUri;
		private Guid startupToken;

		private IBrowserApplicationController browserController;
		private ClientConfiguration configuration;
		private IClientHost clientHost;
		private ILogger logger;
		private IMessageBox messageBox;
		private IProcessMonitor processMonitor;
		private INativeMethods nativeMethods;
		private IRuntimeProxy runtimeProxy;
		private ISystemInfo systemInfo;
		private IText text;
		private ITextResource textResource;
		private IUserInterfaceFactory uiFactory;
		private IWindowMonitor windowMonitor;

		internal IClientController ClientController { get; private set; }
		internal Taskbar Taskbar { get; private set; }

		internal void BuildObjectGraph(Action shutdown)
		{
			ValidateCommandLineArguments();

			configuration = new ClientConfiguration();
			logger = new Logger();
			nativeMethods = new NativeMethods();
			systemInfo = new SystemInfo();

			InitializeLogging();
			InitializeText();

			messageBox = new MessageBox(text);
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, nameof(ProcessMonitor)), nativeMethods);
			uiFactory = new UserInterfaceFactory(text);
			runtimeProxy = new RuntimeProxy(runtimeHostUri, new ProxyObjectFactory(), new ModuleLogger(logger, nameof(RuntimeProxy)));
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, nameof(WindowMonitor)), nativeMethods);

			var displayMonitor = new DisplayMonitor(new ModuleLogger(logger, nameof(DisplayMonitor)), nativeMethods);
			var explorerShell = new ExplorerShell(new ModuleLogger(logger, nameof(ExplorerShell)), nativeMethods);

			Taskbar = new Taskbar(new ModuleLogger(logger, nameof(Taskbar)));

			var operations = new Queue<IOperation>();

			operations.Enqueue(new I18nOperation(logger, text, textResource));
			operations.Enqueue(new RuntimeConnectionOperation(logger, runtimeProxy, startupToken));
			operations.Enqueue(new ConfigurationOperation(configuration, logger, runtimeProxy));
			operations.Enqueue(new DelegateOperation(UpdateAppConfig));
			operations.Enqueue(new LazyInitializationOperation(BuildClientHostOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildClientHostDisconnectionOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildKeyboardInterceptorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildWindowMonitorOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildProcessMonitorOperation));
			operations.Enqueue(new DisplayMonitorOperation(displayMonitor, logger, Taskbar));
			operations.Enqueue(new LazyInitializationOperation(BuildTaskbarOperation));
			operations.Enqueue(new LazyInitializationOperation(BuildBrowserOperation));
			operations.Enqueue(new ClipboardOperation(logger, nativeMethods));
			operations.Enqueue(new LazyInitializationOperation(BuildMouseInterceptorOperation));
			operations.Enqueue(new DelegateOperation(UpdateClientControllerDependencies));

			var sequence = new OperationSequence(logger, operations);

			ClientController = new ClientController(displayMonitor, explorerShell, logger, messageBox, sequence, processMonitor, runtimeProxy, shutdown, Taskbar, text, uiFactory, windowMonitor);
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
			var hasFour = args?.Length == 4;

			if (hasFour)
			{
				var hasLogfilePath = Uri.TryCreate(args?[1], UriKind.Absolute, out Uri filePath) && filePath.IsFile;
				var hasHostUri = Uri.TryCreate(args?[2], UriKind.Absolute, out Uri hostUri) && hostUri.IsWellFormedOriginalString();
				var hasToken = Guid.TryParse(args?[3], out Guid token);

				if (hasLogfilePath && hasHostUri && hasToken)
				{
					logFilePath = args[1];
					runtimeHostUri = args[2];
					startupToken = Guid.Parse(args[3]);

					return;
				}
			}

			throw new ArgumentException("Invalid arguments! Required: SafeExamBrowser.Client.exe <logfile path> <host URI> <token>");
		}

		private void InitializeLogging()
		{
			var logFileWriter = new LogFileWriter(new DefaultLogFormatter(), logFilePath);

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

		private IOperation BuildBrowserOperation()
		{
			var moduleLogger = new ModuleLogger(logger, "BrowserController");
			var browserController = new BrowserApplicationController(configuration.AppConfig, configuration.Settings.Browser, moduleLogger, messageBox, text, uiFactory);
			var browserInfo = new BrowserApplicationInfo();
			var operation = new BrowserOperation(browserController, browserInfo, logger, Taskbar, uiFactory);

			this.browserController = browserController;

			return operation;
		}

		private IOperation BuildClientHostOperation()
		{
			var processId = Process.GetCurrentProcess().Id;
			var factory = new HostObjectFactory();
			var host = new ClientHost(configuration.AppConfig.ClientAddress, factory, new ModuleLogger(logger, nameof(ClientHost)), processId);
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

		private IOperation BuildTaskbarOperation()
		{
			var keyboardLayout = new KeyboardLayout(new ModuleLogger(logger, nameof(KeyboardLayout)), text);
			var logController = new LogNotificationController(logger, uiFactory);
			var logInfo = new LogNotificationInfo(text);
			var powerSupply = new PowerSupply(new ModuleLogger(logger, nameof(PowerSupply)), text);
			var wirelessNetwork = new WirelessNetwork(new ModuleLogger(logger, nameof(WirelessNetwork)), text);
			var operation = new TaskbarOperation(logger, logInfo, logController, keyboardLayout, powerSupply, wirelessNetwork, systemInfo, Taskbar, configuration.Settings.Taskbar, text, uiFactory);

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
