/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Settings.Browser.Proxy;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal class ConfigurationResponsibility : BrowserResponsibility, IFunctionalResponsibility<BrowserTask>
	{
		public ConfigurationResponsibility(BrowserApplicationContext context) : base(context)
		{
		}

		public override void Assume(BrowserTask task)
		{
			switch (task)
			{
				case BrowserTask.InitializePreferences:
					InitializePreferences();
					break;
			}
		}

		public bool TryAssume<TResult>(BrowserTask task, out TResult result) where TResult : class
		{
			result = default;

			if (task == BrowserTask.InitializeBrowserConfiguration)
			{
				result = InitializeCefSettings() as TResult;
			}

			return result != default;
		}

		private CefSettings InitializeCefSettings()
		{
			var warning = Logger.LogLevel == LogLevel.Warning;
			var error = Logger.LogLevel == LogLevel.Error;
			var settings = new CefSettings();

			settings.AcceptLanguageList = CultureInfo.CurrentUICulture.Name;
			settings.CachePath = AppConfig.BrowserCachePath;
			settings.LogFile = AppConfig.BrowserLogFilePath;
			settings.LogSeverity = error ? LogSeverity.Error : (warning ? LogSeverity.Warning : LogSeverity.Info);
			settings.PersistSessionCookies = !Settings.DeleteCookiesOnStartup || !Settings.DeleteCookiesOnShutdown;
			settings.UserAgent = InitializeUserAgent();

			settings.CefCommandLineArgs.Add("do-not-de-elevate");
			settings.CefCommandLineArgs.Add("enable-chrome-runtime", "1");
			settings.CefCommandLineArgs.Add("no-sandbox");
			settings.CefCommandLineArgs.Add("disable-gpu");
			settings.CefCommandLineArgs.Add("disable-gpu-compositing");
			settings.CefCommandLineArgs.Add("disable-software-rasterizer");
			settings.CefCommandLineArgs.Add("disable-dev-shm-usage");
			settings.CefCommandLineArgs.Add("enable-experimental-web-platform-features");
			settings.CefCommandLineArgs.Add("extensions-on-chrome-urls");
			settings.CefCommandLineArgs.Add("enable-media-stream");
			settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
			settings.CefCommandLineArgs.Add("touch-events", "enabled");
			settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
			settings.CefCommandLineArgs.Add("disable-web-security");
			settings.CefCommandLineArgs.Add("allow-running-insecure-content");
			settings.CefCommandLineArgs.Add("enable-blink-features", "ShadowDOMV0");
			settings.CefCommandLineArgs.Add("disable-background-timer-throttling");
			settings.CefCommandLineArgs.Add("disable-backgrounding-occluded-windows");
			settings.CefCommandLineArgs.Add("disable-renderer-backgrounding");
			settings.CefCommandLineArgs.Add("no-pings");

			var extensionPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions", "AST");
			if (System.IO.Directory.Exists(extensionPath))
			{
				settings.CefCommandLineArgs.Add("load-extension", extensionPath);
				Logger.Info($"[Extensions] Added 'load-extension' argument for: {extensionPath}");
			}
			else
			{
				Logger.Warn($"[Extensions] Extension NOT found for command line: {extensionPath}");
			}

			if (!Settings.AllowPageZoom)
			{
				settings.CefCommandLineArgs.Add("disable-pinch");
			}

			if (!Settings.AllowPdfReader)
			{
				settings.CefCommandLineArgs.Add("disable-pdf-extension");
			}

			if (!Settings.AllowSpellChecking)
			{
				settings.CefCommandLineArgs.Add("disable-spell-checking");
			}

			InitializeProxySettings(settings);

			Logger.Debug($"Accept Language: {settings.AcceptLanguageList}");
			Logger.Debug($"Cache Path: {settings.CachePath}");
			Logger.Debug($"Engine Version: Chromium {Cef.ChromiumVersion}, CEF {Cef.CefVersion}, CefSharp {Cef.CefSharpVersion}");
			Logger.Debug($"Log File: {settings.LogFile}");
			Logger.Debug($"Log Severity: {settings.LogSeverity}.");
			Logger.Debug($"PDF Reader: {(Settings.AllowPdfReader ? "Enabled" : "Disabled")}.");
			Logger.Debug($"Session Persistence: {(settings.PersistSessionCookies ? "Enabled" : "Disabled")}.");

			return settings;
		}

		private void InitializePreferences()
		{
			Cef.UIThreadTaskFactory.StartNew(() =>
			{
				using (var requestContext = Cef.GetGlobalRequestContext())
				{
					requestContext.SetPreference("autofill.credit_card_enabled", false, out _);
					requestContext.SetPreference("autofill.profile_enabled", false, out _);
				}
			});
		}

		private void InitializeProxySettings(CefSettings settings)
		{
			if (Settings.Proxy.Policy == ProxyPolicy.Custom)
			{
				if (Settings.Proxy.AutoConfigure)
				{
					settings.CefCommandLineArgs.Add("proxy-pac-url", Settings.Proxy.AutoConfigureUrl);
				}

				if (Settings.Proxy.AutoDetect)
				{
					settings.CefCommandLineArgs.Add("proxy-auto-detect", "");
				}

				if (Settings.Proxy.BypassList.Any())
				{
					settings.CefCommandLineArgs.Add("proxy-bypass-list", string.Join(";", Settings.Proxy.BypassList));
				}

				if (Settings.Proxy.Proxies.Any())
				{
					var proxies = new List<string>();

					foreach (var proxy in Settings.Proxy.Proxies)
					{
						proxies.Add($"{ToScheme(proxy.Protocol)}={proxy.Host}:{proxy.Port}");
					}

					settings.CefCommandLineArgs.Add("proxy-server", string.Join(";", proxies));
				}
			}
		}

		private string InitializeUserAgent()
		{
			var osVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
			var sebVersion = $"SEB/{AppConfig.ProgramInformationalVersion}";
			var userAgent = default(string);

			if (Settings.UseCustomUserAgent)
			{
				userAgent = $"{Settings.CustomUserAgent} {sebVersion}";
			}
			else
			{
				userAgent = $"Mozilla/5.0 (Windows NT {osVersion}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{Cef.ChromiumVersion} {sebVersion}";
			}

			if (!string.IsNullOrWhiteSpace(Settings.UserAgentSuffix))
			{
				userAgent = $"{userAgent} {Settings.UserAgentSuffix}";
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
	}
}
