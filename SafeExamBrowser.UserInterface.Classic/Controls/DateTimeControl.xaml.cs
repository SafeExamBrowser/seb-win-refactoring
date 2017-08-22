/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Controls;
using SafeExamBrowser.UserInterface.Classic.ViewModels;

namespace SafeExamBrowser.UserInterface.Classic.Controls
{
	public partial class DateTimeControl : UserControl
	{
		private DateTimeViewModel model = new DateTimeViewModel();

		public DateTimeControl()
		{
			InitializeComponent();

			DataContext = model;
			TimeTextBlock.DataContext = model;
			DateTextBlock.DataContext = model;
		}
	}
}
