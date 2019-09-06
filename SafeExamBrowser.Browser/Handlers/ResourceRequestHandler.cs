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
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using BrowserSettings = SafeExamBrowser.Configuration.Contracts.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class ResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
	{
		private AppConfig appConfig;
		private BrowserSettings settings;
		private ILogger logger;

		internal ResourceRequestHandler(AppConfig appConfig, BrowserSettings settings, ILogger logger)
		{
			this.appConfig = appConfig;
			this.settings = settings;
			this.logger = logger;
		}

		protected override IResourceHandler GetResourceHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request)
		{
			if (FilterMainRequest(request) || FilterContentRequest(request))
			{
				return ResourceHandler.FromString("<html><body>Blocked!</body></html>");
			}

			return base.GetResourceHandler(webBrowser, browser, frame, request);
		}

		protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			// TODO: CEF does not yet support intercepting requests from service workers, thus the user agent must be statically set at browser
			//       startup for now. Once CEF has full support of service workers, the static user agent should be removed and the method below
			//       reactivated. See https://bitbucket.org/chromiumembedded/cef/issues/2622 for the current status of development.
			// AppendCustomUserAgent(request);

			if (IsMailtoUrl(request.Url))
			{
				return CefReturnValue.Cancel;
			}

			ReplaceCustomScheme(request);

			return base.OnBeforeResourceLoad(webBrowser, browser, frame, request, callback);
		}

		private void AppendCustomUserAgent(IRequest request)
		{
			var headers = new NameValueCollection(request.Headers);
			var userAgent = request.Headers["User-Agent"];

			headers["User-Agent"] = $"{userAgent} SEB/{appConfig.ProgramInformationalVersion}";
			request.Headers = headers;
		}

		private bool FilterContentRequest(IRequest request)
		{
			return settings.FilterContentRequests && request.ResourceType != ResourceType.MainFrame;
		}

		private bool FilterMainRequest(IRequest request)
		{
			return settings.FilterMainRequests && request.ResourceType == ResourceType.MainFrame;
		}

		private bool IsMailtoUrl(string url)
		{
			return url.StartsWith(Uri.UriSchemeMailto);
		}

		private void ReplaceCustomScheme(IRequest request)
		{
			if (Uri.IsWellFormedUriString(request.Url, UriKind.RelativeOrAbsolute))
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
}
