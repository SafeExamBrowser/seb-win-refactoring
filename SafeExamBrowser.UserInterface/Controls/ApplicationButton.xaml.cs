/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.Utilities;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class ApplicationButton : UserControl, IApplicationButton
	{
		private IApplicationInfo info;
		private IList<IApplicationInstance> instances = new List<IApplicationInstance>();

		public event TaskbarButtonClickHandler OnClick;

		public ApplicationButton(IApplicationInfo info)
		{
			this.info = info;

			InitializeComponent();
			InitializeApplicationButton();
		}

		public void RegisterInstance(IApplicationInstance instance)
		{
			var instanceButton = new ApplicationInstanceButton(instance, info);

			instances.Add(instance);
			instanceButton.Click += (id) => OnClick?.Invoke(id);
			InstanceStackPanel.Children.Add(instanceButton);

			if (instances.Count > 1)
			{
				InstancePopup.IsOpen = true;
			}
		}

		public void UnregisterInstance(Guid id)
		{
			throw new NotImplementedException();
		}

		private void InitializeApplicationButton()
		{
			Button.ToolTip = info.Tooltip;
			Button.MouseLeave += (o, args) => InstancePopup.IsOpen = InstancePopup.IsMouseOver;
			Button.Content = ApplicationIconResourceLoader.Load(info.IconResource);

			InstancePopup.MouseLeave += (o, args) => InstancePopup.IsOpen = false;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (instances.Count <= 1)
			{
				OnClick?.Invoke();
			}
			else
			{
				InstancePopup.IsOpen = true;
			}
		}
	}
}
