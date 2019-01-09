/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;

namespace SafeExamBrowser.UserInterface.Windows10.Controls
{
	public partial class QuitButton : UserControl
	{
		public event QuitButtonClickedEventHandler Clicked;

		public QuitButton()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Clicked?.Invoke(new CancelEventArgs());
		}
	}
}
