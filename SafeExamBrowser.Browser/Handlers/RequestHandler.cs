/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RequestHandler : CefSharp.Handler.RequestHandler
	{
		private RequestFilter filter;
		private ILogger logger;
		private ResourceHandler resourceHandler;
		private BrowserFilterSettings settings;

		internal event RequestBlockedEventHandler RequestBlocked;

		internal RequestHandler(AppConfig appConfig, BrowserFilterSettings settings, ILogger logger, IText text)
		{
			this.filter = new RequestFilter();
			this.logger = logger;
			this.resourceHandler = new ResourceHandler(appConfig, settings, filter, logger, text);
			this.settings = settings;
		}

		internal void Initiailize()
		{
			if (settings.FilterMainRequests || settings.FilterContentRequests)
			{
				InitializeFilter();
			}

			resourceHandler.Initialize();
		}

		protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, 	bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
		{
			return resourceHandler;
		}

		protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
		{
			if (Block(request))
			{
				RequestBlocked?.Invoke(request.Url);

				return true;
			}

			return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
		}

		private bool Block(IRequest request)
		{
			if (settings.FilterMainRequests)
			{
				var result = filter.Process(request.Url);
				var block = result == FilterResult.Block;

				if (block)
				{
					logger.Info($"Blocked main request for '{request.Url}'.");
				}

				return block;
			}

			return false;
		}

		private void InitializeFilter()
		{
			foreach (var rule in settings.Rules)
			{
				filter.Load(rule);
			}

			logger.Debug($"Initialized request filter with {settings.Rules.Count} rules.");
		}
	}
}
