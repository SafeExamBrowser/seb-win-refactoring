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
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskViewInstanceControl : UserControl
	{
		private IApplicationInstance instance;

		internal InstanceIdentifier Id => instance.Id;

		public TaskViewInstanceControl(IApplicationInstance instance)
		{
			this.instance = instance;

			InitializeComponent();
			InitializeControl();
		}

		internal void Activate()
		{
			instance.Activate();
		}

		internal void Deselect()
		{
			Border.Visibility = Visibility.Hidden;
			Icon.MaxWidth = 40;
			Title.FontWeight = FontWeights.Normal;
		}

		internal void Select()
		{
			Border.Visibility = Visibility.Visible;
			Icon.MaxWidth = 50;
			Title.FontWeight = FontWeights.SemiBold;
		}

		private void InitializeControl()
		{
			Icon.Content = IconResourceLoader.Load(instance.Icon);
			Title.Text = instance.Name;

			instance.IconChanged += Instance_IconChanged;
			instance.NameChanged += Instance_NameChanged;
		}

		private void Instance_NameChanged(string name)
		{
			Dispatcher.InvokeAsync(() => Title.Text = name);
		}

		private void Instance_IconChanged(IconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(instance.Icon));
		}
	}
}
