/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.UserInterface.Utilities;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class ApplicationInstanceButton : UserControl
	{
		private IApplicationInfo info;
		private IApplicationInstance instance;

		public delegate void OnClickHandler(Guid instanceId);
		public event OnClickHandler Click;

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
			Button.ToolTip = $"{instance.Name} - {info.Tooltip}";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Click?.Invoke(instance.Id);
		}
	}
}
