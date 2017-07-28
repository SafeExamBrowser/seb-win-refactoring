/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class BrowserWindow : Window, IBrowserWindow
	{
		private IBrowserSettings settings;

		public event WindowCloseHandler OnClose;

		public BrowserWindow(IBrowserControl browserControl, IBrowserSettings settings)
		{
			this.settings = settings;

			InitializeComponent();
			InitializeBrowserWindow(browserControl);
		}

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

			UrlTextBox.IsEnabled = settings.AllowAddressBar;
			UrlTextBox.Visibility = settings.AllowAddressBar ? Visibility.Visible : Visibility.Collapsed;

			ReloadButton.IsEnabled = settings.AllowReloading;
			ReloadButton.Visibility = settings.AllowReloading ? Visibility.Visible : Visibility.Collapsed;

			BackButton.IsEnabled = settings.AllowBackwardNavigation;
			BackButton.Visibility = settings.AllowBackwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			ForwardButton.IsEnabled = settings.AllowForwardNavigation;
			ForwardButton.Visibility = settings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			Closing += (o, args) => OnClose?.Invoke();
		}
	}
}
