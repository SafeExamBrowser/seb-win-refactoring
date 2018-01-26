/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Documents;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface.Classic
{
	public partial class RuntimeWindow : Window, ILogObserver, IRuntimeWindow
	{
		private ILogContentFormatter formatter;
		private IRuntimeInfo runtimeInfo;
		private IText text;
		private WindowClosingEventHandler closing;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public RuntimeWindow(ILogContentFormatter formatter, IRuntimeInfo runtimeInfo, IText text)
		{
			this.formatter = formatter;
			this.runtimeInfo = runtimeInfo;
			this.text = text;

			InitializeComponent();
			InitializeRuntimeWindow();
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public void Notify(ILogContent content)
		{
			Dispatcher.Invoke(() =>
			{
				LogTextBlock.Text += formatter.Format(content) + Environment.NewLine;
				LogScrollViewer.ScrollToEnd();
			});
		}

		public void UpdateStatus(TextKey key)
		{
			Dispatcher.Invoke(() => StatusTextBlock.Text = text.Get(key));
		}

		private void InitializeRuntimeWindow()
		{
			Title = $"{runtimeInfo.ProgramTitle} - Version {runtimeInfo.ProgramVersion}";
			InfoTextBlock.Inlines.Add(new Run($"Version {runtimeInfo.ProgramVersion}") { FontStyle = FontStyles.Italic });
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new Run(runtimeInfo.ProgramCopyright) { FontSize = 10 });
		}
	}
}
