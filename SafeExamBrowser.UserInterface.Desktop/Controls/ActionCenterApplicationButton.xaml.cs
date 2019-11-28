/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Controls;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class ActionCenterApplicationButton : UserControl
	{
		private ApplicationInfo info;
		private IApplicationWindow window;

		internal event EventHandler Clicked;

		public ActionCenterApplicationButton(ApplicationInfo info, IApplicationWindow window = null)
		{
			this.info = info;
			this.window = window;

			InitializeComponent();
			InitializeApplicationInstanceButton();
		}

		private void InitializeApplicationInstanceButton()
		{
			Icon.Content = IconResourceLoader.Load(info.Icon);
			Text.Text = window?.Title ?? info.Name;
			Button.Click += (o, args) => Clicked?.Invoke(this, EventArgs.Empty);
			Button.ToolTip = window?.Title ?? info.Tooltip;

			if (window != null)
			{
				window.IconChanged += Window_IconChanged;
				window.TitleChanged += Window_TitleChanged;
			}
		}

		private void Window_IconChanged(IconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(icon));
		}

		private void Window_TitleChanged(string title)
		{
			Dispatcher.InvokeAsync(() =>
			{
				Text.Text = title;
				Button.ToolTip = title;
			});
		}
	}
}
