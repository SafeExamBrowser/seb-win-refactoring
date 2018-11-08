/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using CefSharp;
using CefSharp.Handler;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/63.0.0/html/T_CefSharp_Handler_DefaultRequestHandler.htm.
	/// </remarks>
	internal class RequestHandler : DefaultRequestHandler
	{
		private AppConfig appConfig;

		internal RequestHandler(AppConfig appConfig)
		{
			this.appConfig = appConfig;
		}

		public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			var uri = new Uri(request.Url);

			if (uri.Scheme == appConfig.SebUriScheme)
			{
				request.Url = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp }.Uri.AbsoluteUri;
			}
			else if (uri.Scheme == appConfig.SebUriSchemeSecure)
			{
				request.Url = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps }.Uri.AbsoluteUri;
			}

			return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
		}
	}
}
