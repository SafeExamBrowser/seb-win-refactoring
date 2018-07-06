/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Windows;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;
using SafeExamBrowser.UserInterface.Classic.Utilities;

namespace SafeExamBrowser.UserInterface.Classic
{
	public partial class Taskbar : Window, ITaskbar
	{
		private bool allowClose;
		private ILogger logger;

		public event QuitButtonClickedEventHandler QuitButtonClicked;

		public Taskbar(ILogger logger)
		{
			InitializeComponent();

			this.logger = logger;

			Closing += Taskbar_Closing;
			Loaded += (o, args) => InitializeBounds();
			QuitButton.Clicked += QuitButton_Clicked;
		}

		public void AddApplication(IApplicationButton button)
		{
			if (button is UIElement uiElement)
			{
				ApplicationStackPanel.Children.Add(uiElement);
			}
		}

		public void AddNotification(INotificationButton button)
		{
			if (button is UIElement uiElement)
			{
				NotificationStackPanel.Children.Add(uiElement);
			}
		}

		public void AddSystemControl(ISystemControl control)
		{
			if (control is UIElement uiElement)
			{
				SystemControlStackPanel.Children.Add(uiElement);
			}
		}

		public new void Close()
		{
			Dispatcher.Invoke(base.Close);
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

		private void QuitButton_Clicked(CancelEventArgs args)
		{
			QuitButtonClicked?.Invoke(args);
			allowClose = !args.Cancel;
		}

		private void Taskbar_Closing(object sender, CancelEventArgs e)
		{
			if (!allowClose)
			{
				e.Cancel = true;

				return;
			}

			foreach (var child in SystemControlStackPanel.Children)
			{
				if (child is ISystemControl systemControl)
				{
					systemControl.Close();
				}
			}
		}
	}
}
