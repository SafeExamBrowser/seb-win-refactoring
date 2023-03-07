/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;
using Screen = System.Windows.Forms.Screen;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class LockScreen : Window, ILockScreen
	{
		private readonly AutoResetEvent autoResetEvent;
		private readonly IText text;

		private bool canceled;
		private IList<Window> windows;

		event WindowClosedEventHandler IWindow.Closed
		{
			add { throw new NotImplementedException(); }
			remove { throw new NotImplementedException(); }
		}

		event WindowClosingEventHandler IWindow.Closing
		{
			add { throw new NotImplementedException(); }
			remove { throw new NotImplementedException(); }
		}

		internal LockScreen(string message, string title, IText text, IEnumerable<LockScreenOption> options)
		{
			this.autoResetEvent = new AutoResetEvent(false);
			this.text = text;

			InitializeComponent();
			InitializeLockScreen(message, title, options);
		}

		public void BringToForeground()
		{
			Dispatcher.Invoke(Activate);
		}

		public void Cancel()
		{
			canceled = true;
			autoResetEvent.Set();
		}

		public new void Close()
		{
			Dispatcher.Invoke(CloseAll);
		}

		public void InitializeBounds()
		{
			Dispatcher.Invoke(() =>
			{
				foreach (var window in windows)
				{
					window.Topmost = false;
					window.WindowState = WindowState.Normal;
					window.Activate();
					window.Topmost = true;
					window.WindowState = WindowState.Maximized;
				}

				Topmost = false;
				WindowState = WindowState.Normal;
				Activate();
				Topmost = true;
				WindowState = WindowState.Maximized;
			});
		}

		public new void Show()
		{
			Dispatcher.Invoke(ShowAll);
		}

		public LockScreenResult WaitForResult()
		{
			var result = new LockScreenResult();

			autoResetEvent.WaitOne();

			Dispatcher.Invoke(() =>
			{
				result.Password = Password.Password;

				foreach (var child in Options.Children)
				{
					if (child is RadioButton option && option.IsChecked == true)
					{
						result.OptionId = option.Tag as Guid?;
					}
				}
			});

			result.Canceled = canceled;

			return result;
		}

		private void InitializeLockScreen(string message, string title, IEnumerable<LockScreenOption> options)
		{
			windows = new List<Window>();

			Button.Content = text.Get(TextKey.LockScreen_UnlockButton);
			Button.Click += Button_Click;
			Heading.Text = title;
			Loaded += (o, args) => Activate();
			Message.Text = message;
			Password.KeyDown += Password_KeyDown;

			foreach (var option in options)
			{
				Options.Children.Add(new RadioButton
				{
					Content = new TextBlock { Text = option.Text, TextWrapping = TextWrapping.Wrap },
					Cursor = Cursors.Hand,
					FontSize = Message.FontSize,
					Foreground = Message.Foreground,
					IsChecked = options.First() == option,
					Margin = new Thickness(5),
					Tag = option.Id,
					VerticalContentAlignment = VerticalAlignment.Center
				});
			}
		}

		private void CloseAll()
		{
			foreach (var window in windows)
			{
				window.Close();
			}

			base.Close();
		}

		private void ShowAll()
		{
			foreach (var screen in Screen.AllScreens)
			{
				if (!screen.Primary)
				{
					ShowLockWindowOn(screen);
				}
			}

			base.Show();
		}

		private void ShowLockWindowOn(Screen screen)
		{
			var window = new Window();
			var position = this.TransformFromPhysical(screen.Bounds.X, screen.Bounds.Y);
			var size = this.TransformFromPhysical(screen.Bounds.Width, screen.Bounds.Height);

			window.Background = Brushes.Red;
			window.Topmost = true;
			window.Left = position.X;
			window.Top = position.Y;
			window.Width = size.X;
			window.Height = size.Y;
			window.ResizeMode = ResizeMode.NoResize;
			window.WindowStyle = WindowStyle.None;

			window.Show();
			windows.Add(window);

			// The window can only be maximized after it was shown on its screen, otherwise it is rendered on the primary screen!
			window.WindowState = WindowState.Maximized;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			autoResetEvent.Set();
		}

		private void Password_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				autoResetEvent.Set();
			}
		}
	}
}
