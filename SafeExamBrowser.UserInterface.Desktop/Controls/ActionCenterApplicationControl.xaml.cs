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
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class ActionCenterApplicationControl : UserControl, IApplicationControl
	{
		private IApplication application;

		public ActionCenterApplicationControl(IApplication application)
		{
			this.application = application;

			InitializeComponent();
			InitializeApplicationControl();
		}

		private void InitializeApplicationControl()
		{
			var button = new ActionCenterApplicationButton(application.Info);

			application.InstanceStarted += Application_InstanceStarted;
			button.Clicked += (o, args) => application.Start();
			ApplicationName.Text = application.Info.Name;
			ApplicationName.Visibility = Visibility.Collapsed;
			ApplicationButton.Content = button;
		}

		private void Application_InstanceStarted(IApplicationInstance instance)
		{
			Dispatcher.InvokeAsync(() =>
			{
				var button = new ActionCenterApplicationButton(application.Info, instance);

				button.Clicked += (o, args) => instance.Activate();
				instance.Terminated += (_) => RemoveInstance(button);
				InstancePanel.Children.Add(button);

				ApplicationName.Visibility = Visibility.Visible;
				ApplicationButton.Visibility = Visibility.Collapsed;
			});
		}

		private void RemoveInstance(ActionCenterApplicationButton button)
		{
			Dispatcher.InvokeAsync(() =>
			{
				InstancePanel.Children.Remove(button);

				if (InstancePanel.Children.Count == 0)
				{
					ApplicationName.Visibility = Visibility.Collapsed;
					ApplicationButton.Visibility = Visibility.Visible;
				}
			});
		}
	}
}
