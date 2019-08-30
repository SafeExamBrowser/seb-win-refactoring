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
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarKeyboardLayoutControl : UserControl, ISystemControl
	{
		private IKeyboard keyboard;
		private IText text;

		public TaskbarKeyboardLayoutControl(IKeyboard keyboard, IText text)
		{
			this.keyboard = keyboard;
			this.text = text;

			InitializeComponent();
			InitializeKeyboardLayoutControl();
		}

		public void Close()
		{
			Dispatcher.Invoke(() => Popup.IsOpen = false);
		}

		private void InitializeKeyboardLayoutControl()
		{
			var originalBrush = Button.Background;

			InitializeLayouts();

			keyboard.LayoutChanged += Keyboard_LayoutChanged;
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

		private void Keyboard_LayoutChanged(IKeyboardLayout layout)
		{
			Dispatcher.InvokeAsync(() => SetCurrent(layout));
		}

		private void InitializeLayouts()
		{
			foreach (var layout in keyboard.GetLayouts())
			{
				var button = new TaskbarKeyboardLayoutButton(layout);

				button.LayoutSelected += (o, args) => ActivateLayout(layout);
				LayoutsStackPanel.Children.Add(button);

				if (layout.IsCurrent)
				{
					SetCurrent(layout);
				}
			}
		}

		private void ActivateLayout(IKeyboardLayout layout)
		{
			Popup.IsOpen = false;
			keyboard.ActivateLayout(layout.Id);
		}

		private void SetCurrent(IKeyboardLayout layout)
		{
			var name = layout.Name?.Length > 3 ? String.Join(string.Empty, layout.Name.Split(' ').Where(s => Char.IsLetter(s.First())).Select(s => s.First())) : layout.Name;
			var tooltip = text.Get(TextKey.SystemControl_KeyboardLayoutTooltip).Replace("%%LAYOUT%%", layout.Name);

			foreach (var child in LayoutsStackPanel.Children)
			{
				if (child is TaskbarKeyboardLayoutButton layoutButton)
				{
					layoutButton.IsCurrent = layout.Id == layoutButton.LayoutId;
				}
			}

			LayoutCultureCode.Text = layout.CultureCode;
			Button.ToolTip = tooltip;
		}
	}
}
