/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class Taskbar : Window, ITaskbar
	{
		public Taskbar()
		{
			InitializeComponent();

			Loaded += Taskbar_Loaded;
		}

		private void Taskbar_Loaded(object sender, RoutedEventArgs e)
		{
			Width = SystemParameters.WorkArea.Right;
			Left = SystemParameters.WorkArea.Right - Width;
			Top = SystemParameters.WorkArea.Bottom;
		}

		public void AddButton(ITaskbarButton button)
		{
			if (button is UIElement)
			{
				ApplicationStackPanel.Children.Add(button as UIElement);
			}
		}

		public void AddNotification(ITaskbarNotification button)
		{
			if (button is UIElement)
			{
				NotificationWrapPanel.Children.Add(button as UIElement);
			}
		}

		public int GetAbsoluteHeight()
		{
			// WPF works with device-independent pixels. The following code is required
			// to get the real height of the taskbar (in absolute, device-specific pixels).
			// Source: https://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels

			Matrix transformToDevice;
			var source = PresentationSource.FromVisual(this);

			if (source != null)
			{
				transformToDevice = source.CompositionTarget.TransformToDevice;
			}
			else
			{
				using (var newSource = new HwndSource(new HwndSourceParameters()))
				{
					transformToDevice = newSource.CompositionTarget.TransformToDevice;
				}
			}

			return (int) transformToDevice.Transform((Vector) new Size(Width, Height)).Y;
		}

		private void ApplicationScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var scrollAmount = 20;

			if (ApplicationScrollViewer.IsMouseOver)
			{
				if (e.Delta < 0)
				{
					if (ApplicationScrollViewer.HorizontalOffset + scrollAmount > 0)
					{
						ApplicationScrollViewer.ScrollToHorizontalOffset(ApplicationScrollViewer.HorizontalOffset + scrollAmount);
					}
					else
					{
						ApplicationScrollViewer.ScrollToLeftEnd();
					}
				}
				else
				{
					if (ApplicationScrollViewer.ExtentWidth > ApplicationScrollViewer.HorizontalOffset - scrollAmount)
					{
						ApplicationScrollViewer.ScrollToHorizontalOffset(ApplicationScrollViewer.HorizontalOffset - scrollAmount);
					}
					else
					{
						ApplicationScrollViewer.ScrollToRightEnd();
					}
				}
			}
		}
	}
}
