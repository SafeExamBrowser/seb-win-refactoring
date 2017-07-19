/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.Utilities;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class ApplicationButton : UserControl, ITaskbarButton
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

			ActiveBar.Visibility = Visibility.Visible;
		}

		public void UnregisterInstance(Guid id)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));

			if (!instances.Any())
			{
				ActiveBar.Visibility = Visibility.Collapsed;
			}
		}

		private void InitializeApplicationButton()
		{
			Button.ToolTip = info.Tooltip;
			Button.Content = IconResourceLoader.Load(info.IconResource);

			Button.MouseEnter += (o, args) => InstancePopup.IsOpen = instances.Count > 1;
			Button.MouseLeave += (o, args) => InstancePopup.IsOpen &= InstancePopup.IsMouseOver || ActiveBar.IsMouseOver;
			ActiveBar.MouseLeave += (o, args) => InstancePopup.IsOpen &= InstancePopup.IsMouseOver || Button.IsMouseOver;
			InstancePopup.MouseLeave += (o, args) => InstancePopup.IsOpen = false;
			InstancePopup.Opened += (o, args) => ActiveBar.Width = Double.NaN;
			InstancePopup.Closed += (o, args) => ActiveBar.Width = 40;
			InstanceStackPanel.SizeChanged += (o, args) =>
			{
				if (instances.Count > 9)
				{
					InstanceScrollViewer.MaxHeight = InstanceScrollViewer.ActualHeight;
				}
			};
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (instances.Count <= 1)
			{
				OnClick?.Invoke(instances.FirstOrDefault()?.Id);
			}
			else
			{
				InstancePopup.IsOpen = true;
			}
		}
	}
}
