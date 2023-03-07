/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.ActionCenter
{
	internal partial class ApplicationControl : UserControl, IApplicationControl
	{
		private IApplication application;

		internal ApplicationControl(IApplication application)
		{
			this.application = application;

			InitializeComponent();
			InitializeApplicationControl();
		}

		private void InitializeApplicationControl()
		{
			var button = new ApplicationButton(application);

			application.WindowsChanged += Application_WindowsChanged;
			button.Clicked += (o, args) => application.Start();
			ApplicationName.Text = application.Name;
			ApplicationName.Visibility = Visibility.Collapsed;
			ApplicationButton.Content = button;
		}

		private void Application_WindowsChanged()
		{
			Dispatcher.InvokeAsync(Update);
		}

		private void Update()
		{
			var windows = application.GetWindows();

			WindowPanel.Children.Clear();

			foreach (var window in windows)
			{
				var button = new ApplicationButton(application, window);

				button.Clicked += (o, args) => window.Activate();
				WindowPanel.Children.Add(button);
			}

			if (WindowPanel.Children.Count == 0)
			{
				ApplicationName.Visibility = Visibility.Collapsed;
				ApplicationButton.Visibility = Visibility.Visible;
			}
			else
			{
				ApplicationName.Visibility = Visibility.Visible;
				ApplicationButton.Visibility = Visibility.Collapsed;
			}
		}
	}
}
