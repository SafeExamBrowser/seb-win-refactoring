/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.Utilities;

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
				NotificationStackPanel.Children.Add(button as UIElement);
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
	}
}
