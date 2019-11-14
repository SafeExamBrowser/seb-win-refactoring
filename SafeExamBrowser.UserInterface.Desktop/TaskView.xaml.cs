/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public partial class TaskView : Window, ITaskView
	{
		public TaskView()
		{
			InitializeComponent();
		}

		public void Add(IApplication application)
		{
			
		}

		public void Register(ITaskViewActivator activator)
		{
			
		}
	}
}
