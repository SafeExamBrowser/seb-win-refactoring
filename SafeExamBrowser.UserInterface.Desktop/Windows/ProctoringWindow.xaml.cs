/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class ProctoringWindow : Window, IProctoringWindow
	{
		private WindowClosedEventHandler closed;
		private WindowClosingEventHandler closing;

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

		public ProctoringWindow(IProctoringControl control)
		{
			InitializeComponent();
			InitializeWindow(control);
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
				Closing -= ProctoringWindow_Closing;
				closing?.Invoke();
				base.Close();
			});
		}

		public new void Hide()
		{
			Dispatcher.Invoke(base.Hide);
		}

		public void HideWithDelay()
		{
			Dispatcher.Invoke(() => this.MoveToBackground());
			Task.Delay(15000).ContinueWith(_ => Hide());
		}

		public void SetTitle(string title)
		{
			Dispatcher.Invoke(() => Title = title ?? "");
		}

		public new void Show()
		{
			Dispatcher.Invoke(base.Show);
		}

		public void Toggle()
		{
			Dispatcher.Invoke(() =>
			{
				if (Visibility == Visibility.Visible)
				{
					base.Hide();
				}
				else
				{
					base.Show();
				}
			});
		}

		private void InitializeWindow(IProctoringControl control)
		{
			if (control is UIElement element)
			{
				Content = element;
				control.FullScreenChanged += Control_FullScreenChanged;
			}

			Closed += (o, args) => closed?.Invoke();
			Closing += ProctoringWindow_Closing;
			Loaded += ProctoringWindow_Loaded;
			Top = SystemParameters.WorkArea.Height - Height - 15;
			Left = SystemParameters.WorkArea.Width - Width - 20;
		}

		private void Control_FullScreenChanged(bool fullScreen)
		{
			if (fullScreen)
			{
				WindowState = WindowState.Maximized;
				WindowStyle = WindowStyle.None;
			}
			else
			{
				WindowState = WindowState.Normal;
				WindowStyle = WindowStyle.ToolWindow;
			}
		}

		private void ProctoringWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void ProctoringWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.HideCloseButton();
		}
	}
}
