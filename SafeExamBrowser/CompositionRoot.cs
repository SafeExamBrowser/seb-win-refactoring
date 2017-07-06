/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Configuration;
using SafeExamBrowser.Core.I18n;
using SafeExamBrowser.Core.Logging;
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	class CompositionRoot
	{
		public ILogger Logger { get; private set; }
		public ISettings Settings { get; private set; }
		public IText Text { get; private set; }
		public Window Taskbar { get; private set; }

		public void InitializeGlobalModules()
		{
			Settings = new Settings();
			Logger = new Logger();
			Logger.Subscribe(new LogFileWriter(Settings));
			Text = new Text(new XmlTextResource());
		}

		public void BuildObjectGraph()
		{
			Taskbar = new Taskbar();
		}
	}
}
