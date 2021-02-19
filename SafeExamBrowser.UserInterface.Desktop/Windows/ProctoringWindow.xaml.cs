/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Windows;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Events;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class ProctoringWindow : Window, IProctoringWindow
	{
		private WindowClosingEventHandler closing;

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

		private void ProctoringWindow_Closing(object sender, CancelEventArgs e)
		{
			closing?.Invoke();
		}

		private void InitializeWindow(object control)
		{
			if (control is UIElement element)
			{
				ControlContainer.Children.Add(element);
			}

			Closing += ProctoringWindow_Closing;
		}
	}
}
