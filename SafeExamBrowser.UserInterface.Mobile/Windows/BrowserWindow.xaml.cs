/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;
using SafeExamBrowser.UserInterface.Mobile.Controls.Browser;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class BrowserWindow : Window, IBrowserWindow
	{
		private readonly bool isMainWindow;
		private readonly ILogger logger;
		private readonly BrowserSettings settings;
		private readonly IText text;

		private WindowClosedEventHandler closed;
		private WindowClosingEventHandler closing;

		private WindowSettings WindowSettings
		{
			get { return isMainWindow ? settings.MainWindow : settings.AdditionalWindow; }
		}

		public bool CanNavigateBackwards { set => Dispatcher.Invoke(() => BackwardButton.IsEnabled = value); }
		public bool CanNavigateForwards { set => Dispatcher.Invoke(() => ForwardButton.IsEnabled = value); }
		public IntPtr Handle { get; private set; }

		public event AddressChangedEventHandler AddressChanged;
		public event ActionRequestedEventHandler BackwardNavigationRequested;
		public event ActionRequestedEventHandler DeveloperConsoleRequested;
		public event FindRequestedEventHandler FindRequested;
		public event ActionRequestedEventHandler ForwardNavigationRequested;
		public event LoseFocusRequestedEventHandler LoseFocusRequested { add { } remove { } }
		public event ActionRequestedEventHandler HomeNavigationRequested;
		public event ActionRequestedEventHandler ReloadRequested;
		public event ActionRequestedEventHandler ZoomInRequested;
		public event ActionRequestedEventHandler ZoomOutRequested;
		public event ActionRequestedEventHandler ZoomResetRequested;

		event WindowClosedEventHandler IWindow.Closed
		{
			add { closed += value; }
			remove { closed -= value; }
		}

		event WindowClosingEventHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		internal BrowserWindow(IBrowserControl browserControl, BrowserSettings settings, bool isMainWindow, IText text, ILogger logger)
		{
			this.isMainWindow = isMainWindow;
			this.logger = logger;
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

		public void ShowFindbar()
		{
			Dispatcher.InvokeAsync(() =>
			{
				Findbar.Visibility = Visibility.Visible;
				FindTextBox.Focus();
			});
		}

		public void UpdateAddress(string url)
		{
			Dispatcher.Invoke(() => UrlTextBox.Text = url);
		}

		public void UpdateIcon(IconResource icon)
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (icon is BitmapIconResource bitmap)
				{
					Icon = new BitmapImage(bitmap.Uri);
				}
			});
		}

		public void UpdateDownloadState(DownloadItemState state)
		{
			Dispatcher.InvokeAsync(() =>
			{
				var isNewItem = true;

				foreach (var child in Downloads.Children)
				{
					if (child is DownloadItemControl control && control.Id == state.Id)
					{
						control.Update(state);
						isNewItem = false;

						break;
					}
				}

				if (isNewItem)
				{
					var control = new DownloadItemControl(state.Id, text);

					control.Update(state);
					Downloads.Children.Add(control);
				}

				DownloadsButton.Visibility = Visibility.Visible;
				DownloadsPopup.IsOpen = IsActive;
			});
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

			if (e.Key == Key.Home)
			{
				HomeNavigationRequested?.Invoke();
			}

			if (settings.AllowFind && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.F)
			{
				ShowFindbar();
			}
		}

		private void BrowserWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Handle = new WindowInteropHelper(this).Handle;

			if (isMainWindow)
			{
				this.DisableCloseButton();
			}
		}

		private void FindbarCloseButton_Click(object sender, RoutedEventArgs e)
		{
			FindRequested?.Invoke("thisisahacktoclearthesearchresultsasitappearsthatthereisnosuchfunctionalityincef", true, false);
			Findbar.Visibility = Visibility.Collapsed;
		}

		private void FindNextButton_Click(object sender, RoutedEventArgs e)
		{
			FindRequested?.Invoke(FindTextBox.Text, false, FindCaseSensitiveCheckBox.IsChecked == true);
		}

		private void FindPreviousButton_Click(object sender, RoutedEventArgs e)
		{
			FindRequested?.Invoke(FindTextBox.Text, false, FindCaseSensitiveCheckBox.IsChecked == true, false);
		}

		private void FindTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (string.IsNullOrEmpty(FindTextBox.Text))
			{
				FindRequested?.Invoke("thisisahacktoclearthesearchresultsasitappearsthatthereisnosuchfunctionalityincef", true, false);
			}
			else
			{
				FindRequested?.Invoke(FindTextBox.Text, true, FindCaseSensitiveCheckBox.IsChecked == true);
			}
		}

		private CustomPopupPlacement[] Popup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
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
			if (browserControl.EmbeddableControl is System.Windows.Forms.Control control)
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
			BackwardButton.Click += (o, args) => BackwardNavigationRequested?.Invoke();
			Closed += (o, args) => closed?.Invoke();
			Closing += BrowserWindow_Closing;
			DeveloperConsoleButton.Click += (o, args) => DeveloperConsoleRequested?.Invoke();
			DownloadsButton.Click += (o, args) => DownloadsPopup.IsOpen = !DownloadsPopup.IsOpen;
			DownloadsButton.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => DownloadsPopup.IsOpen = DownloadsPopup.IsMouseOver));
			DownloadsPopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
			DownloadsPopup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => DownloadsPopup.IsOpen = DownloadsPopup.IsMouseOver));
			FindbarCloseButton.Click += FindbarCloseButton_Click;
			FindNextButton.Click += FindNextButton_Click;
			FindPreviousButton.Click += FindPreviousButton_Click;
			FindMenuButton.Click += (o, args) => ShowFindbar();
			FindTextBox.KeyUp += FindTextBox_KeyUp;
			ForwardButton.Click += (o, args) => ForwardNavigationRequested?.Invoke();
			HomeButton.Click += (o, args) => HomeNavigationRequested?.Invoke();
			Loaded += BrowserWindow_Loaded;
			MenuButton.Click += (o, args) => MenuPopup.IsOpen = !MenuPopup.IsOpen;
			MenuButton.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => MenuPopup.IsOpen = MenuPopup.IsMouseOver));
			MenuPopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
			MenuPopup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => MenuPopup.IsOpen = MenuPopup.IsMouseOver));
			KeyUp += BrowserWindow_KeyUp;
			LocationChanged += (o, args) => { DownloadsPopup.IsOpen = false; MenuPopup.IsOpen = false; };
			ReloadButton.Click += (o, args) => ReloadRequested?.Invoke();
			SizeChanged += (o, args) => { DownloadsPopup.IsOpen = false; MenuPopup.IsOpen = false; };
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
			FindMenuItem.Visibility = settings.AllowFind ? Visibility.Visible : Visibility.Collapsed;
			ForwardButton.IsEnabled = WindowSettings.AllowForwardNavigation;
			ForwardButton.Visibility = WindowSettings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;
			HomeButton.IsEnabled = WindowSettings.ShowHomeButton;
			HomeButton.Visibility = WindowSettings.ShowHomeButton ? Visibility.Visible : Visibility.Collapsed;
			MenuButton.IsEnabled = settings.AllowPageZoom || WindowSettings.AllowDeveloperConsole;
			ReloadButton.IsEnabled = WindowSettings.AllowReloading;
			ReloadButton.Visibility = WindowSettings.ShowReloadButton ? Visibility.Visible : Visibility.Collapsed;
			Toolbar.Visibility = WindowSettings.ShowToolbar ? Visibility.Visible : Visibility.Collapsed;
			UrlTextBox.Visibility = WindowSettings.AllowAddressBar ? Visibility.Visible : Visibility.Hidden;
			ZoomMenuItem.Visibility = settings.AllowPageZoom ? Visibility.Visible : Visibility.Collapsed;

			if (!WindowSettings.AllowAddressBar)
			{
				BackwardButton.Height = 35;
				ForwardButton.Height = 35;
				ReloadButton.Height = 35;
				UrlTextBox.Height = 20;
				DownloadsButton.Height = 35;
				MenuButton.Height = 35;
			}
		}

		private void InitializeBounds()
		{
			if (isMainWindow && WindowSettings.FullScreenMode)
			{
				Top = 0;
				Left = 0;
				Height = SystemParameters.WorkArea.Height;
				Width = SystemParameters.WorkArea.Width;
				ResizeMode = ResizeMode.NoResize;
				WindowStyle = WindowStyle.None;
			}
			else if (WindowSettings.RelativeHeight == 100 && WindowSettings.RelativeWidth == 100)
			{
				WindowState = WindowState.Maximized;
			}
			else
			{
				if (WindowSettings.RelativeHeight > 0)
				{
					Height = SystemParameters.WorkArea.Height * WindowSettings.RelativeHeight.Value / 100;
					Top = (SystemParameters.WorkArea.Height / 2) - (Height / 2);
				}
				else if (WindowSettings.AbsoluteHeight > 0)
				{
					Height = this.TransformFromPhysical(0, WindowSettings.AbsoluteHeight.Value).Y;
					Top = (SystemParameters.WorkArea.Height / 2) - (Height / 2);
				}

				if (WindowSettings.RelativeWidth > 0)
				{
					Width = SystemParameters.WorkArea.Width * WindowSettings.RelativeWidth.Value / 100;
				}
				else if (WindowSettings.AbsoluteWidth > 0)
				{
					Width = this.TransformFromPhysical(WindowSettings.AbsoluteWidth.Value, 0).X;
				}

				if (Height > SystemParameters.WorkArea.Height)
				{
					Top = 0;
					Height = SystemParameters.WorkArea.Height;
				}

				if (Width > SystemParameters.WorkArea.Width)
				{
					Left = 0;
					Width = SystemParameters.WorkArea.Width;
				}

				switch (WindowSettings.Position)
				{
					case WindowPosition.Left:
						Left = 0;
						break;
					case WindowPosition.Center:
						Left = (SystemParameters.WorkArea.Width / 2) - (Width / 2);
						break;
					case WindowPosition.Right:
						Left = SystemParameters.WorkArea.Width - Width;
						break;
				}
			}
		}

		private void LoadIcons()
		{
			var backward = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/NavigateBack.xaml") };
			var forward = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/NavigateForward.xaml") };
			var home = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/Home.xaml") };
			var menu = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/Menu.xaml") };
			var reload = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/Reload.xaml") };

			BackwardButton.Content = IconResourceLoader.Load(backward);
			ForwardButton.Content = IconResourceLoader.Load(forward);
			HomeButton.Content = IconResourceLoader.Load(home);
			MenuButton.Content = IconResourceLoader.Load(menu);
			ReloadButton.Content = IconResourceLoader.Load(reload);
		}

		private void LoadText()
		{
			DeveloperConsoleText.Text = text.Get(TextKey.BrowserWindow_DeveloperConsoleMenuItem);
			FindCaseSensitiveCheckBox.Content = text.Get(TextKey.BrowserWindow_FindCaseSensitive);
			FindMenuText.Text = text.Get(TextKey.BrowserWindow_FindMenuItem);
			ZoomText.Text = text.Get(TextKey.BrowserWindow_ZoomMenuItem);
		}

		public void FocusToolbar(bool forward)
		{
			throw new NotImplementedException();
		}

		public void FocusBrowser()
		{
			throw new NotImplementedException();
		}

		public void Debug()
		{
			throw new NotImplementedException();
		}

		public void FocusAddressBar()
		{
			this.UrlTextBox.Focus();
		}
	}
}
