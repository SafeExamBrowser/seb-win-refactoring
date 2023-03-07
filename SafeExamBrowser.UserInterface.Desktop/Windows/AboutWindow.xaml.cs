/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	internal partial class AboutWindow : Window, IWindow
	{
		private readonly AppConfig appConfig;
		private readonly IText text;

		private WindowClosedEventHandler closed;
		private WindowClosingEventHandler closing;

		event WindowClosedEventHandler IWindow.Closed
		{
			add { closed += value; }
			remove { closed -= value; }
		}

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		internal AboutWindow(AppConfig appConfig, IText text)
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
			Closed += (o, args) => closed?.Invoke();
			Closing += (o, args) => closing?.Invoke();
			MainText.Inlines.InsertBefore(MainText.Inlines.FirstInline, new Run(text.Get(TextKey.AboutWindow_LicenseInfo)));
			Title = text.Get(TextKey.AboutWindow_Title);

			VersionInfo.Inlines.Add(new Run($"{text.Get(TextKey.Version)} {appConfig.ProgramInformationalVersion}") { FontSize = 12 });
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new Run($"{text.Get(TextKey.Build)} {appConfig.ProgramBuildVersion}") { FontSize = 8, Foreground = Brushes.Gray });
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new Run(appConfig.ProgramCopyright) { FontSize = 10, Foreground = Brushes.Gray });
		}
	}
}
