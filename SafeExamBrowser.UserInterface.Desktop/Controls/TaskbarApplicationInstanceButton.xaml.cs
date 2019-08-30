/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarApplicationInstanceButton : UserControl
	{
		private IApplicationInfo info;
		private IApplicationInstance instance;

		public TaskbarApplicationInstanceButton(IApplicationInstance instance, IApplicationInfo info)
		{
			this.info = info;
			this.instance = instance;

			InitializeComponent();
			InitializeApplicationInstanceButton();
		}

		private void InitializeApplicationInstanceButton()
		{
			Button.Click += Button_Click;
			Button.ToolTip = instance.Name;
			Icon.Content = IconResourceLoader.Load(info.IconResource);
			instance.IconChanged += Instance_IconChanged;
			instance.NameChanged += Instance_NameChanged;
			Text.Text = instance.Name;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			instance.Activate();
		}

		private void Instance_IconChanged(IIconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(icon));
		}

		private void Instance_NameChanged(string name)
		{
			Dispatcher.Invoke(() =>
			{
				Text.Text = name;
				Button.ToolTip = name;
			});
		}
	}
}
