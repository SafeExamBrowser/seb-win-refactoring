/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.UserInterface.Browser;
using SafeExamBrowser.Contracts.UserInterface.Browser.Events;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.UserInterface.Desktop.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public partial class BrowserWindow : Window, IBrowserWindow
	{
		private bool isMainWindow;
		private BrowserSettings settings;
		public WindowClosingEventHandler closing;

		public bool IsMainWindow
		{
			get
			{
				return isMainWindow;
			}
			set
			{
				isMainWindow = value;
				ApplySettings();
			}
		}

		public event AddressChangedEventHandler AddressChanged;
		public event ActionRequestedEventHandler BackwardNavigationRequested;
		public event ActionRequestedEventHandler ForwardNavigationRequested;
		public event ActionRequestedEventHandler ReloadRequested;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public BrowserWindow(IBrowserControl browserControl, BrowserSettings settings)
		{
			this.settings = settings;

			InitializeComponent();
			InitializeBrowserWindow(browserControl);
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(() =>
			{
				if (WindowState == WindowState.Minimized)
				{
					WindowState = WindowState.Normal;
				}

				Activate();
			});
		}

		public new void Close()
		{
			Dispatcher.Invoke(base.Close);
		}

		public new void Hide()
		{
			Dispatcher.Invoke(base.Hide);
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		public void UpdateAddress(string url)
		{
			Dispatcher.Invoke(() => UrlTextBox.Text = url);
		}

		public void UpdateIcon(IIconResource icon)
		{
			Dispatcher.BeginInvoke(new Action(() => Icon = new BitmapImage(icon.Uri)));
		}

		public void UpdateLoadingState(bool isLoading)
		{
			Dispatcher.Invoke(() =>
			{
				LoadingIcon.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
				LoadingIcon.Spin = isLoading;
			});
		}

		public void UpdateTitle(string title)
		{
			Dispatcher.Invoke(() => Title = title);
		}

		private void InitializeBrowserWindow(IBrowserControl browserControl)
		{
			if (browserControl is System.Windows.Forms.Control control)
			{
				BrowserControlHost.Child = control;
			}

			Closing += (o, args) => closing?.Invoke();
			KeyUp += BrowserWindow_KeyUp;
			UrlTextBox.GotKeyboardFocus += (o, args) => UrlTextBox.SelectAll();
			UrlTextBox.GotMouseCapture += UrlTextBox_GotMouseCapture;
			UrlTextBox.LostKeyboardFocus += (o, args) => UrlTextBox.Tag = null;
			UrlTextBox.LostFocus += (o, args) => UrlTextBox.Tag = null;
			UrlTextBox.KeyUp += UrlTextBox_KeyUp;
			UrlTextBox.MouseDoubleClick += (o, args) => UrlTextBox.SelectAll();
			ReloadButton.Click += (o, args) => ReloadRequested?.Invoke();
			BackButton.Click += (o, args) => BackwardNavigationRequested?.Invoke();
			ForwardButton.Click += (o, args) => ForwardNavigationRequested?.Invoke();

			ApplySettings();
			LoadIcons();
		}

		private void UrlTextBox_GotMouseCapture(object sender, MouseEventArgs e)
		{
			if (UrlTextBox.Tag as bool? != true)
			{
				UrlTextBox.SelectAll();
				UrlTextBox.Tag = true;
			}
		}

		private void BrowserWindow_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F5)
			{
				ReloadRequested?.Invoke();
			}
		}

		private void UrlTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AddressChanged?.Invoke(UrlTextBox.Text);
			}
		}

		private void ApplySettings()
		{
			if (IsMainWindow)
			{
				if (settings.FullScreenMode)
				{
					MaxHeight = SystemParameters.WorkArea.Height;
					ResizeMode = ResizeMode.NoResize;
					WindowState = WindowState.Maximized;
					WindowStyle = WindowStyle.None;
				}
				else
				{
					WindowState = WindowState.Maximized;
				}
			}
			else
			{
				Top = 0;
				Left = SystemParameters.WorkArea.Width / 2;
				Height = SystemParameters.WorkArea.Height;
				Width = SystemParameters.WorkArea.Width / 2;
			}

			UrlTextBox.IsEnabled = settings.AllowAddressBar;

			ReloadButton.IsEnabled = settings.AllowReloading;
			ReloadButton.Visibility = settings.AllowReloading ? Visibility.Visible : Visibility.Collapsed;

			BackButton.IsEnabled = settings.AllowBackwardNavigation;
			BackButton.Visibility = settings.AllowBackwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			ForwardButton.IsEnabled = settings.AllowForwardNavigation;
			ForwardButton.Visibility = settings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;
		}

		private void LoadIcons()
		{
			var backUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/NavigateBack.xaml");
			var forwardUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/NavigateForward.xaml");
			var reloadUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/Reload.xaml");
			var back = new XamlIconResource(backUri);
			var forward = new XamlIconResource(forwardUri);
			var reload = new XamlIconResource(reloadUri);

			ReloadButton.Content = IconResourceLoader.Load(reload);
			BackButton.Content = IconResourceLoader.Load(back);
			ForwardButton.Content = IconResourceLoader.Load(forward);
		}
	}
}
