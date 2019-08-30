/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class RequestHandler : CefSharp.Handler.RequestHandler
	{
		private AppConfig appConfig;
		private ResourceRequestHandler resourceRequestHandler;

		internal RequestHandler(AppConfig appConfig)
		{
			this.appConfig = appConfig;
			this.resourceRequestHandler = new ResourceRequestHandler(appConfig);
		}

		protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
		{
			return resourceRequestHandler;
		}
	}
}
