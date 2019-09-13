/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RequestHandler : CefSharp.Handler.RequestHandler
	{
		private IRequestFilter filter;
		private ILogger logger;
		private ResourceHandler resourceHandler;
		private BrowserFilterSettings settings;

		internal event RequestBlockedEventHandler RequestBlocked;

		internal RequestHandler(AppConfig appConfig, BrowserFilterSettings settings, IRequestFilter filter, ILogger logger, IText text)
		{
			this.filter = filter;
			this.logger = logger;
			this.resourceHandler = new ResourceHandler(appConfig, settings, filter, logger, text);
			this.settings = settings;
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
			if (settings.ProcessMainRequests)
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
