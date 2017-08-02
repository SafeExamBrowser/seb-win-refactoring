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
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationController : IApplicationController
	{
		private ITaskbarButton button;
		private IList<IApplicationInstance> instances = new List<IApplicationInstance>();
		private ISettings settings;
		private IUserInterfaceFactory uiFactory;
		private IText text;

		public BrowserApplicationController(ISettings settings, IText text, IUserInterfaceFactory uiFactory)
		{
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void Initialize()
		{
			var cefSettings = new CefSettings
			{
				CachePath = settings.Browser.CachePath,
				LogFile = settings.Browser.LogFile
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
			this.button.Clicked += Button_OnClick;
		}

		public void Terminate()
		{
			foreach (var instance in instances)
			{
				instance.Terminated -= Instance_Terminated;
				instance.Window.Close();
			}

			Cef.Shutdown();
		}

		private void CreateNewInstance()
		{
			var instance = new BrowserApplicationInstance(settings.Browser, text, uiFactory, instances.Count == 0);

			button.RegisterInstance(instance);
			instances.Add(instance);

			instance.Terminated += Instance_Terminated;
			instance.Window.Show();
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

		private void Instance_Terminated(Guid id)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));
		}
	}
}
