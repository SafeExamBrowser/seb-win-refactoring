/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Configuration;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.Monitoring.Processes;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private IMessageBox messageBox;
		private INotificationInfo aboutInfo;
		private IProcessMonitor processMonitor;
		private ISettings settings;
		private IText text;
		private IUiElementFactory uiFactory;
		private ITextResource textResource;

		public IShutdownController ShutdownController { get; private set; }
		public IStartupController StartupController { get; private set; }
		public Taskbar Taskbar { get; private set; }

		public void BuildObjectGraph()
		{
			browserController = new BrowserApplicationController();
			browserInfo = new BrowserApplicationInfo();
			logger = new Logger();
			messageBox = new WpfMessageBox();
			settings = new Settings();
			Taskbar = new Taskbar();
			textResource = new XmlTextResource();
			uiFactory = new UiElementFactory();

			logger.Subscribe(new LogFileWriter(settings));

			text = new Text(textResource);
			aboutInfo = new AboutNotificationInfo(text);
			processMonitor = new ProcessMonitor(logger);
			ShutdownController = new ShutdownController(logger, messageBox, processMonitor, settings, text, uiFactory);
			StartupController = new StartupController(browserController, browserInfo, logger, messageBox, aboutInfo, processMonitor, settings, Taskbar, text, uiFactory);
		}
	}
}
