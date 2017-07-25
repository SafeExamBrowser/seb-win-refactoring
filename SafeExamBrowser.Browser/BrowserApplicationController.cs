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
		private IUiElementFactory uiFactory;

		public BrowserApplicationController(ISettings settings, IUiElementFactory uiFactory)
		{
			this.settings = settings;
			this.uiFactory = uiFactory;
		}

		public void Initialize()
		{
			var cefSettings = new CefSettings
			{
				CachePath = settings.BrowserCachePath
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
			this.button.OnClick += ButtonClick;
		}

		public void Terminate()
		{
			Cef.Shutdown();
		}

		private void ButtonClick(Guid? instanceId = null)
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

		private void CreateNewInstance()
		{
			var control = new BrowserControl();
			var window = uiFactory.CreateBrowserWindow(control);
			var instance = new BrowserApplicationInstance("DuckDuckGo");

			instances.Add(instance);
			instance.RegisterWindow(window);
			button.RegisterInstance(instance);

			control.Address = "www.duckduckgo.com";

			window.Display();
		}
	}
}
