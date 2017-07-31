/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationInstance : IApplicationInstance
	{
		private IBrowserControl control;
		private IBrowserWindow window;

		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public IWindow Window { get { return window; } }

		public event TerminationEventHandler Terminated;
		public event NameChangedEventHandler NameChanged;

		public BrowserApplicationInstance(IBrowserSettings settings, IText text, IUserInterfaceFactory uiFactory, bool isMainInstance)
		{
			Id = Guid.NewGuid();

			control = new BrowserControl(settings, text);
			control.AddressChanged += Control_AddressChanged;
			control.TitleChanged += Control_TitleChanged;

			window = uiFactory.CreateBrowserWindow(control, settings);
			window.IsMainWindow = isMainInstance;
			window.Closing += () => Terminated?.Invoke(Id);
			window.AddressChanged += Window_AddressChanged;
			window.ReloadRequested += Window_ReloadRequested;
			window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			window.ForwardNavigationRequested += Window_ForwardNavigationRequested;
		}

		private void Control_AddressChanged(string address)
		{
			window.UpdateAddress(address);
		}

		private void Control_TitleChanged(string title)
		{
			window.UpdateTitle(title);
			NameChanged?.Invoke(title);
		}

		private void Window_AddressChanged(string address)
		{
			control.NavigateTo(address);
		}

		private void Window_ReloadRequested()
		{
			control.Reload();
		}

		private void Window_BackwardNavigationRequested()
		{
			control.NavigateBackwards();
		}

		private void Window_ForwardNavigationRequested()
		{
			control.NavigateForwards();
		}
	}
}
