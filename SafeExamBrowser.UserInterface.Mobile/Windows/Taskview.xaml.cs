/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using System.Windows.Interop;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Mobile.Controls.Taskview;

namespace SafeExamBrowser.UserInterface.Mobile.Windows
{
	internal partial class Taskview : Window, ITaskview
	{
		private IList<IApplication> applications;
		private LinkedListNode<WindowControl> current;
		private LinkedList<WindowControl> controls;

		internal IntPtr Handle { get; private set; }

		internal Taskview()
		{
			applications = new List<IApplication>();
			controls = new LinkedList<WindowControl>();

			InitializeComponent();
			InitializeTaskview();
		}

		public void Add(IApplication application)
		{
			application.WindowsChanged += Application_WindowsChanged;
			applications.Add(application);
		}

		public void Register(ITaskviewActivator activator)
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
			if (IsVisible)
			{
				Activate();
				current?.Value.Activate();
				Hide();
			}
		}

		private void InitializeTaskview()
		{
			Loaded += (o, args) =>
			{
				Handle = new WindowInteropHelper(this).Handle;
				Update();
			};
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
			ClearTaskview();
			LoadControls();
			UpdateLocation();
		}

		private void ClearTaskview()
		{
			foreach (var control in controls)
			{
				control.Destroy();
			}

			controls.Clear();
			Rows.Children.Clear();
		}

		private void LoadControls()
		{
			var windows = GetAllWindows();
			var maxColumns = Math.Ceiling(Math.Sqrt(windows.Count));

			while (windows.Any())
			{
				var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

				Rows.Children.Add(row);

				for (var column = 0; column < maxColumns && windows.Any(); column++)
				{
					var window = windows.Pop();
					var control = new WindowControl(window, this);

					controls.AddLast(control);
					row.Children.Add(control);
				}
			}

			current = controls.First;
			current?.Value.Select();
		}

		private void UpdateLocation()
		{
			if (controls.Any())
			{
				UpdateLayout();

				Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
				Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;
			}
			else
			{
				Hide();
			}
		}

		private Stack<IApplicationWindow> GetAllWindows()
		{
			var stack = new Stack<IApplicationWindow>();

			foreach (var application in applications)
			{
				foreach (var window in application.GetWindows())
				{
					stack.Push(window);
				}
			}

			return stack;
		}
	}
}
