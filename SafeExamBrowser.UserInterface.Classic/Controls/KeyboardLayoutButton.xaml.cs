/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;

namespace SafeExamBrowser.UserInterface.Classic.Controls
{
	public partial class KeyboardLayoutButton : UserControl
	{
		public event RoutedEventHandler Click;

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

		public KeyboardLayoutButton()
		{
			InitializeComponent();

			Button.Click += (o, args) => Click?.Invoke(o, args);
		}
	}
}
