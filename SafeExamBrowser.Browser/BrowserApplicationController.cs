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
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core;
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
		private int instanceIdCounter = default(int);

		private AppConfig appConfig;
		private IApplicationButton button;
		private IList<IApplicationInstance> instances;
		private IModuleLogger logger;
		private IMessageBox messageBox;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		public BrowserApplicationController(
			AppConfig appConfig,
			BrowserSettings settings,
			IModuleLogger logger,
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

			logger.Info("Initialized CEF.");

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

				logger.Info($"Terminated browser instance {instance.Id}.");
			}

			Cef.Shutdown();

			logger.Info("Terminated CEF.");
		}

		private void CreateNewInstance()
		{
			var id = new BrowserInstanceIdentifier(++instanceIdCounter);
			var isMainInstance = instances.Count == 0;
			var instanceLogger = logger.CloneFor($"BrowserInstance {id}");
			var instance = new BrowserApplicationInstance(appConfig, settings, id, isMainInstance, instanceLogger, text, uiFactory);

			instance.Initialize();
			instance.ConfigurationDownloadRequested += (fileName, args) => ConfigurationDownloadRequested?.Invoke(fileName, args);
			instance.Terminated += Instance_Terminated;

			button.RegisterInstance(instance);
			instances.Add(instance);
			instance.Window.Show();

			logger.Info($"Created browser instance {instance.Id}.");
		}

		private CefSettings InitializeCefSettings()
		{
			var warning = appConfig.LogLevel == LogLevel.Warning;
			var error = appConfig.LogLevel == LogLevel.Error;
			var cefSettings = new CefSettings
			{
				CachePath = appConfig.BrowserCachePath,
				LogFile = appConfig.BrowserLogFile,
				LogSeverity = error ? LogSeverity.Error : (warning ? LogSeverity.Warning : LogSeverity.Info)
			};

			logger.Debug($"CEF cache path is '{cefSettings.CachePath}'.");
			logger.Debug($"CEF log file is '{cefSettings.LogFile}'.");
			logger.Debug($"CEF log severity is '{cefSettings.LogSeverity}'.");

			return cefSettings;
		}

		private void Button_OnClick(InstanceIdentifier id = null)
		{
			if (id is null)
			{
				CreateNewInstance();
			}
			else
			{
				instances.FirstOrDefault(i => i.Id == id)?.Window?.BringToForeground();
			}
		}

		private void Instance_Terminated(InstanceIdentifier id)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));
			logger.Info($"Browser instance {id} was terminated.");
		}
	}
}
