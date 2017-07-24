/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class BrowserWindow : Window, IBrowserWindow
	{
		public BrowserWindow(IBrowserControl browserControl)
		{
			InitializeComponent();

			if (browserControl is UIElement)
			{
				BrowserContainer.Content = browserControl;
			}
		}

		public void BringToForeground()
		{
			Activate();
		}
	}
}
