/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using CefSharp;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class ResourceHandler : CefSharp.Handler.ResourceRequestHandler
	{
		private AppConfig appConfig;
		private BrowserFilterSettings settings;
		private ILogger logger;
		private RequestFilter filter;
		private IResourceHandler contentBlockedHandler;
		private IResourceHandler pageBlockedHandler;
		private IText text;

		internal ResourceHandler(AppConfig appConfig, BrowserFilterSettings settings, ILogger logger, IText text)
		{
			this.appConfig = appConfig;
			this.filter = new RequestFilter();
			this.logger = logger;
			this.settings = settings;
			this.text = text;
		}

		internal void Initialize()
		{
			if (settings.FilterMainRequests || settings.FilterContentRequests)
			{
				InitializeFilter();
			}
		}

		protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
		{
			if (BlockMainRequest(request))
			{
				return pageBlockedHandler;
			}

			if (BlockContentRequest(request))
			{
				return contentBlockedHandler;
			}

			return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
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

		private bool BlockContentRequest(IRequest request)
		{
			if (settings.FilterContentRequests && request.ResourceType != ResourceType.MainFrame)
			{
				var result = filter.Process(request.Url);
				var block = result == FilterResult.Block;

				if (block)
				{
					logger.Info($"Blocked content request for '{request.Url}'.");
				}

				return block;
			}

			return false;
		}

		private bool BlockMainRequest(IRequest request)
		{
			if (settings.FilterMainRequests && request.ResourceType == ResourceType.MainFrame)
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
			var assembly = Assembly.GetAssembly(typeof(RequestFilter));
			var contentMessage = text.Get(TextKey.Browser_BlockedContentMessage);
			var contentStream = assembly.GetManifestResourceStream($"{typeof(RequestFilter).Namespace}.BlockedContent.html");
			var pageButton = text.Get(TextKey.Browser_BlockedPageButton);
			var pageMessage = text.Get(TextKey.Browser_BlockedPageMessage);
			var pageTitle = text.Get(TextKey.Browser_BlockedPageTitle);
			var pageStream = assembly.GetManifestResourceStream($"{typeof(RequestFilter).Namespace}.BlockedPage.html");
			var contentHtml = new StreamReader(contentStream).ReadToEnd();
			var pageHtml = new StreamReader(pageStream).ReadToEnd();

			contentHtml = contentHtml.Replace("%%MESSAGE%%", contentMessage);
			pageHtml = pageHtml.Replace("%%MESSAGE%%", pageMessage).Replace("%%TITLE%%", pageTitle).Replace("%%BACK_BUTTON%%", pageButton);

			contentBlockedHandler = CefSharp.ResourceHandler.FromString(contentHtml);
			pageBlockedHandler = CefSharp.ResourceHandler.FromString(pageHtml);

			foreach (var rule in settings.Rules)
			{
				filter.Load(rule);
			}

			logger.Debug($"Initialized request filter with {settings.Rules.Count} rules.");
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
