/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Applications.Contracts.Resources.Icons;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Proxy;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.WindowsApi.Contracts;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	public class BrowserApplication : IBrowserApplication
	{
		private int instanceIdCounter = default(int);

		private AppConfig appConfig;
		private List<BrowserApplicationInstance> instances;
		private IFileSystemDialog fileSystemDialog;
		private IHashAlgorithm hashAlgorithm;
		private INativeMethods nativeMethods;
		private IMessageBox messageBox;
		private IModuleLogger logger;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public bool AutoStart { get; private set; }
		public IconResource Icon { get; private set; }
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string Tooltip { get; private set; }

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		public event SessionIdentifierDetectedEventHandler SessionIdentifierDetected;
		public event TerminationRequestedEventHandler TerminationRequested;
		public event WindowsChangedEventHandler WindowsChanged;

		public BrowserApplication(
			AppConfig appConfig,
			BrowserSettings settings,
			IFileSystemDialog fileSystemDialog,
			IHashAlgorithm hashAlgorithm,
			INativeMethods nativeMethods,
			IMessageBox messageBox,
			IModuleLogger logger,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.fileSystemDialog = fileSystemDialog;
			this.hashAlgorithm = hashAlgorithm;
			this.nativeMethods = nativeMethods;
			this.instances = new List<BrowserApplicationInstance>();
			this.logger = logger;
			this.messageBox = messageBox;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public IEnumerable<IApplicationWindow> GetWindows()
		{
			return new List<IApplicationWindow>(instances);
		}

		public void Initialize()
		{
			logger.Info("Starting initialization...");

			var cefSettings = InitializeCefSettings();
			var success = Cef.Initialize(cefSettings, true, default(IApp));

			InitializeApplicationInfo();

			if (success)
			{
				if (settings.DeleteCookiesOnStartup)
				{
					DeleteCookies();
				}

				logger.Info("Initialized browser.");
			}
			else
			{
				throw new Exception("Failed to initialize browser!");
			}
		}

		public void Start()
		{
			CreateNewInstance();
		}

		public void Terminate()
		{
			logger.Info("Initiating termination...");

			foreach (var instance in instances)
			{
				instance.Terminated -= Instance_Terminated;
				instance.Terminate();
				logger.Info($"Terminated browser instance {instance.Id}.");
			}

			if (settings.DeleteCookiesOnShutdown)
			{
				DeleteCookies();
			}

			Cef.Shutdown();
			logger.Info("Terminated browser.");

			if (settings.DeleteCacheOnShutdown && settings.DeleteCookiesOnShutdown)
			{
				DeleteCache();
			}
			else
			{
				logger.Info("Retained browser cache.");
			}
		}

		private void CreateNewInstance(string url = null)
		{
			var id = ++instanceIdCounter;
			var isMainInstance = instances.Count == 0;
			var instanceLogger = logger.CloneFor($"Browser Instance #{id}");
			var startUrl = url ?? GenerateStartUrl();
			var instance = new BrowserApplicationInstance(appConfig, settings, id, isMainInstance, fileSystemDialog, hashAlgorithm, messageBox, instanceLogger, text, uiFactory, startUrl);

			instance.ConfigurationDownloadRequested += (fileName, args) => ConfigurationDownloadRequested?.Invoke(fileName, args);
			instance.PopupRequested += Instance_PopupRequested;
			instance.ResetRequested += Instance_ResetRequested;
			instance.SessionIdentifierDetected += (i) => SessionIdentifierDetected?.Invoke(i);
			instance.Terminated += Instance_Terminated;
			instance.TerminationRequested += () => TerminationRequested?.Invoke();

			instance.Initialize();
			instances.Add(instance);

			logger.Info($"Created browser instance {instance.Id}.");
			WindowsChanged?.Invoke();
		}

		private void DeleteCache()
		{
			try
			{
				Directory.Delete(appConfig.BrowserCachePath, true);
				logger.Info("Deleted browser cache.");
			}
			catch (Exception e)
			{
				logger.Error("Failed to delete browser cache!", e);
			}
		}

		private void DeleteCookies()
		{
			var callback = new TaskDeleteCookiesCallback();

			callback.Task.ContinueWith(task =>
			{
				if (!task.IsCompleted || task.Result == TaskDeleteCookiesCallback.InvalidNoOfCookiesDeleted)
				{
					logger.Warn("Failed to delete cookies!");
				}
				else
				{
					logger.Debug($"Deleted {task.Result} cookies.");
				}
			});

			if (Cef.GetGlobalCookieManager().DeleteCookies(callback: callback))
			{
				logger.Debug("Successfully initiated cookie deletion.");
			}
			else
			{
				logger.Warn("Failed to initiate cookie deletion!");
			}
		}

		private string GenerateStartUrl()
		{
			var url = settings.StartUrl;

			if (settings.UseQueryParameter)
			{
				if (url.Contains("?") && settings.StartUrlQuery?.Length > 1 && Uri.TryCreate(url, UriKind.Absolute, out var uri))
				{
					url = url.Replace(uri.Query, $"{uri.Query}&{settings.StartUrlQuery.Substring(1)}");
				}
				else
				{
					url = $"{url}{settings.StartUrlQuery}";
				}
			}

			return url;
		}

		private void InitializeApplicationInfo()
		{
			AutoStart = true;
			Icon = new BrowserIconResource();
			Id = Guid.NewGuid();
			Name = text.Get(TextKey.Browser_Name);
			Tooltip = text.Get(TextKey.Browser_Tooltip);
		}

		private CefSettings InitializeCefSettings()
		{
			var warning = logger.LogLevel == LogLevel.Warning;
			var error = logger.LogLevel == LogLevel.Error;
			var cefSettings = new CefSettings();

			cefSettings.CachePath = appConfig.BrowserCachePath;
			cefSettings.CefCommandLineArgs.Add("touch-events", "enabled");
			cefSettings.LogFile = appConfig.BrowserLogFilePath;
			cefSettings.LogSeverity = error ? LogSeverity.Error : (warning ? LogSeverity.Warning : LogSeverity.Info);
			cefSettings.PersistSessionCookies = !settings.DeleteCookiesOnStartup || !settings.DeleteCookiesOnShutdown;
			cefSettings.UserAgent = InitializeUserAgent();

			if (!settings.AllowPdfReader)
			{
				cefSettings.CefCommandLineArgs.Add("disable-pdf-extension");
			}

			if (!settings.AllowSpellChecking)
			{
				cefSettings.CefCommandLineArgs.Add("disable-spell-checking");
			}

			InitializeProxySettings(cefSettings);

			logger.Debug($"Cache Path: {cefSettings.CachePath}");
			logger.Debug($"Engine Version: Chromium {Cef.ChromiumVersion}, CEF {Cef.CefVersion}, CefSharp {Cef.CefSharpVersion}");
			logger.Debug($"Log File: {cefSettings.LogFile}");
			logger.Debug($"Log Severity: {cefSettings.LogSeverity}.");
			logger.Debug($"PDF Reader: {(settings.AllowPdfReader ? "Enabled" : "Disabled")}.");
			logger.Debug($"Session Persistence: {(cefSettings.PersistSessionCookies ? "Enabled" : "Disabled")}.");

			return cefSettings;
		}

		private void InitializeProxySettings(CefSettings cefSettings)
		{
			if (settings.Proxy.Policy == ProxyPolicy.Custom)
			{
				if (settings.Proxy.AutoConfigure)
				{
					cefSettings.CefCommandLineArgs.Add("proxy-pac-url", settings.Proxy.AutoConfigureUrl);
				}

				if (settings.Proxy.AutoDetect)
				{
					cefSettings.CefCommandLineArgs.Add("proxy-auto-detect", "");
				}

				if (settings.Proxy.BypassList.Any())
				{
					cefSettings.CefCommandLineArgs.Add("proxy-bypass-list", string.Join(";", settings.Proxy.BypassList));
				}

				if (settings.Proxy.Proxies.Any())
				{
					var proxies = new List<string>();

					foreach (var proxy in settings.Proxy.Proxies)
					{
						proxies.Add($"{ToScheme(proxy.Protocol)}={proxy.Host}:{proxy.Port}");
					}

					cefSettings.CefCommandLineArgs.Add("proxy-server", string.Join(";", proxies));
				}
			}
		}

		private string InitializeUserAgent()
		{
			var osVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
			var sebVersion = $"SEB/{appConfig.ProgramInformationalVersion}";
			var userAgent = default(string);

			if (settings.UseCustomUserAgent)
			{
				userAgent = $"{settings.CustomUserAgent} {sebVersion}";
			}
			else
			{
				userAgent = $"Mozilla/5.0 (Windows NT {osVersion}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{Cef.ChromiumVersion} {sebVersion}";
			}

			if (!string.IsNullOrWhiteSpace(settings.UserAgentSuffix))
			{
				userAgent = $"{userAgent} {settings.UserAgentSuffix}";
			}

			return userAgent;
		}

		private string ToScheme(ProxyProtocol protocol)
		{
			switch (protocol)
			{
				case ProxyProtocol.Ftp:
					return Uri.UriSchemeFtp;
				case ProxyProtocol.Http:
					return Uri.UriSchemeHttp;
				case ProxyProtocol.Https:
					return Uri.UriSchemeHttps;
				case ProxyProtocol.Socks:
					return "socks";
			}

			throw new NotImplementedException($"Mapping for proxy protocol '{protocol}' is not yet implemented!");
		}

		private void Instance_PopupRequested(PopupRequestedEventArgs args)
		{
			logger.Info($"Received request to create new instance{(settings.AdditionalWindow.UrlPolicy.CanLog() ? $" for '{args.Url}'" : "")}...");
			CreateNewInstance(args.Url);
		}

		private void Instance_ResetRequested()
		{
			logger.Info("Attempting to reset browser...");

			foreach (var instance in instances)
			{
				instance.Terminated -= Instance_Terminated;
				instance.Terminate();
				logger.Info($"Terminated browser instance {instance.Id}.");
			}

			instances.Clear();
			WindowsChanged?.Invoke();

			if (settings.DeleteCookiesOnStartup && settings.DeleteCookiesOnShutdown)
			{
				DeleteCookies();
			}

			nativeMethods.EmptyClipboard();
			CreateNewInstance();
			logger.Info("Successfully reset browser.");
		}

		private void Instance_Terminated(int id)
		{
			instances.Remove(instances.First(i => i.Id == id));
			WindowsChanged?.Invoke();
		}
	}
}
