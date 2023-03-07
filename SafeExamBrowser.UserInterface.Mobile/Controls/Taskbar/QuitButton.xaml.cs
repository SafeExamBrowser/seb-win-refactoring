/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskbar
{
	internal partial class QuitButton : UserControl
	{
		internal event QuitButtonClickedEventHandler Clicked;

		public QuitButton()
		{
			InitializeComponent();
			LoadIcon();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Clicked?.Invoke(new CancelEventArgs());
		}

		private void LoadIcon()
		{
			var uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ShutDown.xaml");
			var resource = new XamlIconResource { Uri = uri };

			Button.Content = IconResourceLoader.Load(resource);
		}
	}
}
