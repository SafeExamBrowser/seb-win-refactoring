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
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Configuration;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private IMessageBox messageBox;
		private INotificationInfo aboutInfo;
		private ILogger logger;
		private IUiElementFactory uiFactory;

		public ISettings Settings { get; private set; }
		public IShutdownController ShutdownController { get; private set; }
		public IStartupController StartupController { get; private set; }
		public SplashScreen SplashScreen { get; set; }
		public Taskbar Taskbar { get; private set; }
		public IText Text { get; private set; }

		public void BuildModulesRequiredBySplashScreen()
		{
			Settings = new Settings();
			Text = new Text(new XmlTextResource());
		}

		public void BuildObjectGraph()
		{
			browserController = new BrowserApplicationController();
			browserInfo = new BrowserApplicationInfo();
			logger = new Logger();
			messageBox = new WpfMessageBox();
			Taskbar = new Taskbar();
			uiFactory = new UiElementFactory();

			logger.Subscribe(new LogFileWriter(Settings));

			aboutInfo = new AboutNotificationInfo(Text);
			ShutdownController = new ShutdownController(logger, messageBox, Text);
			StartupController = new StartupController(browserController, browserInfo, logger, messageBox, aboutInfo, Settings, SplashScreen, Taskbar, Text, uiFactory);
		}
	}
}
