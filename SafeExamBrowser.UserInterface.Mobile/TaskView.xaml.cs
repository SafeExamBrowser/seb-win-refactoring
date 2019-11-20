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
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Mobile.Controls;

namespace SafeExamBrowser.UserInterface.Mobile
{
	public partial class TaskView : Window, ITaskView
	{
		private LinkedListNode<TaskViewInstanceControl> current;
		private LinkedList<TaskViewInstanceControl> controls;
		private List<IApplicationInstance> instances;

		public TaskView()
		{
			controls = new LinkedList<TaskViewInstanceControl>();
			instances = new List<IApplicationInstance>();

			InitializeComponent();
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
			Dispatcher.InvokeAsync(() => Add(instance));
		}

		private void Activator_Deactivated()
		{
			Dispatcher.InvokeAsync(ActivateAndHide);
		}

		private void Activator_Next()
		{
			Dispatcher.InvokeAsync(SelectNext);
		}

		private void Activator_Previous()
		{
			Dispatcher.InvokeAsync(SelectPrevious);
		}

		private void Instance_Terminated(InstanceIdentifier id)
		{
			Dispatcher.InvokeAsync(() => Remove(id));
		}

		private void ActivateAndHide()
		{
			Activate();
			current?.Value.Activate();
			Hide();
		}

		private void Add(IApplicationInstance instance)
		{
			instance.Terminated += Instance_Terminated;
			instances.Add(instance);
			Update();
		}

		private void Remove(InstanceIdentifier id)
		{
			var instance = instances.FirstOrDefault(i => i.Id == id);

			if (instance != default(IApplicationInstance))
			{
				instances.Remove(instance);
				Update();
			}
		}

		private void SelectNext()
		{
			ShowConditional();

			if (current != null)
			{
				current.Value.Deselect();
				current = current.Next ?? controls.First;
				current.Value.Select();
			}
		}

		private void SelectPrevious()
		{
			ShowConditional();

			if (current != null)
			{
				current.Value.Deselect();
				current = current.Previous ?? controls.Last;
				current.Value.Select();
			}
		}

		private void ShowConditional()
		{
			if (instances.Any() && Visibility != Visibility.Visible)
			{
				Show();
				Activate();
			}
		}

		private void Update()
		{
			var max = Math.Ceiling(Math.Sqrt(instances.Count));
			var stack = new Stack<IApplicationInstance>(instances);

			controls.Clear();
			Rows.Children.Clear();

			for (var rowCount = 0; rowCount < max && stack.Any(); rowCount++)
			{
				var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

				Rows.Children.Add(row);

				for (var columnIndex = 0; columnIndex < max && stack.Any(); columnIndex++)
				{
					var instance = stack.Pop();
					var control = new TaskViewInstanceControl(instance);

					controls.AddLast(control);
					row.Children.Add(control);
				}
			}

			current = controls.First;
			current?.Value.Select();

			UpdateLayout();

			Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
			Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;

			if (!instances.Any())
			{
				Hide();
			}
		}
	}
}
