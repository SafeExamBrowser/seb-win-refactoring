/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;

namespace SafeExamBrowser.UserInterface.Windows10
{
	public partial class BrowserWindow : Window // TODO, IBrowserWindow
	{
		//private bool isMainWindow;
		//private BrowserSettings settings;
		//public WindowClosingEventHandler closing;

		//public bool IsMainWindow
		//{
		//	get
		//	{
		//		return isMainWindow;
		//	}
		//	set
		//	{
		//		isMainWindow = value;
		//		ApplySettings();
		//	}
		//}

		//public event AddressChangedEventHandler AddressChanged;
		//public event ActionRequestedEventHandler BackwardNavigationRequested;
		//public event ActionRequestedEventHandler ForwardNavigationRequested;
		//public event ActionRequestedEventHandler ReloadRequested;

		//event WindowClosingEventHandler IWindow.Closing
		//{
		//	add { closing += value; }
		//	remove { closing -= value; }
		//}

		//public BrowserWindow(IBrowserControl browserControl, BrowserSettings settings)
		//{
		//	this.settings = settings;

		//	InitializeComponent();
		//	InitializeBrowserWindow(browserControl);
		//}

		//public void BringToForeground()
		//{
		//	Dispatcher.Invoke(() =>
		//	{
		//		if (WindowState == WindowState.Minimized)
		//		{
		//			WindowState = WindowState.Normal;
		//		}

		//		Activate();
		//	});
		//}

		//public new void Close()
		//{
		//	Dispatcher.Invoke(base.Close);
		//}

		//public new void Hide()
		//{
		//	Dispatcher.Invoke(base.Hide);
		//}

		//public new void Show()
		//{
		//	Dispatcher.Invoke(base.Show);
		//}

		//public void UpdateAddress(string url)
		//{
		//	Dispatcher.Invoke(() => UrlTextBox.Text = url);
		//}

		//public void UpdateTitle(string title)
		//{
		//	Dispatcher.Invoke(() => Title = title);
		//}

		//private void InitializeBrowserWindow(IBrowserControl browserControl)
		//{
		//	if (browserControl is System.Windows.Forms.Control control)
		//	{
		//		BrowserControlHost.Child = control;
		//	}

		//	Closing += (o, args) => closing?.Invoke();
		//	KeyUp += BrowserWindow_KeyUp;
		//	UrlTextBox.KeyUp += UrlTextBox_KeyUp;
		//	ReloadButton.Click += (o, args) => ReloadRequested?.Invoke();
		//	BackButton.Click += (o, args) => BackwardNavigationRequested?.Invoke();
		//	ForwardButton.Click += (o, args) => ForwardNavigationRequested?.Invoke();

		//	ApplySettings();
		//}

		//private void BrowserWindow_KeyUp(object sender, KeyEventArgs e)
		//{
		//	if (e.Key == Key.F5)
		//	{
		//		ReloadRequested?.Invoke();
		//	}
		//}

		//private void UrlTextBox_KeyUp(object sender, KeyEventArgs e)
		//{
		//	if (e.Key == Key.Enter)
		//	{
		//		AddressChanged?.Invoke(UrlTextBox.Text);
		//	}
		//}

		//private void ApplySettings()
		//{
		//	if (IsMainWindow && settings.FullScreenMode)
		//	{
		//		MaxHeight = SystemParameters.WorkArea.Height;
		//		ResizeMode = ResizeMode.NoResize;
		//		WindowState = WindowState.Maximized;
		//		WindowStyle = WindowStyle.None;
		//	}

		//	UrlTextBox.IsEnabled = settings.AllowAddressBar;

		//	ReloadButton.IsEnabled = settings.AllowReloading;
		//	ReloadButton.Visibility = settings.AllowReloading ? Visibility.Visible : Visibility.Collapsed;

		//	BackButton.IsEnabled = settings.AllowBackwardNavigation;
		//	BackButton.Visibility = settings.AllowBackwardNavigation ? Visibility.Visible : Visibility.Collapsed;

		//	ForwardButton.IsEnabled = settings.AllowForwardNavigation;
		//	ForwardButton.Visibility = settings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;
		//}
	}
}
