/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface.Windows;

namespace SafeExamBrowser.UserInterface.Classic
{
	public partial class AboutWindow : Window, IWindow
	{
		private RuntimeInfo runtimeInfo;
		private IText text;
		private WindowClosingEventHandler closing;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public AboutWindow(RuntimeInfo runtimeInfo, IText text)
		{
			this.runtimeInfo = runtimeInfo;
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
			VersionInfo.Inlines.Add(new Run($"{text.Get(TextKey.Version)} {runtimeInfo.ProgramVersion}") { FontStyle = FontStyles.Italic });
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new LineBreak());
			VersionInfo.Inlines.Add(new Run(runtimeInfo.ProgramCopyright) { FontSize = 10 });
		}
	}
}
