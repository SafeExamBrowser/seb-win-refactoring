/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class QuitButton : UserControl
	{
		public QuitButton()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.MainWindow.Close();
		}
	}
}
