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
using SafeExamBrowser.UserInterface.Desktop.Controls;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public partial class TaskView : Window, ITaskView
	{
		private IList<IApplication> applications;
		private LinkedListNode<TaskViewWindowControl> current;
		private LinkedList<TaskViewWindowControl> controls;

		public TaskView()
		{
			applications = new List<IApplication>();
			controls = new LinkedList<TaskViewWindowControl>();

			InitializeComponent();
		}

		public void Add(IApplication application)
		{
			application.WindowsChanged += Application_WindowsChanged;
			applications.Add(application);
		}

		public void Register(ITaskViewActivator activator)
		{
			activator.Deactivated += Activator_Deactivated;
			activator.NextActivated += Activator_Next;
			activator.PreviousActivated += Activator_Previous;
		}

		private void Application_WindowsChanged()
		{
			Dispatcher.InvokeAsync(Update);
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

		private void ActivateAndHide()
		{
			Activate();
			current?.Value.Activate();
			Hide();
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
			if (controls.Any() && Visibility != Visibility.Visible)
			{
				Show();
				Activate();
			}
		}

		private void Update()
		{
			var windows = new Stack<IApplicationWindow>();

			foreach (var application in applications)
			{
				foreach (var window in application.GetWindows())
				{
					windows.Push(window);
				}
			}

			var max = Math.Ceiling(Math.Sqrt(windows.Count));

			controls.Clear();
			Rows.Children.Clear();

			for (var rowCount = 0; rowCount < max && windows.Any(); rowCount++)
			{
				var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

				Rows.Children.Add(row);

				for (var columnIndex = 0; columnIndex < max && windows.Any(); columnIndex++)
				{
					var window = windows.Pop();
					var control = new TaskViewWindowControl(window);

					controls.AddLast(control);
					row.Children.Add(control);
				}
			}

			current = controls.First;
			current?.Value.Select();

			UpdateLayout();

			Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
			Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;

			if (!controls.Any())
			{
				Hide();
			}
		}
	}
}
