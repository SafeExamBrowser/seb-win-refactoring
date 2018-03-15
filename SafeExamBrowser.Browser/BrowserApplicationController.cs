/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CefSharp;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using BrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplicationController : IApplicationController
	{
		private IApplicationButton button;
		private IList<IApplicationInstance> instances = new List<IApplicationInstance>();
		private BrowserSettings settings;
		private ILogger logger;
		private IMessageBox messageBox;
		private IRuntimeProxy runtime;
		private RuntimeInfo runtimeInfo;
		private IUserInterfaceFactory uiFactory;
		private IText text;

		public BrowserApplicationController(
			BrowserSettings settings,
			RuntimeInfo runtimeInfo,
			ILogger logger,
			IMessageBox messageBox,
			IRuntimeProxy runtime,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.messageBox = messageBox;
			this.runtime = runtime;
			this.runtimeInfo = runtimeInfo;
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
			var instance = new BrowserApplicationInstance(settings, text, uiFactory, instances.Count == 0);

			instance.Initialize();
			instance.ConfigurationDetected += Instance_ConfigurationDetected;
			instance.Terminated += Instance_Terminated;

			button.RegisterInstance(instance);
			instances.Add(instance);

			instance.Window.Show();
		}

		private CefSettings InitializeCefSettings()
		{
			var cefSettings = new CefSettings
			{
				CachePath = runtimeInfo.BrowserCachePath,
				LogFile = runtimeInfo.BrowserLogFile,
				// TODO: Set according to current application LogLevel!
				LogSeverity = LogSeverity.Verbose
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

		private void Instance_ConfigurationDetected(string url, CancelEventArgs args)
		{
			var result = messageBox.Show(TextKey.MessageBox_ReconfigurationQuestion, TextKey.MessageBox_ReconfigurationQuestionTitle, MessageBoxAction.YesNo, MessageBoxIcon.Question);
			var reconfigure = result == MessageBoxResult.Yes;
			var allowed = false;

			logger.Info($"Detected configuration request for '{url}'. The user chose to {(reconfigure ? "start" : "abort")} the reconfiguration.");

			if (reconfigure)
			{
				try
				{
					allowed = runtime.RequestReconfiguration(url);
					logger.Info($"The runtime {(allowed ? "accepted" : "denied")} the reconfiguration request.");

					if (!allowed)
					{
						messageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle);
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to communicate the reconfiguration request to the runtime!", e);
					messageBox.Show(TextKey.MessageBox_ReconfigurationError, TextKey.MessageBox_ReconfigurationErrorTitle, icon: MessageBoxIcon.Error);
				}
			}

			args.Cancel = !allowed;
		}

		private void Instance_Terminated(Guid id)
		{
			instances.Remove(instances.FirstOrDefault(i => i.Id == id));
		}
	}
}
