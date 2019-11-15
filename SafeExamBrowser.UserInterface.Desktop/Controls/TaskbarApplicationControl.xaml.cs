/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarApplicationControl : UserControl, IApplicationControl
	{
		private IApplication application;
		private IApplicationInstance single;

		public TaskbarApplicationControl(IApplication application)
		{
			this.application = application;

			InitializeComponent();
			InitializeApplicationControl();
		}

		private void InitializeApplicationControl()
		{
			var originalBrush = Button.Background;

			application.InstanceStarted += Application_InstanceStarted;

			Button.Click += Button_Click;
			Button.Content = IconResourceLoader.Load(application.Info.Icon);
			Button.MouseEnter += (o, args) => InstancePopup.IsOpen = InstanceStackPanel.Children.Count > 1;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => InstancePopup.IsOpen = InstancePopup.IsMouseOver));
			Button.ToolTip = application.Info.Tooltip;
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

		private void Application_InstanceStarted(IApplicationInstance instance)
		{
			Dispatcher.Invoke(() =>
			{
				var button = new TaskbarApplicationInstanceButton(instance, application.Info);

				instance.Terminated += (_) => RemoveInstance(button);
				InstanceStackPanel.Children.Add(button);

				if (single == default(IApplicationInstance))
				{
					single = instance;
				}
			});
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (InstanceStackPanel.Children.Count == 0)
			{
				application.Start();
			}
			else if (InstanceStackPanel.Children.Count == 1)
			{
				single.Activate();
			}
			else
			{
				InstancePopup.IsOpen = true;
			}
		}

		private void RemoveInstance(TaskbarApplicationInstanceButton button)
		{
			Dispatcher.InvokeAsync(() =>
			{
				InstanceStackPanel.Children.Remove(button);

				if (InstanceStackPanel.Children.Count == 0)
				{
					single = default(IApplicationInstance);
				}
			});
		}
	}
}
