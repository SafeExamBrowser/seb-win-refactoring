/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
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
		private int windowIdCounter = default;

		private readonly AppConfig appConfig;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly IHashAlgorithm hashAlgorithm;
		private readonly IKeyGenerator keyGenerator;
		private readonly IModuleLogger logger;
		private readonly IMessageBox messageBox;
		private readonly INativeMethods nativeMethods;
		private readonly SessionMode sessionMode;
		private readonly BrowserSettings settings;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;
		private readonly List<BrowserWindow> windows;

		public bool AutoStart { get; private set; }
		public IconResource Icon { get; private set; }
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string Tooltip { get; private set; }

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		public event SessionIdentifierDetectedEventHandler SessionIdentifierDetected;
		public event LoseFocusRequestedEventHandler LoseFocusRequested;
		public event TerminationRequestedEventHandler TerminationRequested;
		public event WindowsChangedEventHandler WindowsChanged;

		public BrowserApplication(
			AppConfig appConfig,
			BrowserSettings settings,
			IFileSystemDialog fileSystemDialog,
			IHashAlgorithm hashAlgorithm,
			IKeyGenerator keyGenerator,
			IMessageBox messageBox,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			SessionMode sessionMode,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.fileSystemDialog = fileSystemDialog;
			this.hashAlgorithm = hashAlgorithm;
			this.keyGenerator = keyGenerator;
			this.logger = logger;
			this.messageBox = messageBox;
			this.nativeMethods = nativeMethods;
			this.sessionMode = sessionMode;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
			this.windows = new List<BrowserWindow>();
		}

		public void Focus(bool forward)
		{
			windows.ForEach(window =>
			{
				window.Focus(forward);
			});
		}

		public IEnumerable<IApplicationWindow> GetWindows()
		{
			return new List<IApplicationWindow>(windows);
		}

		public void Initialize()
		{
			logger.Info("Starting initialization...");

			var cefSettings = InitializeCefSettings();
			var success = Cef.Initialize(cefSettings, true, default(IApp));

			InitializeApplicationInfo();

			if (success)
			{
				InitializeIntegrityKeys();

				if (settings.DeleteCookiesOnStartup)
				{
					DeleteCookies();
				}

				if (settings.UseTemporaryDownAndUploadDirectory)
				{
					CreateTemporaryDownAndUploadDirectory();
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
			CreateNewWindow();
		}

		public void Terminate()
		{
			logger.Info("Initiating termination...");
			AwaitReady();

			foreach (var window in windows)
			{
				window.Closed -= Window_Closed;
				window.Close();
				logger.Info($"Closed browser window #{window.Id}.");
			}

			if (settings.UseTemporaryDownAndUploadDirectory)
			{
				DeleteTemporaryDownAndUploadDirectory();
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

		private void AwaitReady()
		{
			// We apparently need to let the browser finish any pending work before attempting to reset or terminate it, especially if the
			// reset or termination is initiated automatically (e.g. by a quit URL). Otherwise, the engine will crash on some occasions, seemingly
			// when it can't finish handling its events (like ChromiumWebBrowser.LoadError).

			Thread.Sleep(500);
		}

		private void CreateNewWindow(PopupRequestedEventArgs args = default)
		{
			var id = ++windowIdCounter;
			var isMainWindow = windows.Count == 0;
			var startUrl = GenerateStartUrl();
			var windowLogger = logger.CloneFor($"Browser Window #{id}");
			var window = new BrowserWindow(
				appConfig,
				fileSystemDialog,
				hashAlgorithm,
				id,
				isMainWindow,
				keyGenerator,
				windowLogger,
				messageBox,
				sessionMode,
				settings,
				startUrl,
				text,
				uiFactory);

			window.Closed += Window_Closed;
			window.ConfigurationDownloadRequested += (f, a) => ConfigurationDownloadRequested?.Invoke(f, a);
			window.PopupRequested += Window_PopupRequested;
			window.ResetRequested += Window_ResetRequested;
			window.SessionIdentifierDetected += (i) => SessionIdentifierDetected?.Invoke(i);
			window.TerminationRequested += () => TerminationRequested?.Invoke();
			window.LoseFocusRequested += (forward) => LoseFocusRequested?.Invoke(forward);

			window.InitializeControl();
			windows.Add(window);

			if (args != default(PopupRequestedEventArgs))
			{
				args.Window = window;
			}
			else
			{
				window.InitializeWindow();
			}

			logger.Info($"Created browser window #{window.Id}.");
			WindowsChanged?.Invoke();
		}

		private void CreateTemporaryDownAndUploadDirectory()
		{
			try
			{
				settings.DownAndUploadDirectory = Path.Combine(appConfig.TemporaryDirectory, Path.GetRandomFileName());
				Directory.CreateDirectory(settings.DownAndUploadDirectory);
				logger.Info($"Created temporary down- and upload directory.");
			}
			catch (Exception e)
			{
				logger.Error("Failed to create temporary down- and upload directory!", e);
			}
		}

		private void DeleteTemporaryDownAndUploadDirectory()
		{
			try
			{
				Directory.Delete(settings.DownAndUploadDirectory, true);
				logger.Info("Deleted temporary down- and upload directory.");
			}
			catch (Exception e)
			{
				logger.Error("Failed to delete temporary down- and upload directory!", e);
			}
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

			cefSettings.AcceptLanguageList = CultureInfo.CurrentUICulture.Name;
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

			cefSettings.CefCommandLineArgs.Add("enable-media-stream");
			cefSettings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
			cefSettings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");

			InitializeProxySettings(cefSettings);

			logger.Debug($"Accept Language: {cefSettings.AcceptLanguageList}");
			logger.Debug($"Cache Path: {cefSettings.CachePath}");
			logger.Debug($"Engine Version: Chromium {Cef.ChromiumVersion}, CEF {Cef.CefVersion}, CefSharp {Cef.CefSharpVersion}");
			logger.Debug($"Log File: {cefSettings.LogFile}");
			logger.Debug($"Log Severity: {cefSettings.LogSeverity}.");
			logger.Debug($"PDF Reader: {(settings.AllowPdfReader ? "Enabled" : "Disabled")}.");
			logger.Debug($"Session Persistence: {(cefSettings.PersistSessionCookies ? "Enabled" : "Disabled")}.");

			return cefSettings;
		}

		private void InitializeIntegrityKeys()
		{
			logger.Debug($"Browser Exam Key (BEK) transmission is {(settings.SendBrowserExamKey ? "enabled" : "disabled")}.");
			logger.Debug($"Configuration Key (CK) transmission is {(settings.SendConfigurationKey ? "enabled" : "disabled")}.");

			if (settings.CustomBrowserExamKey != default)
			{
				keyGenerator.UseCustomBrowserExamKey(settings.CustomBrowserExamKey);
				logger.Debug($"The browser application will be using a custom browser exam key.");
			}
			else
			{
				logger.Debug($"The browser application will be using the default browser exam key.");
			}
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

		private void Window_Closed(int id)
		{
			windows.Remove(windows.First(i => i.Id == id));
			WindowsChanged?.Invoke();
			logger.Info($"Window #{id} has been closed.");
		}

		private void Window_PopupRequested(PopupRequestedEventArgs args)
		{
			logger.Info($"Received request to create new window...");
			CreateNewWindow(args);
		}

		private void Window_ResetRequested()
		{
			logger.Info("Attempting to reset browser...");
			AwaitReady();

			foreach (var window in windows)
			{
				window.Closed -= Window_Closed;
				window.Close();
				logger.Info($"Closed browser window #{window.Id}.");
			}

			windows.Clear();
			WindowsChanged?.Invoke();

			if (settings.DeleteCookiesOnStartup && settings.DeleteCookiesOnShutdown)
			{
				DeleteCookies();
			}

			nativeMethods.EmptyClipboard();
			CreateNewWindow();
			logger.Info("Successfully reset browser.");
		}
	}
}
