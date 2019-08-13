/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.Contracts.UserInterface.Windows.Events;

namespace SafeExamBrowser.UserInterface.Mobile
{
	public partial class AboutWindow : Window, IWindow
	{
		private AppConfig appConfig;
		private IText text;
		private WindowClosingEventHandler closing;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public AboutWindow(AppConfig appConfig, IText text)
		{
			this.appConfig = appConfig;
			this.text = text;

			InitializeComponent();
			InitializeAboutWindow();
		}

		public void BringToForeground()
		{
			Activate();
		}

		private void InitializeAboutWindow()
		{
			Closing += (o, args) => closing?.Invoke();
			VersionInfo.Inlines.Add(new Run($"{text.Get(TextKey.Version)} {appConfig.ProgramInformationalVersion}") { FontSize = 12 });
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new Run($"{text.Get(TextKey.Build)} {appConfig.ProgramBuildVersion}") { FontSize = 8, Foreground = Brushes.Gray });
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new Run(appConfig.ProgramCopyright) { FontSize = 10, Foreground = Brushes.Gray });
		}
	}
}
