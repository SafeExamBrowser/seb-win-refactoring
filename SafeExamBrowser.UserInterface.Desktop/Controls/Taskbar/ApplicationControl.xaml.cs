/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.Taskbar
{
	internal partial class ApplicationControl : UserControl, IApplicationControl
	{
		private IApplication application;
		private IApplicationWindow single;

		internal ApplicationControl(IApplication application)
		{
			this.application = application;

			InitializeComponent();
			InitializeApplicationControl();
		}

		private void InitializeApplicationControl()
		{
			var originalBrush = Button.Background;

			application.WindowsChanged += Application_WindowsChanged;

			Button.Click += Button_Click;
			Button.Content = IconResourceLoader.Load(application.Icon);
			Button.MouseEnter += (o, args) => WindowPopup.IsOpen = WindowStackPanel.Children.Count > 0;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => WindowPopup.IsOpen = WindowPopup.IsMouseOver));
			Button.ToolTip = application.Tooltip;
			WindowPopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(WindowPopup_PlacementCallback);
			WindowPopup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => WindowPopup.IsOpen = IsMouseOver));

			WindowPopup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				Button.Background = Brushes.LightGray;
			};

			WindowPopup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
			};
		}

		private void Application_WindowsChanged()
		{
			Dispatcher.InvokeAsync(Update);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (WindowStackPanel.Children.Count == 0)
			{
				application.Start();
			}
			else if (WindowStackPanel.Children.Count == 1)
			{
				single?.Activate();
			}
		}

		private CustomPopupPlacement[] WindowPopup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
		{
			return new[]
			{
				new CustomPopupPlacement(new Point(targetSize.Width / 2 - popupSize.Width / 2, -popupSize.Height), PopupPrimaryAxis.None)
			};
		}

		private void Update()
		{
			var windows = application.GetWindows();

			WindowStackPanel.Children.Clear();

			foreach (var window in windows)
			{
				WindowStackPanel.Children.Add(new ApplicationWindowButton(window));
			}

			if (WindowStackPanel.Children.Count == 1)
			{
				single = windows.First();
			}
			else
			{
				single = default(IApplicationWindow);
			}
		}
	}
}
