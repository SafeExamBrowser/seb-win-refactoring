/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Configuration.Contracts.Settings.Browser;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

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
		public event ActionRequestedEventHandler DeveloperConsoleRequested;
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
			Dispatcher.Invoke(() =>
			{
				Closing -= BrowserWindow_Closing;
				closing?.Invoke();
				base.Close();
			});
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
			Dispatcher.InvokeAsync(() => Icon = new BitmapImage(icon.Uri));
		}

		public void UpdateLoadingState(bool isLoading)
		{
			Dispatcher.Invoke(() => ProgressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Hidden);
		}

		public void UpdateProgress(double value)
		{
			Dispatcher.Invoke(() => ProgressBar.Value = value * 100);
		}

		public void UpdateTitle(string title)
		{
			Dispatcher.Invoke(() => Title = title);
		}

		public void UpdateZoomLevel(double value)
		{
			Dispatcher.Invoke(() => ZoomLevel.Text = $"{value}%");
		}

		private void BrowserWindow_Closing(object sender, CancelEventArgs e)
		{
			if (isMainWindow)
			{
				e.Cancel = true;
			}
			else
			{
				closing?.Invoke();
			}
		}

		private void BrowserWindow_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F5)
			{
				ReloadRequested?.Invoke();
			}
		}

		private void BrowserWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (isMainWindow)
			{
				WindowUtility.DisableCloseButtonFor(this);
			}
		}

		private CustomPopupPlacement[] MenuPopup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
		{
			return new[]
			{
				new CustomPopupPlacement(new Point(targetSize.Width - Toolbar.Margin.Right - popupSize.Width, -2), PopupPrimaryAxis.None)
			};
		}

		private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SystemParameters.WorkArea))
			{
				Dispatcher.InvokeAsync(InitializeBounds);
			}
		}

		private void UrlTextBox_GotMouseCapture(object sender, MouseEventArgs e)
		{
			if (UrlTextBox.Tag as bool? != true)
			{
				UrlTextBox.SelectAll();
				UrlTextBox.Tag = true;
			}
		}

		private void UrlTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AddressChanged?.Invoke(UrlTextBox.Text);
			}
		}

		private void InitializeBrowserWindow(IBrowserControl browserControl)
		{
			if (browserControl is System.Windows.Forms.Control control)
			{
				BrowserControlHost.Child = control;
			}

			RegisterEvents();
			InitializeBounds();
			ApplySettings();
			LoadIcons();
			LoadText();
		}

		private void RegisterEvents()
		{
			var originalBrush = MenuButton.Background;

			BackwardButton.Click += (o, args) => BackwardNavigationRequested?.Invoke();
			Closing += BrowserWindow_Closing;
			DeveloperConsoleButton.Click += (o, args) => DeveloperConsoleRequested?.Invoke();
			ForwardButton.Click += (o, args) => ForwardNavigationRequested?.Invoke();
			Loaded += BrowserWindow_Loaded;
			MenuButton.Click += (o, args) => MenuPopup.IsOpen = !MenuPopup.IsOpen;
			MenuButton.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => MenuPopup.IsOpen = MenuPopup.IsMouseOver));
			MenuPopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(MenuPopup_PlacementCallback);
			MenuPopup.Closed += (o, args) => MenuButton.Background = originalBrush;
			MenuPopup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => MenuPopup.IsOpen = IsMouseOver));
			MenuPopup.Opened += (o, args) => MenuButton.Background = Brushes.LightGray;
			KeyUp += BrowserWindow_KeyUp;
			ReloadButton.Click += (o, args) => ReloadRequested?.Invoke();
			SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
			UrlTextBox.GotKeyboardFocus += (o, args) => UrlTextBox.SelectAll();
			UrlTextBox.GotMouseCapture += UrlTextBox_GotMouseCapture;
			UrlTextBox.LostKeyboardFocus += (o, args) => UrlTextBox.Tag = null;
			UrlTextBox.LostFocus += (o, args) => UrlTextBox.Tag = null;
			UrlTextBox.KeyUp += UrlTextBox_KeyUp;
			UrlTextBox.MouseDoubleClick += (o, args) => UrlTextBox.SelectAll();
			ZoomInButton.Click += (o, args) => ZoomInRequested?.Invoke();
			ZoomOutButton.Click += (o, args) => ZoomOutRequested?.Invoke();
			ZoomResetButton.Click += (o, args) => ZoomResetRequested?.Invoke();
		}

		private void ApplySettings()
		{
			BackwardButton.IsEnabled = WindowSettings.AllowBackwardNavigation;
			BackwardButton.Visibility = WindowSettings.AllowBackwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			DeveloperConsoleMenuItem.Visibility = WindowSettings.AllowDeveloperConsole ? Visibility.Visible : Visibility.Collapsed;

			ForwardButton.IsEnabled = WindowSettings.AllowForwardNavigation;
			ForwardButton.Visibility = WindowSettings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			ReloadButton.IsEnabled = WindowSettings.AllowReloading;
			ReloadButton.Visibility = WindowSettings.AllowReloading ? Visibility.Visible : Visibility.Collapsed;

			UrlTextBox.Visibility = WindowSettings.AllowAddressBar ? Visibility.Visible : Visibility.Hidden;
		}

		private void InitializeBounds()
		{
			if (isMainWindow)
			{
				if (WindowSettings.FullScreenMode)
				{
					Top = 0;
					Left = 0;
					Height = SystemParameters.WorkArea.Height;
					Width = SystemParameters.WorkArea.Width;
					ResizeMode = ResizeMode.NoResize;
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
			DeveloperConsoleText.Text = text.Get(TextKey.BrowserWindow_DeveloperConsoleMenuItem);
			ZoomText.Text = text.Get(TextKey.BrowserWindow_ZoomMenuItem);
		}
	}
}
