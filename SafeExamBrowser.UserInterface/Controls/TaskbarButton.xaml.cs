/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class TaskbarButton : UserControl, ITaskbarButton
	{
		public event TaskbarButtonClickHandler OnClick;

		public TaskbarButton(string imageUri)
		{
			InitializeComponent();

			var icon = new BitmapImage();

			icon.BeginInit();
			icon.UriSource = new Uri(imageUri);
			icon.EndInit();

			IconImage.Source = icon;
		}
	}
}
