/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	internal partial class Taskbar : Window, ITaskbar
	{
		private readonly ILogger logger;

		private bool allowClose;
		private bool isQuitButtonFocusedAtKeyDown;
		private bool isFirstChildFocusedAtKeyDown;

		public bool ShowClock
		{
			set { Dispatcher.Invoke(() => Clock.Visibility = value ? Visibility.Visible : Visibility.Collapsed); }
		}

		public bool ShowQuitButton
		{
			set { Dispatcher.Invoke(() => QuitButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed); }
		}

		public event LoseFocusRequestedEventHandler LoseFocusRequested;
		public event QuitButtonClickedEventHandler QuitButtonClicked;

		internal Taskbar(ILogger logger)
		{
			this.logger = logger;

			InitializeComponent();
			InitializeTaskbar();
		}

		public void AddApplicationControl(IApplicationControl control, bool atFirstPosition = false)
		{
			if (control is UIElement uiElement)
			{
				if (atFirstPosition)
				{
					ApplicationStackPanel.Children.Insert(0, uiElement);
				}
				else
				{
					ApplicationStackPanel.Children.Add(uiElement);
				}
			}
		}

		public void AddNotificationControl(INotificationControl control)
		{
			if (control is UIElement uiElement)
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

		public void Focus(bool forward)
		{
			Dispatcher.BeginInvoke((Action) (() =>
			{
				Activate();

				if (forward && ApplicationStackPanel.Children.Count > 0)
				{
					SetFocusWithin(ApplicationStackPanel.Children[0]);
				}
				else
				{
					QuitButton.Focus();
				}
			}));
		}

		public int GetAbsoluteHeight()
		{
			return Dispatcher.Invoke(() =>
			{
				var height = (int) this.TransformToPhysical(Width, Height).Y;

				logger.Debug($"Calculated physical taskbar height is {height}px.");

				return height;
			});
		}

		public int GetRelativeHeight()
		{
			return Dispatcher.Invoke(() =>
			{
				var height = (int) Height;

				logger.Debug($"Logical taskbar height is {height}px.");

				return height;
			});
		}

		public void InitializeBounds()
		{
			Dispatcher.Invoke(() =>
			{
				Width = SystemParameters.PrimaryScreenWidth;
				Left = 0;
				Top = SystemParameters.PrimaryScreenHeight - Height;

				var position = this.TransformToPhysical(Left, Top);
				var size = this.TransformToPhysical(Width, Height);

				logger.Debug($"Set taskbar bounds to {Width}x{Height} at ({Left}/{Top}), in physical pixels: {size.X}x{size.Y} at ({position.X}/{position.Y}).");
			});
		}

		private void InitializeTaskbar()
		{
			Closing += Taskbar_Closing;
			Loaded += (o, args) => InitializeBounds();
			QuitButton.Clicked += QuitButton_Clicked;
		}

		public void InitializeText(IText text)
		{
			Dispatcher.Invoke(() =>
			{
				var txt = text.Get(TextKey.Shell_QuitButton);
				QuitButton.ToolTip = txt;
				QuitButton.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, txt);
			});
		}

		public void Register(ITaskbarActivator activator)
		{
			activator.Activated += Activator_Activated;
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		private void Activator_Activated()
		{
			(this as ITaskbar).Focus(true);
		}

		private void QuitButton_Clicked(CancelEventArgs args)
		{
			QuitButtonClicked?.Invoke(args);
			allowClose = !args.Cancel;
		}

		private void Taskbar_Closing(object sender, CancelEventArgs e)
		{
			if (allowClose)
			{
				foreach (var child in SystemControlStackPanel.Children)
				{
					if (child is ISystemControl systemControl)
					{
						systemControl.Close();
					}
				}
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			isQuitButtonFocusedAtKeyDown = QuitButton.IsKeyboardFocusWithin;
			isFirstChildFocusedAtKeyDown = ApplicationStackPanel.Children.Count > 0 && ApplicationStackPanel.Children[0].IsKeyboardFocusWithin;
		}

		private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Tab)
			{
				var shift = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift);
				var hasFocus = ApplicationStackPanel.Children.Count > 0 && ApplicationStackPanel.Children[0].IsKeyboardFocusWithin;

				if (!shift && hasFocus && isQuitButtonFocusedAtKeyDown)
				{
					LoseFocusRequested?.Invoke(true);
					e.Handled = true;
				}
				else if (shift && QuitButton.IsKeyboardFocusWithin && isFirstChildFocusedAtKeyDown)
				{
					LoseFocusRequested?.Invoke(false);
					e.Handled = true;
				}
			}

			isQuitButtonFocusedAtKeyDown = false;
			isFirstChildFocusedAtKeyDown = false;
		}

		private bool SetFocusWithin(UIElement uIElement)
		{
			if (uIElement.Focusable)
			{
				uIElement.Focus();

				return true;
			}

			if (uIElement is System.Windows.Controls.Panel)
			{
				var panel = uIElement as System.Windows.Controls.Panel;

				for (var i = 0; i < panel.Children.Count; i++)
				{
					if (SetFocusWithin(panel.Children[i]))
					{
						return true;
					}
				}

				return false;
			}
			else if (uIElement is System.Windows.Controls.ContentControl)
			{
				var control = uIElement as System.Windows.Controls.ContentControl;
				var content = control.Content as UIElement;

				if (content != null)
				{
					return SetFocusWithin(content);
				}
			}

			return false;
		}
	}
}
