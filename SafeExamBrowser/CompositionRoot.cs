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
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Behaviour.Operations;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.Monitoring.Processes;
using SafeExamBrowser.Monitoring.Windows;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private INotificationInfo aboutInfo;
		private IProcessMonitor processMonitor;
		private ISettings settings;
		private IText text;
		private ITextResource textResource;
		private IUiElementFactory uiFactory;
		private IWindowMonitor windowMonitor;
		private IWorkingArea workingArea;

		public IShutdownController ShutdownController { get; private set; }
		public IStartupController StartupController { get; private set; }
		public Queue<IOperation> StartupOperations { get; private set; }
		public Taskbar Taskbar { get; private set; }

		public void BuildObjectGraph()
		{
			browserController = new BrowserApplicationController();
			browserInfo = new BrowserApplicationInfo();
			logger = new Logger();
			settings = new Settings();
			Taskbar = new Taskbar();
			textResource = new XmlTextResource();
			uiFactory = new UiElementFactory();

			logger.Subscribe(new LogFileWriter(settings));

			text = new Text(textResource);
			aboutInfo = new AboutNotificationInfo(text);
			processMonitor = new ProcessMonitor(new ModuleLogger(logger, typeof(ProcessMonitor)));
			windowMonitor = new WindowMonitor(new ModuleLogger(logger, typeof(WindowMonitor)));
			workingArea = new WorkingArea(new ModuleLogger(logger, typeof(WorkingArea)));
			ShutdownController = new ShutdownController(logger, settings, text, uiFactory);
			StartupController = new StartupController(logger, settings, text, uiFactory);

			StartupOperations = new Queue<IOperation>();
			StartupOperations.Enqueue(new WindowMonitoringOperation(logger, windowMonitor));
			StartupOperations.Enqueue(new ProcessMonitoringOperation(logger, processMonitor));
			StartupOperations.Enqueue(new WorkingAreaOperation(logger, Taskbar, workingArea));
			StartupOperations.Enqueue(new TaskbarInitializationOperation(logger, aboutInfo, Taskbar, uiFactory));
			StartupOperations.Enqueue(new BrowserInitializationOperation(browserController, browserInfo, logger, Taskbar, uiFactory));
		}
	}
}
