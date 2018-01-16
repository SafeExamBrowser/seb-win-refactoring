/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using System.Windows.Media;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.UserInterface.Windows10.Utilities;

namespace SafeExamBrowser.UserInterface.Windows10.Controls
{
	public partial class ApplicationButton : UserControl, IApplicationButton
	{
		private IApplicationInfo info;
		private IList<IApplicationInstance> instances = new List<IApplicationInstance>();

		public event ApplicationButtonClickedEventHandler Clicked;

		public ApplicationButton(IApplicationInfo info)
		{
			this.info = info;

			InitializeComponent();
			InitializeApplicationButton();
		}

		public void RegisterInstance(IApplicationInstance instance)
		{
			var instanceButton = new ApplicationInstanceButton(instance, info);

			instanceButton.Clicked += (id) => Clicked?.Invoke(id);
			instance.Terminated += (id) => Instance_OnTerminated(id, instanceButton);

			instances.Add(instance);
			InstanceStackPanel.Children.Add(instanceButton);

			ActiveBar.Visibility = Visibility.Visible;
		}

		private void InitializeApplicationButton()
		{
			Button.ToolTip = info.Tooltip;
			Button.Content = IconResourceLoader.Load(info.IconResource);
			
			Button.MouseEnter += (o, args) => InstancePopup.IsOpen = instances.Count > 1;
			Button.MouseLeave += (o, args) => InstancePopup.IsOpen &= InstancePopup.IsMouseOver || ActiveBar.IsMouseOver;
			ActiveBar.MouseLeave += (o, args) => InstancePopup.IsOpen &= InstancePopup.IsMouseOver || Button.IsMouseOver;
			InstancePopup.MouseLeave += (o, args) => InstancePopup.IsOpen = false;

			InstancePopup.Opened += (o, args) =>
			{
				ActiveBar.Width = Double.NaN;
				Background = (Brush) new BrushConverter().ConvertFrom("#2AFFFFFF");
			};

			InstancePopup.Closed += (o, args) =>
			{
				ActiveBar.Width = 40;
				Background = (Brush) new BrushConverter().ConvertFrom("#00000000");
			};

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
				Clicked?.Invoke(instances.FirstOrDefault()?.Id);
			}
			else
			{
				InstancePopup.IsOpen = true;
			}
		}

		private void Instance_OnTerminated(Guid id, ApplicationInstanceButton instanceButton)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));
			InstanceStackPanel.Children.Remove(instanceButton);

			if (!instances.Any())
			{
				ActiveBar.Visibility = Visibility.Collapsed;
			}
		}
	}
}
