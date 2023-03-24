/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskbar
{
	internal partial class KeyboardLayoutControl : UserControl, ISystemControl
	{
		private readonly IKeyboard keyboard;
		private readonly IText text;

		internal KeyboardLayoutControl(IKeyboard keyboard, IText text)
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
			Button.Click += (o, args) =>
			{
				Popup.IsOpen = !Popup.IsOpen;
				Task.Delay(200).ContinueWith(_ => this.Dispatcher.BeginInvoke((System.Action) (() =>
				{
					((LayoutsStackPanel.Children[0] as ContentControl).Content as UIElement).Focus();
				})));
			};
			var lastOpenedBySpacePress = false;
			Button.PreviewKeyDown += (o, args) =>
			{
				// For some reason, the popup immediately closes again if opened by a Space Bar key event - as a mitigation,
				// we record the space bar event and leave the popup open for at least 3 seconds.
				if (args.Key == System.Windows.Input.Key.Space)
				{
					lastOpenedBySpacePress = true;
				}
			};
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() =>
			{
				if (Popup.IsOpen && lastOpenedBySpacePress)
				{
					return;
				}
				Popup.IsOpen = Popup.IsMouseOver;
			}));
			Popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() =>
			{
				if (Popup.IsOpen && lastOpenedBySpacePress)
				{
					return;
				}
				Popup.IsOpen = IsMouseOver;
			}));

			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				Button.Background = Brushes.LightGray;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
				lastOpenedBySpacePress = false;
			};
		}

		private void Keyboard_LayoutChanged(IKeyboardLayout layout)
		{
			Dispatcher.InvokeAsync(() => SetCurrent(layout));
		}

		private CustomPopupPlacement[] Popup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
		{
			return new[]
			{
				new CustomPopupPlacement(new Point(targetSize.Width / 2 - popupSize.Width / 2, -popupSize.Height), PopupPrimaryAxis.None)
			};
		}

		private void InitializeLayouts()
		{
			foreach (var layout in keyboard.GetLayouts())
			{
				var button = new KeyboardLayoutButton(layout);

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
			var name = layout.CultureName?.Length > 3 ? String.Join(string.Empty, layout.CultureName.Split(' ').Where(s => Char.IsLetter(s.First())).Select(s => s.First())) : layout.CultureName;
			var tooltip = text.Get(TextKey.SystemControl_KeyboardLayoutTooltip).Replace("%%LAYOUT%%", layout.CultureName);

			foreach (var child in LayoutsStackPanel.Children)
			{
				if (child is KeyboardLayoutButton layoutButton)
				{
					layoutButton.IsCurrent = layout.Id == layoutButton.LayoutId;
				}
			}

			LayoutCultureCode.Text = layout.CultureCode;
			Button.ToolTip = tooltip;

			AutomationProperties.SetName(Button, tooltip);
		}

		private void Popup_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Escape)
			{
				Popup.IsOpen = false;
				Button.Focus();
			}
		}
	}
}
