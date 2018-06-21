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

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/63.0.0/html/T_CefSharp_Handler_DefaultRequestHandler.htm.
	/// </remarks>
	internal class RequestHandler : DefaultRequestHandler
	{
		public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			var uri = new Uri(request.Url);

			// TODO: Move to globals -> SafeExamBrowserUriScheme, SafeExamBrowserSecureUriScheme
			if (uri.Scheme == "seb")
			{
				request.Url = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp }.ToString();
			}
			else if (uri.Scheme == "sebs")
			{
				request.Url = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps }.ToString();
			}

			return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
		}
	}
}
