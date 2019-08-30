/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarApplicationControl : UserControl, IApplicationControl
	{
		private IApplicationInfo info;
		private IList<IApplicationInstance> instances = new List<IApplicationInstance>();

		public event ApplicationControlClickedEventHandler Clicked;

		public TaskbarApplicationControl(IApplicationInfo info)
		{
			this.info = info;

			InitializeComponent();
			InitializeApplicationControl();
		}

		public void RegisterInstance(IApplicationInstance instance)
		{
			Dispatcher.Invoke(() =>
			{
				var instanceButton = new TaskbarApplicationInstanceButton(instance, info);

				instanceButton.Clicked += (id) => Clicked?.Invoke(id);
				instance.Terminated += (id) => Instance_OnTerminated(id, instanceButton);

				instances.Add(instance);
				InstanceStackPanel.Children.Add(instanceButton);
			});
		}

		private void InitializeApplicationControl()
		{
			var originalBrush = Button.Background;

			Button.ToolTip = info.Tooltip;
			Button.Content = IconResourceLoader.Load(info.IconResource);

			Button.MouseEnter += (o, args) => InstancePopup.IsOpen = instances.Count > 1;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => InstancePopup.IsOpen = InstancePopup.IsMouseOver));
			InstancePopup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => InstancePopup.IsOpen = IsMouseOver));

			InstancePopup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				Button.Background = Brushes.LightGray;
			};

			InstancePopup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
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

		private void Instance_OnTerminated(InstanceIdentifier id, TaskbarApplicationInstanceButton instanceButton)
		{
			Dispatcher.InvokeAsync(() =>
			{
				instances.Remove(instances.FirstOrDefault(i => i.Id == id));
				InstanceStackPanel.Children.Remove(instanceButton);
			});
		}
	}
}
