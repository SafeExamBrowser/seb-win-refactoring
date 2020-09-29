/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.RegularExpressions;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RequestHandler : CefSharp.Handler.RequestHandler
	{
		private IRequestFilter filter;
		private ILogger logger;
		private string quitUrlPattern;
		private ResourceHandler resourceHandler;
		private WindowSettings windowSettings;
		private BrowserSettings settings;

		internal event UrlEventHandler QuitUrlVisited;
		internal event UrlEventHandler RequestBlocked;

		internal RequestHandler(
			AppConfig appConfig,
			IRequestFilter filter,
			ILogger logger,
			ResourceHandler resourceHandler,
			BrowserSettings settings,
			WindowSettings windowSettings,
			IText text)
		{
			this.filter = filter;
			this.logger = logger;
			this.resourceHandler = resourceHandler;
			this.settings = settings;
			this.windowSettings = windowSettings;
		}

		protected override bool GetAuthCredentials(IWebBrowser webBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
		{
			if (isProxy)
			{
				foreach (var proxy in settings.Proxy.Proxies)
				{
					if (proxy.RequiresAuthentication && host?.Equals(proxy.Host, StringComparison.OrdinalIgnoreCase) == true && port == proxy.Port)
					{
						callback.Continue(proxy.Username, proxy.Password);

						return true;
					}
				}
			}

			return base.GetAuthCredentials(webBrowser, browser, originUrl, isProxy, host, port, realm, scheme, callback);
		}

		protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
		{
			return resourceHandler;
		}

		protected override bool OnBeforeBrowse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
		{
			if (IsQuitUrl(request))
			{
				QuitUrlVisited?.Invoke(request.Url);

				return true;
			}

			if (Block(request))
			{
				if (request.ResourceType == ResourceType.MainFrame)
				{
					RequestBlocked?.Invoke(request.Url);
				}

				return true;
			}

			return base.OnBeforeBrowse(webBrowser, browser, frame, request, userGesture, isRedirect);
		}

		private bool IsQuitUrl(IRequest request)
		{
			var isQuitUrl = false;

			if (!string.IsNullOrWhiteSpace(settings.QuitUrl))
			{
				if (quitUrlPattern == default(string))
				{
					quitUrlPattern = Regex.Escape(settings.QuitUrl.TrimEnd('/')) + @"\/?";
				}

				isQuitUrl = Regex.IsMatch(request.Url, quitUrlPattern, RegexOptions.IgnoreCase);

				if (isQuitUrl)
				{
					logger.Debug($"Detected quit URL{(windowSettings.UrlPolicy.CanLog() ? $"'{request.Url}'" : "")}.");
				}
			}

			return isQuitUrl;
		}

		private bool Block(IRequest request)
		{
			var block = false;

			if (settings.Filter.ProcessMainRequests && request.ResourceType == ResourceType.MainFrame)
			{
				var result = filter.Process(new Request { Url = request.Url });

				if (result == FilterResult.Block)
				{
					block = true;
					logger.Info($"Blocked main request{(windowSettings.UrlPolicy.CanLog() ? $" for '{request.Url}'" : "")} ({request.ResourceType}, {request.TransitionType}).");
				}
			}

			if (settings.Filter.ProcessContentRequests && request.ResourceType != ResourceType.MainFrame)
			{
				var result = filter.Process(new Request { Url = request.Url });

				if (result == FilterResult.Block)
				{
					block = true;
					logger.Info($"Blocked content request{(windowSettings.UrlPolicy.CanLog() ? $" for '{request.Url}'" : "")} ({request.ResourceType}, {request.TransitionType}).");
				}
			}

			return block;
		}
	}
}
