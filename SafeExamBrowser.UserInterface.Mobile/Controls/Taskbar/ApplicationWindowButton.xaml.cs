/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskbar
{
	internal partial class ApplicationWindowButton : UserControl
	{
		private IApplicationWindow window;

		internal ApplicationWindowButton(IApplicationWindow window)
		{
			this.window = window;

			InitializeComponent();
			InitializeApplicationInstanceButton();
		}

		private void InitializeApplicationInstanceButton()
		{
			Button.Click += Button_Click;
			Button.ToolTip = window.Title;
			Icon.Content = IconResourceLoader.Load(window.Icon);
			window.IconChanged += Instance_IconChanged;
			window.TitleChanged += Window_TitleChanged;
			Text.Text = window.Title;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			window.Activate();
		}

		private void Instance_IconChanged(IconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(icon));
		}

		private void Window_TitleChanged(string title)
		{
			Dispatcher.InvokeAsync(() =>
			{
				Text.Text = title;
				Button.ToolTip = title;
			});
		}
	}
}
