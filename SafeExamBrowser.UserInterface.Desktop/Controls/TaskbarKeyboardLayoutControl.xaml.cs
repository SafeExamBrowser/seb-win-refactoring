/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarKeyboardLayoutControl : UserControl, ISystemKeyboardLayoutControl
	{
		public event KeyboardLayoutSelectedEventHandler LayoutSelected;

		public TaskbarKeyboardLayoutControl()
		{
			InitializeComponent();
			InitializeKeyboardLayoutControl();
		}

		public void Add(IKeyboardLayout layout)
		{
			Dispatcher.Invoke(() =>
			{
				var button = new TaskbarKeyboardLayoutButton(layout);

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
				var name = layout.Name?.Length > 3 ? String.Join(string.Empty, layout.Name.Split(' ').Where(s => Char.IsLetter(s.First())).Select(s => s.First())) : layout.Name;

				foreach (var child in LayoutsStackPanel.Children)
				{
					if (child is TaskbarKeyboardLayoutButton layoutButton)
					{
						layoutButton.IsCurrent = layout.Id == layoutButton.LayoutId;
					}
				}

				LayoutCultureCode.Text = layout.CultureCode;
			});
		}

		public void SetTooltip(string text)
		{
			Dispatcher.Invoke(() => Button.ToolTip = text);
		}

		private void InitializeKeyboardLayoutControl()
		{
			var originalBrush = Button.Background;

			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = Popup.IsMouseOver));
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = IsMouseOver));

			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				Button.Background = Brushes.LightGray;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
			};
		}

		private void Button_LayoutSelected(Guid id)
		{
			Popup.IsOpen = false;
			LayoutSelected?.Invoke(id);
		}
	}
}
