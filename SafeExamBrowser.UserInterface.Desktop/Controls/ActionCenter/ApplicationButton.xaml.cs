/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Automation;
using System.Windows.Controls;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.ActionCenter
{
	internal partial class ApplicationButton : UserControl
	{
		private readonly IApplication application;
		private readonly IApplicationWindow window;

		internal event EventHandler Clicked;

		internal ApplicationButton(IApplication application, IApplicationWindow window = null)
		{
			this.application = application;
			this.window = window;

			InitializeComponent();
			InitializeApplicationInstanceButton();
		}

		private void InitializeApplicationInstanceButton()
		{
			var tooltip = window?.Title ?? application.Tooltip;

			Button.Click += (o, args) => Clicked?.Invoke(this, EventArgs.Empty);
			Button.ToolTip = tooltip;
			Icon.Content = IconResourceLoader.Load(window?.Icon ?? application.Icon);
			Text.Text = window?.Title ?? application.Name;

			if (tooltip != default)
			{
				AutomationProperties.SetName(Button, string.Empty);
			}

			if (window != default)
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
