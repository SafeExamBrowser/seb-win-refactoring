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
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class ActionCenterApplicationControl : UserControl, IApplicationControl
	{
		private IApplicationInfo info;

		public event ApplicationControlClickedEventHandler Clicked;

		public ActionCenterApplicationControl(IApplicationInfo info)
		{
			this.info = info;

			InitializeComponent();
			InitializeApplicationControl(info);
		}

		public void RegisterInstance(IApplicationInstance instance)
		{
			Dispatcher.Invoke(() =>
			{
				var button = new ActionCenterApplicationButton(info, instance);

				button.Clicked += (id) => Clicked?.Invoke(id);
				instance.Terminated += (id) => Instance_OnTerminated(id, button);
				InstancePanel.Children.Add(button);

				ApplicationName.Visibility = Visibility.Visible;
				ApplicationButton.Visibility = Visibility.Collapsed;
			});
		}

		private void InitializeApplicationControl(IApplicationInfo info)
		{
			var button = new ActionCenterApplicationButton(info);

			button.Button.Click += (o, args) => Clicked?.Invoke();
			ApplicationName.Text = info.Name;
			ApplicationButton.Content = button;
		}

		private void Instance_OnTerminated(InstanceIdentifier id, ActionCenterApplicationButton button)
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
