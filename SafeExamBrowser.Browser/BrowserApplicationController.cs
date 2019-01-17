/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Browser.Events;
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
		private IMessageBox messageBox;
		private IModuleLogger logger;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		public BrowserApplicationController(
			AppConfig appConfig,
			BrowserSettings settings,
			IMessageBox messageBox,
			IModuleLogger logger,
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

			logger.Info("Initialized browser engine.");

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

		public void Start()
		{
			CreateNewInstance();
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

			logger.Info("Terminated browser engine.");
		}

		private void CreateNewInstance(BrowserSettings custom = null)
		{
			var id = new BrowserInstanceIdentifier(++instanceIdCounter);
			var isMainInstance = instances.Count == 0;
			var instanceLogger = logger.CloneFor($"BrowserInstance {id}");
			var instanceSettings = custom ?? settings;
			var instance = new BrowserApplicationInstance(appConfig, instanceSettings, id, isMainInstance, messageBox, instanceLogger, text, uiFactory);

			instance.Initialize();
			instance.ConfigurationDownloadRequested += (fileName, args) => ConfigurationDownloadRequested?.Invoke(fileName, args);
			instance.PopupRequested += Instance_PopupRequested;
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
				LogSeverity = error ? LogSeverity.Error : (warning ? LogSeverity.Warning : LogSeverity.Info),
				UserAgent = settings.UseCustomUserAgent ? settings.CustomUserAgent : string.Empty
			};

			logger.Debug($"Browser cache path: {cefSettings.CachePath}");
			logger.Debug($"Browser log file: {cefSettings.LogFile}");
			logger.Debug($"Browser log severity: {cefSettings.LogSeverity}");
			logger.Debug($"Browser engine version: Chromium {Cef.ChromiumVersion}, CEF {Cef.CefVersion}, CefSharp {Cef.CefSharpVersion}");

			return cefSettings;
		}

		private void Button_OnClick(InstanceIdentifier id = null)
		{
			if (id == null)
			{
				CreateNewInstance();
			}
			else
			{
				instances.FirstOrDefault(i => i.Id == id)?.Window?.BringToForeground();
			}
		}

		private void Instance_PopupRequested(PopupRequestedEventArgs args)
		{
			var popupSettings = new BrowserSettings
			{
				AllowAddressBar = false,
				AllowBackwardNavigation = false,
				AllowConfigurationDownloads = settings.AllowConfigurationDownloads,
				AllowDeveloperConsole = settings.AllowDeveloperConsole,
				AllowDownloads = settings.AllowDownloads,
				AllowForwardNavigation = false,
				AllowPageZoom = settings.AllowPageZoom,
				AllowPopups = settings.AllowPopups,
				AllowReloading = settings.AllowReloading,
				ShowReloadWarning = settings.ShowReloadWarning,
				StartUrl = args.Url
			};

			logger.Info($"Received request to create new instance for '{args.Url}'...");
			CreateNewInstance(popupSettings);
		}

		private void Instance_Terminated(InstanceIdentifier id)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));
			logger.Info($"Browser instance {id} was terminated.");
		}
	}
}
