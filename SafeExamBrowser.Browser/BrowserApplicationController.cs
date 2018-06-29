/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using BrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationController : IBrowserApplicationController
	{
		private AppConfig appConfig;
		private IApplicationButton button;
		private IList<IApplicationInstance> instances;
		private ILogger logger;
		private IMessageBox messageBox;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		public BrowserApplicationController(
			AppConfig appConfig,
			BrowserSettings settings,
			ILogger logger,
			IMessageBox messageBox,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.instances = new List<IApplicationInstance>();
			this.logger = logger;
			this.messageBox = messageBox;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void Initialize()
		{
			var cefSettings = InitializeCefSettings();
			var success = Cef.Initialize(cefSettings, true, null);

			if (!success)
			{
				throw new Exception("Failed to initialize the browser engine!");
			}
		}

		public void RegisterApplicationButton(IApplicationButton button)
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
			var instance = new BrowserApplicationInstance(appConfig, settings, text, uiFactory, instances.Count == 0);

			instance.Initialize();
			instance.ConfigurationDownloadRequested += (fileName, args) => ConfigurationDownloadRequested?.Invoke(fileName, args);
			instance.Terminated += Instance_Terminated;

			button.RegisterInstance(instance);
			instances.Add(instance);
			instance.Window.Show();
		}

		private CefSettings InitializeCefSettings()
		{
			var cefSettings = new CefSettings
			{
				CachePath = appConfig.BrowserCachePath,
				LogFile = appConfig.BrowserLogFile,
				// TODO: Set according to current application LogLevel, but avoid verbose!
				LogSeverity = LogSeverity.Info
			};

			cefSettings.RegisterScheme(new CefCustomScheme { SchemeName = "seb", SchemeHandlerFactory = new SchemeHandlerFactory() });
			cefSettings.RegisterScheme(new CefCustomScheme { SchemeName = "sebs", SchemeHandlerFactory = new SchemeHandlerFactory() });

			return cefSettings;
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
