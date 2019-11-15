/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using System.Windows.Documents;
using System.Windows.Media;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public partial class TaskView : Window, ITaskView
	{
		private IList<IApplicationInstance> instances;

		public TaskView()
		{
			InitializeComponent();
			instances = new List<IApplicationInstance>();
		}

		public void Add(IApplication application)
		{
			application.InstanceStarted += Application_InstanceStarted;
		}

		public void Register(ITaskViewActivator activator)
		{
			activator.Deactivated += Activator_Deactivated;
			activator.NextActivated += Activator_Next;
			activator.PreviousActivated += Activator_Previous;
		}

		private void Application_InstanceStarted(IApplicationInstance instance)
		{
			Dispatcher.InvokeAsync(() =>
			{
				instance.IconChanged += Instance_IconChanged;
				instance.NameChanged += Instance_NameChanged;
				instance.Terminated += Instance_Terminated;

				instances.Add(instance);
				Update();
			});
		}

		private void Activator_Deactivated()
		{
			Dispatcher.InvokeAsync(Hide);
		}

		private void Activator_Next()
		{
			Dispatcher.InvokeAsync(ShowConditional);
		}

		private void Activator_Previous()
		{
			Dispatcher.InvokeAsync(ShowConditional);
		}

		private void Instance_IconChanged(IconResource icon)
		{
			// TODO Dispatcher.InvokeAsync(...);
		}

		private void Instance_NameChanged(string name)
		{
			// TODO Dispatcher.InvokeAsync(...);
		}

		private void Instance_Terminated(InstanceIdentifier id)
		{
			Dispatcher.InvokeAsync(() =>
			{
				var instance = instances.FirstOrDefault(i => i.Id == id);

				if (instance != default(IApplicationInstance))
				{
					instances.Remove(instance);
					Update();
				}
			});
		}

		private void ShowConditional()
		{
			if (Visibility != Visibility.Visible && instances.Any())
			{
				Show();
			}
		}

		private void Update()
		{
			var max = Math.Ceiling(Math.Sqrt(instances.Count));
			var stack = new Stack<IApplicationInstance>(instances);

			Rows.Children.Clear();

			for (var rowCount = 0; rowCount < max && stack.Any(); rowCount++)
			{
				var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

				Rows.Children.Add(row);

				for (var columnIndex = 0; columnIndex < max && stack.Any(); columnIndex++)
				{
					var instance = stack.Pop();
					var control = BuildInstanceControl(instance);

					row.Children.Add(control);
				}
			}

			UpdateLayout();

			Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
			Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;

			if (!instances.Any())
			{
				Hide();
			}
		}

		private UIElement BuildInstanceControl(IApplicationInstance instance)
		{
			var border = new Border();
			var stackPanel = new StackPanel();
			var icon = IconResourceLoader.Load(instance.Icon);

			border.BorderBrush = Brushes.White;
			border.BorderThickness = new Thickness(1);
			border.Height = 150;
			border.Margin = new Thickness(5);
			border.Padding = new Thickness(2);
			border.Width = 250;
			border.Child = stackPanel;

			stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
			stackPanel.Orientation = Orientation.Vertical;
			stackPanel.VerticalAlignment = VerticalAlignment.Center;
			stackPanel.Children.Add(new ContentControl { Content = icon, MaxWidth = 50 });
			stackPanel.Children.Add(new TextBlock(new Run($"Instance {instance.Name ?? "NULL"}") { Foreground = Brushes.White, FontWeight = FontWeights.Bold }));

			return border;
		}
	}
}
