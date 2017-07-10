/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Core.Configuration;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	internal class CompositionRoot
	{
		public ILogger Logger { get; private set; }
		public IMessageBox MessageBox { get; private set; }
		public ISettings Settings { get; private set; }
		public IShutdownController ShutdownController { get; set; }
		public IStartupController StartupController { get; private set; }
		public IText Text { get; private set; }
		public SplashScreen SplashScreen { get; private set; }
		public Taskbar Taskbar { get; private set; }

		public void BuildObjectGraph()
		{
			MessageBox = new WpfMessageBox();
			Settings = new Settings();
			Taskbar = new Taskbar();
			
			Logger = new Logger();
			Logger.Subscribe(new LogFileWriter(Settings));

			Text = new Text(new XmlTextResource());
			SplashScreen = new SplashScreen(Settings);
			ShutdownController = new ShutdownController(Logger, MessageBox, Text);
			StartupController = new StartupController(Logger, MessageBox, Settings, SplashScreen, Taskbar, Text);
		}
	}
}
