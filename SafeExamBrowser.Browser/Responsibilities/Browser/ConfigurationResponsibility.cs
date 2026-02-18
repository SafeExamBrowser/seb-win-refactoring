/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.Settings.Browser.Proxy;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal class ConfigurationResponsibility : BrowserResponsibility
	{
		public ConfigurationResponsibility(BrowserApplicationContext context) : base(context)
		{
		}

		public override void Assume(BrowserTask task)
		{
			switch (task)
			{
				case BrowserTask.InitializeBrowserConfiguration:
					InitializeCefSettings();
					break;
				case BrowserTask.InitializePreferences:
					InitializePreferences();
					break;
			}
		}

		private void InitializeCefSettings()
		{
			var warning = Logger.LogLevel == LogLevel.Warning;
			var error = Logger.LogLevel == LogLevel.Error;

			Context.CefSettings = new CefSettings();

			Context.CefSettings.AcceptLanguageList = CultureInfo.CurrentUICulture.Name;
			Context.CefSettings.CachePath = AppConfig.BrowserCachePath;
			Context.CefSettings.LogFile = AppConfig.BrowserLogFilePath;
			Context.CefSettings.LogSeverity = error ? LogSeverity.Error : (warning ? LogSeverity.Warning : LogSeverity.Info);
			Context.CefSettings.PersistSessionCookies = !Settings.DeleteCookiesOnStartup || !Settings.DeleteCookiesOnShutdown;
			Context.CefSettings.UserAgent = InitializeUserAgent();

			Context.CefSettings.CefCommandLineArgs.Add("disable-extensions");
			Context.CefSettings.CefCommandLineArgs.Add("do-not-de-elevate");
			Context.CefSettings.CefCommandLineArgs.Add("enable-media-stream");
			Context.CefSettings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
			Context.CefSettings.CefCommandLineArgs.Add("touch-events", "enabled");
			Context.CefSettings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");

			if (!Settings.AllowPageZoom)
			{
				Context.CefSettings.CefCommandLineArgs.Add("disable-pinch");
			}

			if (!Settings.AllowPdfReader)
			{
				Context.CefSettings.CefCommandLineArgs.Add("disable-pdf-extension");
			}

			if (!Settings.AllowSpellChecking)
			{
				Context.CefSettings.CefCommandLineArgs.Add("disable-spell-checking");
			}

			InitializeProxySettings();

			Logger.Debug($"Accept Language: {Context.CefSettings.AcceptLanguageList}");
			Logger.Debug($"Cache Path: {Context.CefSettings.CachePath}");
			Logger.Debug($"Engine Version: Chromium {Cef.ChromiumVersion}, CEF {Cef.CefVersion}, CefSharp {Cef.CefSharpVersion}");
			Logger.Debug($"Log File: {Context.CefSettings.LogFile}");
			Logger.Debug($"Log Severity: {Context.CefSettings.LogSeverity}.");
			Logger.Debug($"PDF Reader: {(Settings.AllowPdfReader ? "Enabled" : "Disabled")}.");
			Logger.Debug($"Session Persistence: {(Context.CefSettings.PersistSessionCookies ? "Enabled" : "Disabled")}.");

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

		private void InitializeProxySettings()
		{
			if (Settings.Proxy.Policy == ProxyPolicy.Custom)
			{
				if (Settings.Proxy.AutoConfigure)
				{
					Context.CefSettings.CefCommandLineArgs.Add("proxy-pac-url", Settings.Proxy.AutoConfigureUrl);
				}

				if (Settings.Proxy.AutoDetect)
				{
					Context.CefSettings.CefCommandLineArgs.Add("proxy-auto-detect", "");
				}

				if (Settings.Proxy.BypassList.Any())
				{
					Context.CefSettings.CefCommandLineArgs.Add("proxy-bypass-list", string.Join(";", Settings.Proxy.BypassList));
				}

				if (Settings.Proxy.Proxies.Any())
				{
					var proxies = new List<string>();

					foreach (var proxy in Settings.Proxy.Proxies)
					{
						proxies.Add($"{ToScheme(proxy.Protocol)}={proxy.Host}:{proxy.Port}");
					}

					Context.CefSettings.CefCommandLineArgs.Add("proxy-server", string.Join(";", proxies));
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
