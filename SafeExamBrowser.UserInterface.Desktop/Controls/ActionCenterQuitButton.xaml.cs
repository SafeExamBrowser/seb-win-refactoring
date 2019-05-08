/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class ActionCenterQuitButton : UserControl
	{
		public event QuitButtonClickedEventHandler Clicked;

		public ActionCenterQuitButton()
		{
			InitializeComponent();
			InitializeControl();
		}

		private void InitializeControl()
		{
			var uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ShutDown.xaml");
			var resource = new XamlIconResource(uri);

			Icon.Content = IconResourceLoader.Load(resource);
			Button.Click += (o, args) => Clicked?.Invoke(new CancelEventArgs());
		}
	}
}
