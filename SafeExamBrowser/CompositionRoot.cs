/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Core.Behaviour;
using SafeExamBrowser.Core.Configuration;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		public IShutdownController ShutdownController { get; set; }
		public IStartupController StartupController { get; private set; }

		public SplashScreen SplashScreen { get; private set; }
		public Taskbar Taskbar { get; private set; }

		public void BuildObjectGraph()
		{
			var browserInfo = new BrowserApplicationInfo();
			var messageBox = new WpfMessageBox();
			var settings = new Settings();
			var logger = new Logger();
			var text = new Text(new XmlTextResource());
			var uiFactory = new UiElementFactory();
			
			logger.Subscribe(new LogFileWriter(settings));

			Taskbar = new Taskbar();
			SplashScreen = new SplashScreen(settings);
			ShutdownController = new ShutdownController(logger, messageBox, text);
			StartupController = new StartupController(browserInfo, logger, messageBox, settings, SplashScreen, Taskbar, text, uiFactory);
		}
	}
}
