/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using SafeExamBrowser.Settings.Browser.Proxy;
using SafeExamBrowser.Settings.UserInterface;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class BrowserDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Browser.AllowConfigurationDownloads:
					MapAllowConfigurationDownloads(settings, value);
					break;
				case Keys.Browser.AllowCustomDownUploadLocation:
					MapAllowCustomDownloadLocation(settings, value);
					break;
				case Keys.Browser.AllowDeveloperConsole:
					MapAllowDeveloperConsole(settings, value);
					break;
				case Keys.Browser.AllowDownloadsAndUploads:
					MapAllowDownloadsAndUploads(settings, value);
					break;
				case Keys.Browser.AllowFind:
					MapAllowFind(settings, value);
					break;
				case Keys.Browser.AllowPageZoom:
					MapAllowPageZoom(settings, value);
					break;
				case Keys.Browser.AllowPdfReaderToolbar:
					MapAllowPdfReaderToolbar(settings, value);
					break;
				case Keys.Browser.AllowPrint:
					MapAllowPrint(settings, value);
					break;
				case Keys.Browser.AllowSpellChecking:
					MapAllowSpellChecking(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.AllowAddressBar:
					MapAllowAddressBarAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.AllowNavigation:
					MapAllowNavigationAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.AllowReload:
					MapAllowReloadAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.ShowReloadWarning:
					MapShowReloadWarningAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.UrlPolicy:
					MapUrlPolicy(settings.Browser.AdditionalWindow, value);
					break;
				case Keys.Browser.AdditionalWindow.WindowHeight:
					MapWindowHeightAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.WindowPosition:
					MapWindowPositionAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.WindowWidth:
					MapWindowWidthAdditionalWindow(settings, value);
					break;
				case Keys.Browser.DeleteCacheOnShutdown:
					MapDeleteCacheOnShutdown(settings, value);
					break;
				case Keys.Browser.DeleteCookiesOnShutdown:
					MapDeleteCookiesOnShutdown(settings, value);
					break;
				case Keys.Browser.DeleteCookiesOnStartup:
					MapDeleteCookiesOnStartup(settings, value);
					break;
				case Keys.Browser.DownloadDirectory:
					MapDownloadDirectory(settings, value);
					break;
				case Keys.Browser.DownloadPdfFiles:
					MapDownloadPdfFiles(settings, value);
					break;
				case Keys.Browser.EnableBrowser:
					MapEnableBrowser(settings, value);
					break;
				case Keys.Browser.ExamKeySalt:
					MapExamKeySalt(settings, value);
					break;
				case Keys.Browser.Filter.FilterRules:
					MapFilterRules(settings, value);
					break;
				case Keys.Browser.HomeButtonMessage:
					MapHomeButtonMessage(settings, value);
					break;
				case Keys.Browser.HomeButtonRequiresPassword:
					MapHomeButtonRequiresPassword(settings, value);
					break;
				case Keys.Browser.HomeButtonUrl:
					MapHomeButtonUrl(settings, value);
					break;
				case Keys.Browser.HomeButtonUseStartUrl:
					MapHomeButtonUseStartUrl(settings, value);
					break;
				case Keys.Browser.MainWindow.AllowAddressBar:
					MapAllowAddressBar(settings, value);
					break;
				case Keys.Browser.MainWindow.AllowNavigation:
					MapAllowNavigation(settings, value);
					break;
				case Keys.Browser.MainWindow.AllowReload:
					MapAllowReload(settings, value);
					break;
				case Keys.Browser.MainWindow.ShowReloadWarning:
					MapShowReloadWarning(settings, value);
					break;
				case Keys.Browser.MainWindow.UrlPolicy:
					MapUrlPolicy(settings.Browser.MainWindow, value);
					break;
				case Keys.Browser.MainWindow.WindowHeight:
					MapWindowHeightMainWindow(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowMode:
					MapMainWindowMode(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowPosition:
					MapWindowPositionMainWindow(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowWidth:
					MapWindowWidthMainWindow(settings, value);
					break;
				case Keys.Browser.Proxy.Policy:
					MapProxyPolicy(settings, value);
					break;
				case Keys.Browser.Proxy.Settings:
					MapProxySettings(settings, value);
					break;
				case Keys.Browser.QuitUrl:
					MapQuitUrl(settings, value);
					break;
				case Keys.Browser.QuitUrlConfirmation:
					MapQuitUrlConfirmation(settings, value);
					break;
				case Keys.Browser.ResetOnQuitUrl:
					MapResetOnQuitUrl(settings, value);
					break;
				case Keys.Browser.SendCustomHeaders:
					MapSendCustomHeaders(settings, value);
					break;
				case Keys.Browser.ShowFileSystemElementPath:
					MapShowFileSystemElementPath(settings, value);
					break;
				case Keys.Browser.ShowReloadButton:
					MapShowReloadButton(settings, value);
					break;
				case Keys.Browser.ShowToolbar:
					MapShowToolbar(settings, value);
					break;
				case Keys.Browser.StartUrl:
					MapStartUrl(settings, value);
					break;
				case Keys.Browser.UserAgentSuffix:
					MapUserAgentSuffix(settings, value);
					break;
				case Keys.Browser.UseStartUrlQuery:
					MapUseStartUrlQuery(settings, value);
					break;
				case Keys.Browser.UseTemporaryDownUploadDirectory:
					MapUseTemporaryDownUploadDirectory(settings, value);
					break;
			}
		}

		internal override void MapGlobal(IDictionary<string, object> rawData, AppSettings settings)
		{
			MapPopupPolicy(rawData, settings);
			MapRequestFilter(rawData, settings);
			MapUserAgentMode(rawData, settings);
		}

		private void MapAllowAddressBar(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowAddressBar = allow;
			}
		}

		private void MapAllowAddressBarAdditionalWindow(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindow.AllowAddressBar = allow;
			}
		}

		private void MapAllowConfigurationDownloads(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowConfigurationDownloads = allow;
			}
		}

		private void MapAllowCustomDownloadLocation(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowCustomDownAndUploadLocation = allow;
			}
		}

		private void MapAllowDeveloperConsole(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowDeveloperConsole = allow;
				settings.Browser.AdditionalWindow.AllowDeveloperConsole = allow;
			}
		}

		private void MapAllowDownloadsAndUploads(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowDownloads = allow;
				settings.Browser.AllowUploads = allow;
			}
		}

		private void MapAllowFind(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowFind = allow;
			}
		}

		private void MapAllowNavigation(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowBackwardNavigation = allow;
				settings.Browser.MainWindow.AllowForwardNavigation = allow;
			}
		}

		private void MapAllowNavigationAdditionalWindow(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindow.AllowBackwardNavigation = allow;
				settings.Browser.AdditionalWindow.AllowForwardNavigation = allow;
			}
		}

		private void MapAllowPageZoom(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowPageZoom = allow;
			}
		}

		private void MapAllowPdfReaderToolbar(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowPdfReaderToolbar = allow;
			}
		}

		private void MapAllowPrint(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowPrint = allow;
			}
		}

		private void MapAllowSpellChecking(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AllowSpellChecking = allow;
			}
		}

		private void MapAllowReload(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.MainWindow.AllowReloading = allow;
			}
		}

		private void MapAllowReloadAdditionalWindow(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Browser.AdditionalWindow.AllowReloading = allow;
			}
		}

		private void MapDeleteCacheOnShutdown(AppSettings settings, object value)
		{
			if (value is bool delete)
			{
				settings.Browser.DeleteCacheOnShutdown = delete;
			}
		}

		private void MapDeleteCookiesOnShutdown(AppSettings settings, object value)
		{
			if (value is bool delete)
			{
				settings.Browser.DeleteCookiesOnShutdown = delete;
			}
		}

		private void MapDeleteCookiesOnStartup(AppSettings settings, object value)
		{
			if (value is bool delete)
			{
				settings.Browser.DeleteCookiesOnStartup = delete;
			}
		}

		private void MapDownloadDirectory(AppSettings settings, object value)
		{
			if (value is string directory)
			{
				settings.Browser.DownAndUploadDirectory = directory;
			}
		}

		private void MapDownloadPdfFiles(AppSettings settings, object value)
		{
			if (value is bool download)
			{
				settings.Browser.AllowPdfReader = !download;
			}
		}

		private void MapEnableBrowser(AppSettings settings, object value)
		{
			if (value is bool enable)
			{
				settings.Browser.EnableBrowser = enable;
			}
		}

		private void MapExamKeySalt(AppSettings settings, object value)
		{
			if (value is byte[] salt)
			{
				settings.Browser.BrowserExamKeySalt = salt;
			}
		}

		private void MapHomeButtonMessage(AppSettings settings, object value)
		{
			if (value is string message)
			{
				settings.Browser.HomeNavigationMessage = message;
			}
		}

		private void MapHomeButtonRequiresPassword(AppSettings settings, object value)
		{
			if (value is bool requires)
			{
				settings.Browser.HomeNavigationRequiresPassword = requires;
			}
		}

		private void MapHomeButtonUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Browser.HomeUrl = url;
			}
		}

		private void MapHomeButtonUseStartUrl(AppSettings settings, object value)
		{
			if (value is bool use)
			{
				settings.Browser.UseStartUrlAsHomeUrl = use;
			}
		}

		private void MapMainWindowMode(AppSettings settings, object value)
		{
			const int FULLSCREEN = 1;

			if (value is int mode)
			{
				settings.Browser.MainWindow.FullScreenMode = mode == FULLSCREEN;
			}
		}

		private void MapPopupPolicy(IDictionary<string, object> rawData, AppSettings settings)
		{
			const int ALLOW = 2;
			const int BLOCK = 0;
			const int SAME_WINDOW = 1;

			var hasPolicy = rawData.TryGetValue(Keys.Browser.PopupPolicy, out var policy);
			var blockForeignHost = rawData.TryGetValue(Keys.Browser.PopupBlockForeignHost, out var value) && value as bool? == true;

			switch (policy)
			{
				case ALLOW:
					settings.Browser.PopupPolicy = blockForeignHost ? PopupPolicy.AllowSameHost : PopupPolicy.Allow;
					break;
				case BLOCK:
					settings.Browser.PopupPolicy = PopupPolicy.Block;
					break;
				case SAME_WINDOW:
					settings.Browser.PopupPolicy = blockForeignHost ? PopupPolicy.AllowSameHostAndWindow : PopupPolicy.AllowSameWindow;
					break;
			}
		}

		private void MapQuitUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Browser.QuitUrl = url;
			}
		}

		private void MapQuitUrlConfirmation(AppSettings settings, object value)
		{
			if (value is bool confirm)
			{
				settings.Browser.ConfirmQuitUrl = confirm;
			}
		}

		private void MapResetOnQuitUrl(AppSettings settings, object value)
		{
			if (value is bool reset)
			{
				settings.Browser.ResetOnQuitUrl = reset;
			}
		}

		private void MapShowReloadButton(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.AdditionalWindow.ShowReloadButton = show;
				settings.Browser.MainWindow.ShowReloadButton = show;
			}
		}

		private void MapRequestFilter(IDictionary<string, object> rawData, AppSettings settings)
		{
			var processMainRequests = rawData.TryGetValue(Keys.Browser.Filter.EnableMainRequestFilter, out var value) && value as bool? == true;
			var processContentRequests = rawData.TryGetValue(Keys.Browser.Filter.EnableContentRequestFilter, out value) && value as bool? == true;

			settings.Browser.Filter.ProcessMainRequests = processMainRequests;
			settings.Browser.Filter.ProcessContentRequests = processMainRequests && processContentRequests;
		}

		private void MapShowReloadWarning(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.MainWindow.ShowReloadWarning = show;
			}
		}

		private void MapSendCustomHeaders(AppSettings settings, object value)
		{
			if (value is bool send)
			{
				settings.Browser.SendConfigurationKey = send;
				settings.Browser.SendBrowserExamKey = send;
			}
		}

		private void MapShowFileSystemElementPath(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.ShowFileSystemElementPath = show;
			}
		}

		private void MapShowReloadWarningAdditionalWindow(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.AdditionalWindow.ShowReloadWarning = show;
			}
		}

		private void MapShowToolbar(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Browser.AdditionalWindow.ShowToolbar = show;
				settings.Browser.MainWindow.ShowToolbar = show;
			}
		}

		private void MapStartUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Browser.StartUrl = url;
			}
		}

		private void MapUrlPolicy(WindowSettings settings, object value)
		{
			const int ALWAYS = 3;
			const int BEFORE_TITLE = 2;
			const int LOAD_ERROR = 1;
			const int NEVER = 0;

			if (value is int policy)
			{
				switch (policy)
				{
					case ALWAYS:
						settings.UrlPolicy = UrlPolicy.Always;
						break;
					case BEFORE_TITLE:
						settings.UrlPolicy = UrlPolicy.BeforeTitle;
						break;
					case LOAD_ERROR:
						settings.UrlPolicy = UrlPolicy.LoadError;
						break;
					case NEVER:
						settings.UrlPolicy = UrlPolicy.Never;
						break;
				}
			}
		}

		private void MapUserAgentMode(IDictionary<string, object> rawData, AppSettings settings)
		{
			const int DEFAULT = 0;

			var useCustomForDesktop = rawData.TryGetValue(Keys.Browser.UserAgentModeDesktop, out var value) && value as int? != DEFAULT;
			var useCustomForMobile = rawData.TryGetValue(Keys.Browser.UserAgentModeMobile, out value) && value as int? != DEFAULT;

			if (settings.UserInterfaceMode == UserInterfaceMode.Desktop && useCustomForDesktop)
			{
				settings.Browser.UseCustomUserAgent = true;
				settings.Browser.CustomUserAgent = rawData[Keys.Browser.CustomUserAgentDesktop] as string;
			}
			else if (settings.UserInterfaceMode == UserInterfaceMode.Mobile && useCustomForMobile)
			{
				settings.Browser.UseCustomUserAgent = true;
				settings.Browser.CustomUserAgent = rawData[Keys.Browser.CustomUserAgentMobile] as string;
			}
		}

		private void MapUserAgentSuffix(AppSettings settings, object value)
		{
			if (value is string suffix)
			{
				settings.Browser.UserAgentSuffix = suffix;
			}
		}

		private void MapUseStartUrlQuery(AppSettings settings, object value)
		{
			if (value is bool use)
			{
				settings.Browser.UseQueryParameter = use;
			}
		}

		private void MapUseTemporaryDownUploadDirectory(AppSettings settings, object value)
		{
			if (value is bool use)
			{
				settings.Browser.UseTemporaryDownAndUploadDirectory = use;
			}
		}

		private void MapFilterRules(AppSettings settings, object value)
		{
			const int ALLOW = 1;

			if (value is IList<object> ruleDataList)
			{
				foreach (var item in ruleDataList)
				{
					if (item is IDictionary<string, object> ruleData)
					{
						var isActive = ruleData.TryGetValue(Keys.Browser.Filter.RuleIsActive, out var v) && v is bool active && active;

						if (isActive)
						{
							var rule = new FilterRuleSettings();

							if (ruleData.TryGetValue(Keys.Browser.Filter.RuleExpression, out v) && v is string expression)
							{
								rule.Expression = expression;
							}

							if (ruleData.TryGetValue(Keys.Browser.Filter.RuleAction, out v) && v is int action)
							{
								rule.Result = action == ALLOW ? FilterResult.Allow : FilterResult.Block;
							}

							if (ruleData.TryGetValue(Keys.Browser.Filter.RuleExpressionIsRegex, out v) && v is bool regex)
							{
								rule.Type = regex ? FilterRuleType.Regex : FilterRuleType.Simplified;
							}

							settings.Browser.Filter.Rules.Add(rule);
						}
					}
				}
			}
		}

		private void MapProxySettings(AppSettings settings, object value)
		{
			if (value is IDictionary<string, object> data)
			{
				if (data.TryGetValue(Keys.Browser.Proxy.AutoConfigure, out var v) && v is bool autoConfigure)
				{
					settings.Browser.Proxy.AutoConfigure = autoConfigure;
				}

				if (data.TryGetValue(Keys.Browser.Proxy.AutoConfigureUrl, out v) && v is string url)
				{
					settings.Browser.Proxy.AutoConfigureUrl = url;
				}

				if (data.TryGetValue(Keys.Browser.Proxy.AutoDetect, out v) && v is bool autoDetect)
				{
					settings.Browser.Proxy.AutoDetect = autoDetect;
				}

				if (data.TryGetValue(Keys.Browser.Proxy.BypassList, out v) && v is IList<object> list)
				{
					MapProxyBypassList(settings, list);
				}

				if (data.TryGetValue(Keys.Browser.Proxy.Ftp.Enable, out v) && v is bool ftpEnable && ftpEnable)
				{
					MapFtpProxy(settings, data);
				}

				if (data.TryGetValue(Keys.Browser.Proxy.Http.Enable, out v) && v is bool httpEnable && httpEnable)
				{
					MapHttpProxy(settings, data);
				}

				if (data.TryGetValue(Keys.Browser.Proxy.Https.Enable, out v) && v is bool httpsEnable && httpsEnable)
				{
					MapHttpsProxy(settings, data);
				}

				if (data.TryGetValue(Keys.Browser.Proxy.Socks.Enable, out v) && v is bool socksEnable && socksEnable)
				{
					MapSocksProxy(settings, data);
				}
			}
		}

		private void MapProxyBypassList(AppSettings settings, IList<object> bypassList)
		{
			foreach (var item in bypassList)
			{
				if (item is string host)
				{
					settings.Browser.Proxy.BypassList.Add(host);
				}
			}
		}

		private void MapFtpProxy(AppSettings settings, IDictionary<string, object> data)
		{
			var proxy = new ProxyConfiguration { Protocol = ProxyProtocol.Ftp };

			if (data.TryGetValue(Keys.Browser.Proxy.Ftp.Host, out var v) && v is string host)
			{
				proxy.Host = host;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Ftp.Password, out v) && v is string password)
			{
				proxy.Password = password;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Ftp.Port, out v) && v is int port)
			{
				proxy.Port = port;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Ftp.RequiresAuthentication, out v) && v is bool requiresAuthentication)
			{
				proxy.RequiresAuthentication = requiresAuthentication;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Ftp.Username, out v) && v is string username)
			{
				proxy.Username = username;
			}

			settings.Browser.Proxy.Proxies.Add(proxy);
		}

		private void MapHttpProxy(AppSettings settings, IDictionary<string, object> data)
		{
			var proxy = new ProxyConfiguration { Protocol = ProxyProtocol.Http };

			if (data.TryGetValue(Keys.Browser.Proxy.Http.Host, out var v) && v is string host)
			{
				proxy.Host = host;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Http.Password, out v) && v is string password)
			{
				proxy.Password = password;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Http.Port, out v) && v is int port)
			{
				proxy.Port = port;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Http.RequiresAuthentication, out v) && v is bool requiresAuthentication)
			{
				proxy.RequiresAuthentication = requiresAuthentication;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Http.Username, out v) && v is string username)
			{
				proxy.Username = username;
			}

			settings.Browser.Proxy.Proxies.Add(proxy);
		}

		private void MapHttpsProxy(AppSettings settings, IDictionary<string, object> data)
		{
			var proxy = new ProxyConfiguration { Protocol = ProxyProtocol.Https };

			if (data.TryGetValue(Keys.Browser.Proxy.Https.Host, out var v) && v is string host)
			{
				proxy.Host = host;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Https.Password, out v) && v is string password)
			{
				proxy.Password = password;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Https.Port, out v) && v is int port)
			{
				proxy.Port = port;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Https.RequiresAuthentication, out v) && v is bool requiresAuthentication)
			{
				proxy.RequiresAuthentication = requiresAuthentication;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Https.Username, out v) && v is string username)
			{
				proxy.Username = username;
			}

			settings.Browser.Proxy.Proxies.Add(proxy);
		}

		private void MapSocksProxy(AppSettings settings, IDictionary<string, object> data)
		{
			var proxy = new ProxyConfiguration { Protocol = ProxyProtocol.Socks };

			if (data.TryGetValue(Keys.Browser.Proxy.Socks.Host, out var v) && v is string host)
			{
				proxy.Host = host;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Socks.Password, out v) && v is string password)
			{
				proxy.Password = password;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Socks.Port, out v) && v is int port)
			{
				proxy.Port = port;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Socks.RequiresAuthentication, out v) && v is bool requiresAuthentication)
			{
				proxy.RequiresAuthentication = requiresAuthentication;
			}

			if (data.TryGetValue(Keys.Browser.Proxy.Socks.Username, out v) && v is string username)
			{
				proxy.Username = username;
			}

			settings.Browser.Proxy.Proxies.Add(proxy);
		}

		private void MapProxyPolicy(AppSettings settings, object value)
		{
			const int SYSTEM = 0;
			const int CUSTOM = 1;

			if (value is int policy)
			{
				switch (policy)
				{
					case CUSTOM:
						settings.Browser.Proxy.Policy = ProxyPolicy.Custom;
						break;
					case SYSTEM:
						settings.Browser.Proxy.Policy = ProxyPolicy.System;
						break;
				}
			}
		}

		private void MapWindowHeightAdditionalWindow(AppSettings settings, object value)
		{
			if (value is string raw)
			{
				if (raw.EndsWith("%") && int.TryParse(raw.Replace("%", string.Empty), out var relativeHeight))
				{
					settings.Browser.AdditionalWindow.AbsoluteHeight = default;
					settings.Browser.AdditionalWindow.RelativeHeight = relativeHeight;
				}
				else if (int.TryParse(raw, out var absoluteHeight))
				{
					settings.Browser.AdditionalWindow.AbsoluteHeight = absoluteHeight;
					settings.Browser.AdditionalWindow.RelativeHeight = default;
				}
			}
		}

		private void MapWindowHeightMainWindow(AppSettings settings, object value)
		{
			if (value is string raw)
			{
				if (raw.EndsWith("%") && int.TryParse(raw.Replace("%", string.Empty), out var relativeHeight))
				{
					settings.Browser.MainWindow.AbsoluteHeight = default;
					settings.Browser.MainWindow.RelativeHeight = relativeHeight;
				}
				else if (int.TryParse(raw, out var absoluteHeight))
				{
					settings.Browser.MainWindow.AbsoluteHeight = absoluteHeight;
					settings.Browser.MainWindow.RelativeHeight = default;
				}
			}
		}

		private void MapWindowPositionAdditionalWindow(AppSettings settings, object value)
		{
			const int LEFT = 0;
			const int CENTER = 1;
			const int RIGHT = 2;

			if (value is int position)
			{
				switch (position)
				{
					case LEFT:
						settings.Browser.AdditionalWindow.Position = WindowPosition.Left;
						break;
					case CENTER:
						settings.Browser.AdditionalWindow.Position = WindowPosition.Center;
						break;
					case RIGHT:
						settings.Browser.AdditionalWindow.Position = WindowPosition.Right;
						break;
				}
			}
		}

		private void MapWindowPositionMainWindow(AppSettings settings, object value)
		{
			const int LEFT = 0;
			const int CENTER = 1;
			const int RIGHT = 2;

			if (value is int position)
			{
				switch (position)
				{
					case LEFT:
						settings.Browser.MainWindow.Position = WindowPosition.Left;
						break;
					case CENTER:
						settings.Browser.MainWindow.Position = WindowPosition.Center;
						break;
					case RIGHT:
						settings.Browser.MainWindow.Position = WindowPosition.Right;
						break;
				}
			}
		}

		private void MapWindowWidthAdditionalWindow(AppSettings settings, object value)
		{
			if (value is string raw)
			{
				if (raw.EndsWith("%") && int.TryParse(raw.Replace("%", string.Empty), out var relativeWidth))
				{
					settings.Browser.AdditionalWindow.AbsoluteWidth = default;
					settings.Browser.AdditionalWindow.RelativeWidth = relativeWidth;
				}
				else if (int.TryParse(raw, out var absoluteWidth))
				{
					settings.Browser.AdditionalWindow.AbsoluteWidth = absoluteWidth;
					settings.Browser.AdditionalWindow.RelativeWidth = default;
				}
			}
		}

		private void MapWindowWidthMainWindow(AppSettings settings, object value)
		{
			if (value is string raw)
			{
				if (raw.EndsWith("%") && int.TryParse(raw.Replace("%", string.Empty), out var relativeWidth))
				{
					settings.Browser.MainWindow.AbsoluteWidth = default;
					settings.Browser.MainWindow.RelativeWidth = relativeWidth;
				}
				else if (int.TryParse(raw, out var absoluteWidth))
				{
					settings.Browser.MainWindow.AbsoluteWidth = absoluteWidth;
					settings.Browser.MainWindow.RelativeWidth = default;
				}
			}
		}
	}
}
