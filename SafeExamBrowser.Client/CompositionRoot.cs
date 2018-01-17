/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Browser;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Configuration.Settings;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
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
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private IClientController clientController;
		private IDisplayMonitor displayMonitor;
		private IKeyboardInterceptor keyboardInterceptor;
		private ILogger logger;
		private IMouseInterceptor mouseInterceptor;
		private IProcessMonitor processMonitor;
		private INativeMethods nativeMethods;
		private ISettings settings;
		private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
		private ISystemInfo systemInfo;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private IWindowMonitor windowMonitor;

		internal IShutdownController ShutdownController { get; private set; }
		internal IStartupController StartupController { get; private set; }
		internal Queue<IOperation> StartupOperations { get; private set; }
		internal Taskbar Taskbar { get; private set; }

		internal void BuildObjectGraph()
		{
			browserInfo = new BrowserApplicationInfo();
			nativeMethods = new NativeMethods();
			settings = new SettingsRepository().LoadDefaults();
			systemInfo = new SystemInfo();
			uiFactory = new UserInterfaceFactory();

			InitializeLogger();

			text = new Text(logger);
			Taskbar = new Taskbar(new ModuleLogger(logger, typeof(Taskbar)));
			browserController = new BrowserApplicationController(settings.Browser, text, uiFactory);
			displayMonitor = new DisplayMonitor(new ModuleLogger(logger, typeof(DisplayMonitor)), nativeMethods);
			keyboardInterceptor = new KeyboardInterceptor(settings.Keyboard, new ModuleLogger(logger, typeof(KeyboardInterceptor)));
			keyboardLayout = new KeyboardLayout(new ModuleLogger(logger, typeof(KeyboardLayout)), text);
			mouseInterceptor = new MouseInterceptor(new ModuleLogger(logger, typeof(MouseInterceptor)), settings.Mouse);
			powerSupply = new PowerSupply(new ModuleLogger(logger, typeof(PowerSupply)), text);
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, typeof(ProcessMonitor)), nativeMethods);
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, typeof(WindowMonitor)), nativeMethods);
			wirelessNetwork = new WirelessNetwork(new ModuleLogger(logger, typeof(WirelessNetwork)), text);

			clientController = new ClientController(displayMonitor, new ModuleLogger(logger, typeof(ClientController)), processMonitor, Taskbar, windowMonitor);
			ShutdownController = new ShutdownController(logger, settings, text, uiFactory);
			StartupController = new StartupController(logger, settings, systemInfo, text, uiFactory);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new I18nOperation(logger, text));
			StartupOperations.Enqueue(new KeyboardInterceptorOperation(keyboardInterceptor, logger, nativeMethods));
			StartupOperations.Enqueue(new WindowMonitorOperation(logger, windowMonitor));
			StartupOperations.Enqueue(new ProcessMonitorOperation(logger, processMonitor));
			StartupOperations.Enqueue(new DisplayMonitorOperation(displayMonitor, logger, Taskbar));
			StartupOperations.Enqueue(new TaskbarOperation(logger, settings.Taskbar, keyboardLayout, powerSupply, wirelessNetwork, systemInfo, Taskbar, text, uiFactory));
			StartupOperations.Enqueue(new BrowserOperation(browserController, browserInfo, logger, Taskbar, uiFactory));
			StartupOperations.Enqueue(new ClientControllerOperation(clientController, logger));
			StartupOperations.Enqueue(new ClipboardOperation(logger, nativeMethods));
			StartupOperations.Enqueue(new MouseInterceptorOperation(logger, mouseInterceptor, nativeMethods));
		}

		private void InitializeLogger()
		{
			logger = new Logger();
			logger.Subscribe(new LogFileWriter(new DefaultLogFormatter(), settings.Logging.ClientLogFile));
		}
	}
}
