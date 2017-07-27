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
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.Monitoring.Processes;
using SafeExamBrowser.Monitoring.Windows;
using SafeExamBrowser.UserInterface;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private IEventController eventController;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private INotificationInfo aboutInfo;
		private IProcessMonitor processMonitor;
		private ISettings settings;
		private IText text;
		private ITextResource textResource;
		private IUserInterfaceFactory uiFactory;
		private IWindowMonitor windowMonitor;
		private IWorkingArea workingArea;

		public IShutdownController ShutdownController { get; private set; }
		public IStartupController StartupController { get; private set; }
		public Queue<IOperation> StartupOperations { get; private set; }
		public Taskbar Taskbar { get; private set; }

		public void BuildObjectGraph()
		{
			browserInfo = new BrowserApplicationInfo();
			logger = new Logger();
			nativeMethods = new NativeMethods();
			settings = new Settings();
			Taskbar = new Taskbar();
			textResource = new XmlTextResource();
			uiFactory = new UserInterfaceFactory();

			logger.Subscribe(new LogFileWriter(settings));

			text = new Text(textResource);
			aboutInfo = new AboutNotificationInfo(text);
			browserController = new BrowserApplicationController(settings, uiFactory);
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, typeof(ProcessMonitor)), nativeMethods);
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, typeof(WindowMonitor)), nativeMethods);
			workingArea = new WorkingArea(new ModuleLogger(logger, typeof(WorkingArea)), nativeMethods);
			eventController = new EventController(new ModuleLogger(logger, typeof(EventController)), processMonitor, Taskbar, windowMonitor, workingArea);
			ShutdownController = new ShutdownController(logger, settings, text, uiFactory);
			StartupController = new StartupController(logger, settings, text, uiFactory);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new WindowMonitorOperation(logger, windowMonitor));
			StartupOperations.Enqueue(new ProcessMonitorOperation(logger, processMonitor));
			StartupOperations.Enqueue(new WorkingAreaOperation(logger, Taskbar, workingArea));
			StartupOperations.Enqueue(new TaskbarOperation(logger, aboutInfo, Taskbar, uiFactory));
			StartupOperations.Enqueue(new BrowserOperation(browserController, browserInfo, logger, Taskbar, uiFactory));
			StartupOperations.Enqueue(new EventControllerOperation(eventController, logger));
		}
	}
}
