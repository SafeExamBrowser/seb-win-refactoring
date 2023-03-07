/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Controls;
using SafeExamBrowser.UserInterface.Desktop.ViewModels;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.Taskbar
{
	internal partial class Clock : UserControl
	{
		private DateTimeViewModel model;

		public Clock()
		{
			InitializeComponent();
			InitializeControl();
		}

		private void InitializeControl()
		{
			model = new DateTimeViewModel(false);
			DataContext = model;
			TimeTextBlock.DataContext = model;
			DateTextBlock.DataContext = model;
		}
	}
}
