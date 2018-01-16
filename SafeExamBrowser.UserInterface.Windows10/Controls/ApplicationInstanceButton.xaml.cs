/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.UserInterface.Windows10.Utilities;

namespace SafeExamBrowser.UserInterface.Windows10.Controls
{
	internal delegate void InstanceButtonClickedEventHandler(Guid instanceId);

	public partial class ApplicationInstanceButton : UserControl
	{
		private IApplicationInfo info;
		private IApplicationInstance instance;

		internal event InstanceButtonClickedEventHandler Clicked;

		public ApplicationInstanceButton(IApplicationInstance instance, IApplicationInfo info)
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

			instance.NameChanged += (name) =>
			{
				Dispatcher.Invoke(() =>
				{
					Text.Text = name;
					Button.ToolTip = name;
				});
			};
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Clicked?.Invoke(instance.Id);
		}
	}
}
