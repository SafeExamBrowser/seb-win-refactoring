/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;
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
		private IText text;
		private WindowClosingEventHandler closing;

		private BrowserWindowSettings WindowSettings
		{
			get { return isMainWindow ? settings.MainWindowSettings : settings.AdditionalWindowSettings; }
		}

		public bool CanNavigateBackwards { set => Dispatcher.Invoke(() => BackwardButton.IsEnabled = value); }
		public bool CanNavigateForwards { set => Dispatcher.Invoke(() => ForwardButton.IsEnabled = value); }

		public event AddressChangedEventHandler AddressChanged;
		public event ActionRequestedEventHandler BackwardNavigationRequested;
		public event ActionRequestedEventHandler ForwardNavigationRequested;
		public event ActionRequestedEventHandler ReloadRequested;
		public event ActionRequestedEventHandler ZoomInRequested;
		public event ActionRequestedEventHandler ZoomOutRequested;
		public event ActionRequestedEventHandler ZoomResetRequested;

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public BrowserWindow(IBrowserControl browserControl, BrowserSettings settings, bool isMainWindow, IText text)
		{
			this.isMainWindow = isMainWindow;
			this.settings = settings;
			this.text = text;

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
			var originalBrush = MenuButton.Background;

			if (browserControl is System.Windows.Forms.Control control)
			{
				BrowserControlHost.Child = control;
			}

			BackwardButton.Click += (o, args) => BackwardNavigationRequested?.Invoke();
			Closing += (o, args) => closing?.Invoke();
			ForwardButton.Click += (o, args) => ForwardNavigationRequested?.Invoke();
			MenuButton.Click += (o, args) => MenuPopup.IsOpen = !MenuPopup.IsOpen;
			MenuButton.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => MenuPopup.IsOpen = MenuPopup.IsMouseOver));
			MenuPopup.Closed += (o, args) => { MenuButton.Background = originalBrush; };
			MenuPopup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => MenuPopup.IsOpen = IsMouseOver));
			MenuPopup.Opened += (o, args) => { MenuButton.Background = Brushes.LightGray; };
			KeyUp += BrowserWindow_KeyUp;
			ReloadButton.Click += (o, args) => ReloadRequested?.Invoke();
			UrlTextBox.GotKeyboardFocus += (o, args) => UrlTextBox.SelectAll();
			UrlTextBox.GotMouseCapture += UrlTextBox_GotMouseCapture;
			UrlTextBox.LostKeyboardFocus += (o, args) => UrlTextBox.Tag = null;
			UrlTextBox.LostFocus += (o, args) => UrlTextBox.Tag = null;
			UrlTextBox.KeyUp += UrlTextBox_KeyUp;
			UrlTextBox.MouseDoubleClick += (o, args) => UrlTextBox.SelectAll();
			ZoomInButton.Click += (o, args) => ZoomInRequested?.Invoke();
			ZoomOutButton.Click += (o, args) => ZoomOutRequested?.Invoke();
			ZoomResetButton.Click += (o, args) => ZoomResetRequested?.Invoke();

			ApplySettings();
			LoadIcons();
			LoadText();
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
			if (isMainWindow)
			{
				if (WindowSettings.FullScreenMode)
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

			UrlTextBox.IsEnabled = WindowSettings.AllowAddressBar;

			ReloadButton.IsEnabled = WindowSettings.AllowReloading;
			ReloadButton.Visibility = WindowSettings.AllowReloading ? Visibility.Visible : Visibility.Collapsed;

			BackwardButton.IsEnabled = WindowSettings.AllowBackwardNavigation;
			BackwardButton.Visibility = WindowSettings.AllowBackwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			ForwardButton.IsEnabled = WindowSettings.AllowForwardNavigation;
			ForwardButton.Visibility = WindowSettings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;
		}

		private void LoadIcons()
		{
			var backUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/NavigateBack.xaml");
			var forwardUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/NavigateForward.xaml");
			var menuUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/Menu.xaml");
			var reloadUri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/Reload.xaml");
			var backward = new XamlIconResource(backUri);
			var forward = new XamlIconResource(forwardUri);
			var menu = new XamlIconResource(menuUri);
			var reload = new XamlIconResource(reloadUri);

			BackwardButton.Content = IconResourceLoader.Load(backward);
			ForwardButton.Content = IconResourceLoader.Load(forward);
			MenuButton.Content = IconResourceLoader.Load(menu);
			ReloadButton.Content = IconResourceLoader.Load(reload);
		}

		private void LoadText()
		{
			ZoomText.Text = text.Get(TextKey.BrowserWindow_ZoomMenuItem);
		}
	}
}
