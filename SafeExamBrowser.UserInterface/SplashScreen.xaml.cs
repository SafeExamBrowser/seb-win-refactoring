/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class SplashScreen : Window, ISplashScreen
	{
		public SplashScreen()
		{
			InitializeComponent();
		}

		public void Notify(ILogContent content)
		{
			// TODO
		}
	}
}
