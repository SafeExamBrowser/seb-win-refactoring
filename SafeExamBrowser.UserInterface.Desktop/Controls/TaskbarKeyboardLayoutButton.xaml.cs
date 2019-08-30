/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarKeyboardLayoutButton : UserControl
	{
		private IKeyboardLayout layout;

		public event KeyboardLayoutSelectedEventHandler LayoutSelected;

		public string CultureCode
		{
			set { CultureCodeTextBlock.Text = value; }
		}

		public bool IsCurrent
		{
			set { IsCurrentTextBlock.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		public string LayoutName
		{
			set { LayoutNameTextBlock.Text = value; }
		}

		public Guid LayoutId
		{
			get { return layout.Id; }
		}

		public TaskbarKeyboardLayoutButton(IKeyboardLayout layout)
		{
			this.layout = layout;

			InitializeComponent();
			InitializeEvents();
		}

		private void InitializeEvents()
		{
			Button.Click += (o, args) => LayoutSelected?.Invoke(layout.Id);
		}
	}
}
