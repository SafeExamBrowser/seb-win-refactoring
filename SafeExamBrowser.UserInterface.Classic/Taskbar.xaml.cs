/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.UserInterface.Classic.Utilities;

namespace SafeExamBrowser.UserInterface.Classic
{
	public partial class Taskbar : Window, ITaskbar
	{
		private ILogger logger;

		public Taskbar(ILogger logger)
		{
			InitializeComponent();

			this.logger = logger;

			Loaded += (o, args) => InitializeBounds();
			Closing += Taskbar_Closing;
		}

		public void AddApplication(IApplicationButton button)
		{
			if (button is UIElement)
			{
				ApplicationStackPanel.Children.Add(button as UIElement);
			}
		}

		public void AddNotification(INotificationButton button)
		{
			if (button is UIElement)
			{
				NotificationStackPanel.Children.Add(button as UIElement);
			}
		}

		public void AddSystemControl(ISystemControl control)
		{
			if (control is UIElement)
			{
				SystemControlStackPanel.Children.Add(control as UIElement);
			}
		}

		public int GetAbsoluteHeight()
		{
			return Dispatcher.Invoke(() =>
			{
				var height = (int) this.TransformToPhysical(Width, Height).Y;

				logger.Info($"Calculated physical taskbar height is {height}px.");

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

				var position = this.TransformToPhysical(Left, Top);
				var size = this.TransformToPhysical(Width, Height);

				logger.Info($"Set taskbar bounds to {Width}x{Height} at ({Left}/{Top}), in physical pixels: {size.X}x{size.Y} at ({position.X}/{position.Y}).");
			});
		}

		private void Taskbar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			foreach (var child in SystemControlStackPanel.Children)
			{
				if (child is ISystemControl)
				{
					(child as ISystemControl).Close();
				}
			}
		}
	}
}
