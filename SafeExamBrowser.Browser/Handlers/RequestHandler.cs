/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using BrowserSettings = SafeExamBrowser.Configuration.Contracts.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RequestHandler : CefSharp.Handler.RequestHandler
	{
		private AppConfig appConfig;
		private BrowserSettings settings;
		private RequestFilter filter;
		private ILogger logger;
		private ResourceRequestHandler resourceRequestHandler;

		internal RequestHandler(AppConfig appConfig, BrowserSettings settings, ILogger logger)
		{
			this.appConfig = appConfig;
			this.settings = settings;
			this.filter = new RequestFilter();
			this.logger = logger;
			this.resourceRequestHandler = new ResourceRequestHandler(appConfig, settings, logger);
		}

		internal void Initiailize()
		{
			if (settings.FilterMainRequests || settings.FilterContentRequests)
			{
				foreach (var rule in settings.FilterRules)
				{
					filter.Load(rule);
				}

				logger.Debug("Initialized request filter.");
			}
		}

		protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
		{
			return resourceRequestHandler;
		}
	}
}
