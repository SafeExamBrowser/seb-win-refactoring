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
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.UserInterface.Classic
{
	public partial class RuntimeWindow : Window, ILogObserver
	{
		private ILogContentFormatter formatter;
		private IRuntimeInfo runtimeInfo;

		public RuntimeWindow(ILogContentFormatter formatter, IRuntimeInfo runtimeInfo)
		{
			this.formatter = formatter;
			this.runtimeInfo = runtimeInfo;

			InitializeComponent();
			InitializeRuntimeWindow();
		}

		public void Notify(ILogContent content)
		{
			LogTextBlock.Text += formatter.Format(content) + Environment.NewLine;
			LogScrollViewer.ScrollToEnd();
		}

		private void InitializeRuntimeWindow()
		{
			InfoTextBlock.Inlines.Add(new Run($"Version {runtimeInfo.ProgramVersion}") { FontStyle = FontStyles.Italic });
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new Run(runtimeInfo.ProgramCopyright) { FontSize = 10 });
		}
	}
}
