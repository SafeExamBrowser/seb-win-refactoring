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

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class ActionCenterApplicationButton : UserControl
	{
		private ApplicationInfo info;
		private IApplicationInstance instance;

		internal event EventHandler Clicked;

		public ActionCenterApplicationButton(ApplicationInfo info, IApplicationInstance instance = null)
		{
			this.info = info;
			this.instance = instance;

			InitializeComponent();
			InitializeApplicationInstanceButton();
		}

		private void InitializeApplicationInstanceButton()
		{
			Icon.Content = IconResourceLoader.Load(info.IconResource);
			Text.Text = instance?.Name ?? info.Name;
			Button.Click += (o, args) => Clicked?.Invoke(this, EventArgs.Empty);
			Button.ToolTip = instance?.Name ?? info.Tooltip;

			if (instance != null)
			{
				instance.IconChanged += Instance_IconChanged;
				instance.NameChanged += Instance_NameChanged;
			}
		}

		private void Instance_IconChanged(IconResource icon)
		{
			Dispatcher.InvokeAsync(() => Icon.Content = IconResourceLoader.Load(icon));
		}

		private void Instance_NameChanged(string name)
		{
			Dispatcher.InvokeAsync(() =>
			{
				Text.Text = name;
				Button.ToolTip = name;
			});
		}
	}
}
