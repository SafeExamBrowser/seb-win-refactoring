/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Browser;
using SafeExamBrowser.Configuration;
using SafeExamBrowser.Contracts.Behaviour;
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

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private IDisplayMonitor displayMonitor;
		private IKeyboardInterceptor keyboardInterceptor;
		private ILogger logger;
		private ILogContentFormatter logFormatter;
		private IMouseInterceptor mouseInterceptor;
		private INativeMethods nativeMethods;
		private IProcessMonitor processMonitor;
		private IRuntimeController runtimeController;
		private ISettings settings;
		private ISystemComponent<ISystemKeyboardLayoutControl> keyboardLayout;
		private ISystemComponent<ISystemPowerSupplyControl> powerSupply;
		private ISystemComponent<ISystemWirelessNetworkControl> wirelessNetwork;
		private ISystemInfo systemInfo;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private IWindowMonitor windowMonitor;

		public IShutdownController ShutdownController { get; private set; }
		public IStartupController StartupController { get; private set; }
		public Queue<IOperation> StartupOperations { get; private set; }
		public Taskbar Taskbar { get; private set; }

		public void BuildObjectGraph()
		{
			browserInfo = new BrowserApplicationInfo();
			logger = new Logger();
			logFormatter = new DefaultLogFormatter();
			nativeMethods = new NativeMethods();
			settings = new Settings();
			systemInfo = new SystemInfo();
			uiFactory = new UserInterfaceFactory();

			logger.Subscribe(new LogFileWriter(logFormatter, settings));

			text = new Text(logger);
			Taskbar = new Taskbar(new ModuleLogger(logger, typeof(Taskbar)));
			browserController = new BrowserApplicationController(settings, text, uiFactory);
			displayMonitor = new DisplayMonitor(new ModuleLogger(logger, typeof(DisplayMonitor)), nativeMethods);
			keyboardInterceptor = new KeyboardInterceptor(settings.Keyboard, new ModuleLogger(logger, typeof(KeyboardInterceptor)));
			keyboardLayout = new KeyboardLayout(new ModuleLogger(logger, typeof(KeyboardLayout)), text);
			mouseInterceptor = new MouseInterceptor(new ModuleLogger(logger, typeof(MouseInterceptor)), settings.Mouse);
			powerSupply = new PowerSupply(new ModuleLogger(logger, typeof(PowerSupply)), text);
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, typeof(ProcessMonitor)), nativeMethods);
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, typeof(WindowMonitor)), nativeMethods);
			wirelessNetwork = new WirelessNetwork(new ModuleLogger(logger, typeof(WirelessNetwork)));

			runtimeController = new RuntimeController(displayMonitor, new ModuleLogger(logger, typeof(RuntimeController)), processMonitor, Taskbar, windowMonitor);
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
			StartupOperations.Enqueue(new RuntimeControllerOperation(runtimeController, logger));
			StartupOperations.Enqueue(new ClipboardOperation(logger, nativeMethods));
			StartupOperations.Enqueue(new MouseInterceptorOperation(logger, mouseInterceptor, nativeMethods));
		}
	}
}
