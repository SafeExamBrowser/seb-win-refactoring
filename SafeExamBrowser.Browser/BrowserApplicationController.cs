/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationController : IApplicationController
	{
		private ITaskbarButton button;
		private IList<IApplicationInstance> instances = new List<IApplicationInstance>();
		private ISettings settings;
		private IUserInterfaceFactory uiFactory;

		public BrowserApplicationController(ISettings settings, IUserInterfaceFactory uiFactory)
		{
			this.settings = settings;
			this.uiFactory = uiFactory;
		}

		public void Initialize()
		{
			var cefSettings = new CefSettings
			{
				CachePath = settings.BrowserCachePath,
				LogFile = settings.BrowserLogFile
			};

			var success = Cef.Initialize(cefSettings, true, null);

			if (!success)
			{
				throw new Exception("Failed to initialize the browser engine!");
			}
		}

		public void RegisterApplicationButton(ITaskbarButton button)
		{
			this.button = button;
			this.button.OnClick += Button_OnClick;
		}

		public void Terminate()
		{
			foreach (var instance in instances)
			{
				instance.OnTerminated -= Instance_OnTerminated;
				instance.Window.Close();
			}

			Cef.Shutdown();
		}

		private void CreateNewInstance()
		{
			var control = new BrowserControl("www.duckduckgo.com");
			var window = uiFactory.CreateBrowserWindow(control);
			var instance = new BrowserApplicationInstance("DuckDuckGo");

			instance.RegisterWindow(window);
			instance.OnTerminated += Instance_OnTerminated;

			button.RegisterInstance(instance);
			instances.Add(instance);

			window.Show();
		}

		private void Button_OnClick(Guid? instanceId = null)
		{
			if (instanceId.HasValue)
			{
				instances.FirstOrDefault(i => i.Id == instanceId)?.Window?.BringToForeground();
			}
			else
			{
				CreateNewInstance();
			}
		}

		private void Instance_OnTerminated(Guid id)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));
		}
	}
}
