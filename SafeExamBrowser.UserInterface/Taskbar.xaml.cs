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
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class Taskbar : Window, ITaskbar
	{
		private ILogger logger;

		public Taskbar(ILogger logger)
		{
			this.logger = logger;

			InitializeComponent();

			Loaded += (o, args) => InitializeBounds();
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
			return Dispatcher.Invoke(() =>
			{
				var height = (int) TransformToPhysical(Width, Height).Y;

				logger.Info($"Calculated absolute taskbar height is {height}px.");

				return height;
			});
		}

		public void InitializeBounds()
		{
			Dispatcher.Invoke(() =>
			{
				Width = SystemParameters.WorkArea.Right;
				Left = SystemParameters.WorkArea.Right - Width;
				Top = SystemParameters.WorkArea.Bottom;

				var position = TransformToPhysical(Left, Top);
				var size = TransformToPhysical(Width, Height);

				logger.Info($"Set taskbar bounds to {Width}x{Height} at ({Left}/{Top}), in physical pixels: {size.X}x{size.Y} at ({position.X}/{position.Y}).");
			});
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

		private Vector TransformToPhysical(double x, double y)
		{
			// WPF works with device-independent pixels. The following code is required
			// to transform those values to their absolute, device-specific pixel value.
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

			return transformToDevice.Transform(new Vector(x, y));
		}
	}
}
