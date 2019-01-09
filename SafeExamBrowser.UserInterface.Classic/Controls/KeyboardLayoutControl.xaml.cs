/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;

namespace SafeExamBrowser.UserInterface.Classic.Controls
{
	public partial class KeyboardLayoutControl : UserControl, ISystemKeyboardLayoutControl
	{
		public event KeyboardLayoutSelectedEventHandler LayoutSelected;

		public KeyboardLayoutControl()
		{
			InitializeComponent();
			InitializeKeyboardLayoutControl();
		}

		public void Add(IKeyboardLayout layout)
		{
			var button = new KeyboardLayoutButton();

			button.Click += (o, args) =>
			{
				SetCurrent(button, layout);
				Popup.IsOpen = false;
				LayoutSelected?.Invoke(layout);
			};
			button.CultureCode = layout.CultureCode;
			button.LayoutName = layout.Name;

			LayoutsStackPanel.Children.Add(button);

			if (layout.IsCurrent)
			{
				SetCurrent(button, layout);
			}
		}

		public void Close()
		{
			Popup.IsOpen = false;
		}

		public void SetTooltip(string text)
		{
			Button.ToolTip = text;
		}

		private void InitializeKeyboardLayoutControl()
		{
			var originalBrush = Button.Background;

			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Popup.IsOpen = Popup.IsMouseOver;
			Popup.MouseLeave += (o, args) => Popup.IsOpen = IsMouseOver;

			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightBlue;
				Button.Background = Brushes.LightBlue;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
			};
		}

		private void SetCurrent(KeyboardLayoutButton button, IKeyboardLayout layout)
		{
			var name = layout.Name?.Length > 3 ? String.Join(string.Empty, layout.Name.Split(' ').Where(s => Char.IsLetter(s.First())).Select(s => s.First())) : layout.Name;

			foreach (var child in LayoutsStackPanel.Children)
			{
				if (child is KeyboardLayoutButton keyboardLayoutButton)
				{
					keyboardLayoutButton.IsCurrent = false;
				}
			}

			button.IsCurrent = true;
			LayoutCultureCode.Text = layout.CultureCode;
			LayoutName.Text = name;
		}
	}
}
