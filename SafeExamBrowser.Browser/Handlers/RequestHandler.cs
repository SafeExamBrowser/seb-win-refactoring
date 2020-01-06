/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RequestHandler : CefSharp.Handler.RequestHandler
	{
		private IRequestFilter filter;
		private ILogger logger;
		private ResourceHandler resourceHandler;
		private BrowserSettings settings;

		internal event UrlEventHandler QuitUrlVisited;
		internal event UrlEventHandler RequestBlocked;

		internal RequestHandler(AppConfig appConfig, IRequestFilter filter, ILogger logger, BrowserSettings settings, IText text)
		{
			this.filter = filter;
			this.logger = logger;
			this.settings = settings;
			this.resourceHandler = new ResourceHandler(appConfig, settings.Filter, filter, logger, text);
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
				RequestBlocked?.Invoke(request.Url);

				return true;
			}

			return base.OnBeforeBrowse(webBrowser, browser, frame, request, userGesture, isRedirect);
		}

		private bool IsQuitUrl(IRequest request)
		{
			var isQuitUrl = settings.QuitUrl?.Equals(request.Url, StringComparison.OrdinalIgnoreCase) == true;

			if (isQuitUrl)
			{
				logger.Debug($"Detected quit URL '{request.Url}'.");
			}

			return isQuitUrl;
		}

		private bool Block(IRequest request)
		{
			if (settings.Filter.ProcessMainRequests)
			{
				var result = filter.Process(new Request { Url = request.Url });
				var block = result == FilterResult.Block;

				if (block)
				{
					logger.Info($"Blocked main request for '{request.Url}'.");
				}

				return block;
			}

			return false;
		}
	}
}
