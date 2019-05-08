/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Applications;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarApplicationInstanceButton : UserControl
	{
		private IApplicationInfo info;
		private IApplicationInstance instance;

		internal event ApplicationControlClickedEventHandler Clicked;

		public TaskbarApplicationInstanceButton(IApplicationInstance instance, IApplicationInfo info)
		{
			this.info = info;
			this.instance = instance;

			InitializeComponent();
			InitializeApplicationInstanceButton();
		}

		private void InitializeApplicationInstanceButton()
		{
			Icon.Content = IconResourceLoader.Load(info.IconResource);
			Text.Text = instance.Name;
			Button.ToolTip = instance.Name;

			instance.IconChanged += Instance_IconChanged;
			instance.NameChanged += Instance_NameChanged;
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Clicked?.Invoke(instance.Id);
		}
	}
}
