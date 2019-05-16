/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using CefSharp;
using CefSharp.Handler;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/71.0.0/html/T_CefSharp_Handler_DefaultRequestHandler.htm.
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
			// TODO: CEF does not yet support intercepting requests from service workers, thus the user agent must be statically set at browser
			//       startup for now. Once CEF has full support of service workers, the static user agent should be removed and the method below
			//       reactivated. See https://bitbucket.org/chromiumembedded/cef/issues/2622 for the current status of development.
			// AppendCustomUserAgent(request);

			ReplaceCustomScheme(request);

			return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
		}

		private void AppendCustomUserAgent(IRequest request)
		{
			var headers = new NameValueCollection(request.Headers);
			var userAgent = request.Headers["User-Agent"];

			headers["User-Agent"] = $"{userAgent} SEB/{appConfig.ProgramVersion}";
			request.Headers = headers;
		}

		private void ReplaceCustomScheme(IRequest request)
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
		}
	}
}
