/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class ResourceHandler : CefSharp.Handler.ResourceRequestHandler
	{
		private SHA256Managed algorithm;
		private AppConfig appConfig;
		private string browserExamKey;
		private IResourceHandler contentHandler;
		private IRequestFilter filter;
		private ILogger logger;
		private IResourceHandler pageHandler;
		private BrowserSettings settings;
		private IText text;

		internal ResourceHandler(AppConfig appConfig, BrowserSettings settings, IRequestFilter filter, ILogger logger, IText text)
		{
			this.appConfig = appConfig;
			this.algorithm = new SHA256Managed();
			this.filter = filter;
			this.logger = logger;
			this.settings = settings;
			this.text = text;
		}

		protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
		{
			if (Block(request))
			{
				return ResourceHandlerFor(request.ResourceType);
			}

			return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
		}

		protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			if (IsMailtoUrl(request.Url))
			{
				return CefReturnValue.Cancel;
			}

			AppendCustomHeaders(request);
			ReplaceSebScheme(request);

			return base.OnBeforeResourceLoad(webBrowser, browser, frame, request, callback);
		}

		protected override bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			var abort = true;

			if (RedirectToDisablePdfToolbar(request, response, out var url))
			{
				chromiumWebBrowser.Load(url);
			}
			else
			{
				abort = base.OnResourceResponse(chromiumWebBrowser, browser, frame, request, response);
			}

			return abort;
		}

		private void AppendCustomHeaders(IRequest request)
		{
			var headers = new NameValueCollection(request.Headers);
			var urlWithoutFragment = request.Url.Split('#')[0];
			var userAgent = request.Headers["User-Agent"];

			// TODO: CEF does not yet support intercepting requests from service workers, thus the user agent must be statically set at browser
			//       startup for now. Once CEF has full support of service workers, the static user agent should be removed and the method below
			//       reactivated. See https://bitbucket.org/chromiumembedded/cef/issues/2622 for the current status of development.
			// headers["User-Agent"] = $"{userAgent} SEB/{appConfig.ProgramInformationalVersion}";

			if (settings.SendConfigurationKey)
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + settings.ConfigurationKey));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				headers["X-SafeExamBrowser-ConfigKeyHash"] = key;
			}

			if (settings.SendExamKey)
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + (browserExamKey ?? ComputeBrowserExamKey())));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				headers["X-SafeExamBrowser-RequestHash"] = key;
			}

			request.Headers = headers;
		}

		private bool Block(IRequest request)
		{
			var block = false;

			if (settings.Filter.ProcessContentRequests)
			{
				var result = filter.Process(new Request { Url = request.Url });

				if (result == FilterResult.Block)
				{
					block = true;
					logger.Info($"Blocked content request for '{request.Url}'.");
				}
			}

			return block;
		}

		private string ComputeBrowserExamKey()
		{
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(settings.ExamKeySalt + appConfig.CodeSignatureHash + appConfig.ProgramBuildVersion + settings.ConfigurationKey));
			var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

			browserExamKey = key;

			return browserExamKey;
		}

		private bool IsMailtoUrl(string url)
		{
			return url.StartsWith(Uri.UriSchemeMailto);
		}

		private bool RedirectToDisablePdfToolbar(IRequest request, IResponse response, out string url)
		{
			const string DISABLE_PDF_TOOLBAR = "#toolbar=0";
			var isPdf = response.Headers["Content-Type"] == MediaTypeNames.Application.Pdf;
			var hasFragment = request.Url.Contains(DISABLE_PDF_TOOLBAR);
			var redirect = settings.AllowPdfReader && !settings.AllowPdfReaderToolbar && isPdf && !hasFragment;

			url = request.Url + DISABLE_PDF_TOOLBAR;

			if (redirect)
			{
				logger.Info($"Redirecting to '{url}' to disable PDF reader toolbar.");
			}

			return redirect;
		}

		private void ReplaceSebScheme(IRequest request)
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

		private IResourceHandler ResourceHandlerFor(ResourceType resourceType)
		{
			if (contentHandler == default(IResourceHandler) || pageHandler == default(IResourceHandler))
			{
				InitializeResourceHandlers();
			}

			switch (resourceType)
			{
				case ResourceType.MainFrame:
				case ResourceType.SubFrame:
					return pageHandler;
				default:
					return contentHandler;
			}
		}

		private void InitializeResourceHandlers()
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
			contentHandler = CefSharp.ResourceHandler.FromString(contentHtml);

			pageHtml = pageHtml.Replace("%%MESSAGE%%", pageMessage).Replace("%%TITLE%%", pageTitle).Replace("%%BACK_BUTTON%%", pageButton);
			pageHandler = CefSharp.ResourceHandler.FromString(pageHtml);

			logger.Debug("Initialized resource handlers for blocked requests.");
		}
	}
}
