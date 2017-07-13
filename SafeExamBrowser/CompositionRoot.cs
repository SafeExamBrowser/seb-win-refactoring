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
		private IApplicationInfo browserInfo;
		private IMessageBox messageBox;
		private ILogger logger;
		private IUiElementFactory uiFactory;
		private IText text;

		public ISettings Settings { get; private set; }
		public IShutdownController ShutdownController { get; private set; }
		public IStartupController StartupController { get; private set; }
		public SplashScreen SplashScreen { get; set; }
		public Taskbar Taskbar { get; private set; }

		public CompositionRoot()
		{
			browserInfo = new BrowserApplicationInfo();
			messageBox = new WpfMessageBox();
			logger = new Logger();
			Settings = new Settings();
			Taskbar = new Taskbar();
			uiFactory = new UiElementFactory();
		}

		public void BuildObjectGraph()
		{
			logger.Subscribe(new LogFileWriter(Settings));

			text = new Text(new XmlTextResource());
			ShutdownController = new ShutdownController(logger, messageBox, text);
			StartupController = new StartupController(browserInfo, logger, messageBox, Settings, SplashScreen, Taskbar, text, uiFactory);
		}
	}
}
