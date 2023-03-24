/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskbar
{
	internal partial class KeyboardLayoutButton : UserControl
	{
		private readonly IKeyboardLayout layout;

		internal bool IsCurrent
		{
			set { IsCurrentTextBlock.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		internal Guid LayoutId
		{
			get { return layout.Id; }
		}

		internal event EventHandler LayoutSelected;

		internal KeyboardLayoutButton(IKeyboardLayout layout)
		{
			this.layout = layout;

			InitializeComponent();
			InitializeLayoutButton();
		}

		private void InitializeLayoutButton()
		{
			Button.Click += (o, args) => LayoutSelected?.Invoke(this, EventArgs.Empty);
			CultureCodeTextBlock.Text = layout.CultureCode;
			CultureNameTextBlock.Text = layout.CultureName;
			LayoutNameTextBlock.Text = layout.LayoutName;

			AutomationProperties.SetName(Button, layout.CultureName);
		}
	}
}
