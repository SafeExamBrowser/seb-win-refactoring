/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class AboutWindow : Window, IWindow
	{
		private ISettings settings;
		private IText text;
		private WindowClosingEventHandler closing;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public AboutWindow(ISettings settings, IText text)
		{
			this.settings = settings;
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
			VersionInfo.Inlines.Add(new Run($"{text.Get(Key.Version)} {settings.ProgramVersion}") { FontStyle = FontStyles.Italic });
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new Run(settings.ProgramCopyright) { FontSize = 10 });
		}
	}
}
