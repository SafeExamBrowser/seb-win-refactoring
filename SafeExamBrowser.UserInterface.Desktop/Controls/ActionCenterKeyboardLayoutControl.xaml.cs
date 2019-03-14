/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class ActionCenterKeyboardLayoutControl : UserControl, ISystemKeyboardLayoutControl
	{
		public event KeyboardLayoutSelectedEventHandler LayoutSelected;

		public ActionCenterKeyboardLayoutControl()
		{
			InitializeComponent();
			InitializeKeyboardLayoutControl();
		}

		public void Add(IKeyboardLayout layout)
		{
			Dispatcher.Invoke(() =>
			{
				var button = new ActionCenterKeyboardLayoutButton(layout);

				button.LayoutSelected += Button_LayoutSelected;
				button.CultureCode = layout.CultureCode;
				button.LayoutName = layout.Name;

				LayoutsStackPanel.Children.Add(button);
			});
		}

		public void Close()
		{
			Dispatcher.Invoke(() => Popup.IsOpen = false);
		}

		public void SetCurrent(IKeyboardLayout layout)
		{
			Dispatcher.Invoke(() =>
			{
				foreach (var child in LayoutsStackPanel.Children)
				{
					if (child is ActionCenterKeyboardLayoutButton layoutButton)
					{
						layoutButton.IsCurrent = layout.Id == layoutButton.LayoutId;
					}
				}

				Text.Text = layout.Name;
			});
		}

		public void SetInformation(string text)
		{
			Dispatcher.Invoke(() => Button.ToolTip = text);
		}

		private void InitializeKeyboardLayoutControl()
		{
			var originalBrush = Grid.Background;

			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = Popup.IsMouseOver));
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = IsMouseOver));
			Popup.Opened += (o, args) => Grid.Background = Brushes.Gray;
			Popup.Closed += (o, args) => Grid.Background = originalBrush;
		}

		private void Button_LayoutSelected(Guid id)
		{
			Popup.IsOpen = false;
			LayoutSelected?.Invoke(id);
		}
	}
}
