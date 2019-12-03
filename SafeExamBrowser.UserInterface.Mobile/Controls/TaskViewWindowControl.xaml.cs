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
using SafeExamBrowser.Applications.Contracts.Resources.Icons;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class TaskViewWindowControl : UserControl
	{
		private IApplicationWindow window;

		public TaskViewWindowControl(IApplicationWindow window)
		{
			this.window = window;

			InitializeComponent();
			InitializeControl();
		}

		internal void Activate()
		{
			window.Activate();
		}

		internal void Deselect()
		{
			Icon.MaxWidth = 40;
			Indicator.Visibility = Visibility.Hidden;
			Title.FontWeight = FontWeights.Normal;
		}

		internal void Select()
		{
			Icon.MaxWidth = 50;
			Indicator.Visibility = Visibility.Visible;
			Title.FontWeight = FontWeights.SemiBold;
		}

		private void InitializeControl()
		{
			Icon.Content = IconResourceLoader.Load(window.Icon);
			Title.Text = window.Title;

			window.IconChanged += Instance_IconChanged;
			window.TitleChanged += Instance_TitleChanged;
		}

		private void Instance_TitleChanged(string title)
		{
			Dispatcher.InvokeAsync(() => Title.Text = title);
		}

		private void Instance_IconChanged(IconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(window.Icon));
		}
	}
}
