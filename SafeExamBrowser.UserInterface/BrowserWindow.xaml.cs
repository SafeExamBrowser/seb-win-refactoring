/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class BrowserWindow : Window, IBrowserWindow
	{
		public BrowserWindow(IBrowserControl browserControl)
		{
			InitializeComponent();
			InitializeBrowserWindow(browserControl);
		}

		public event WindowCloseHandler OnClose;

		public void BringToForeground()
		{
			if (WindowState == WindowState.Minimized)
			{
				WindowState = WindowState.Normal;
			}

			Activate();
		}

		private void InitializeBrowserWindow(IBrowserControl browserControl)
		{
			if (browserControl is System.Windows.Forms.Control)
			{
				BrowserControlHost.Child = browserControl as System.Windows.Forms.Control;
			}

			Closing += (o, args) => OnClose?.Invoke();
		}
	}
}
