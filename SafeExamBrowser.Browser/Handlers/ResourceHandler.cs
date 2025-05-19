/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Content;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Integrations;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class ResourceHandler : CefSharp.Handler.ResourceRequestHandler
	{
		private readonly AppConfig appConfig;
		private readonly ContentLoader contentLoader;
		private readonly IRequestFilter filter;
		private readonly IEnumerable<Integration> integrations;
		private readonly IKeyGenerator keyGenerator;
		private readonly ILogger logger;
		private readonly SessionMode sessionMode;
		private readonly BrowserSettings settings;
		private readonly WindowSettings windowSettings;

		private IResourceHandler contentHandler;
		private IResourceHandler pageHandler;

		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;

		internal ResourceHandler(
			AppConfig appConfig,
			IRequestFilter filter,
			IEnumerable<Integration> integrations,
			IKeyGenerator keyGenerator,
			ILogger logger,
			SessionMode sessionMode,
			BrowserSettings settings,
			WindowSettings windowSettings,
			IText text)
		{
			this.appConfig = appConfig;
			this.filter = filter;
			this.integrations = integrations;
			this.contentLoader = new ContentLoader(text);
			this.keyGenerator = keyGenerator;
			this.logger = logger;
			this.sessionMode = sessionMode;
			this.settings = settings;
			this.windowSettings = windowSettings;
		}

		protected override IResourceHandler GetResourceHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request)
		{
			if (Block(request))
			{
				return ResourceHandlerFor(request.ResourceType);
			}

			return base.GetResourceHandler(webBrowser, browser, frame, request);
		}

		protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			if (IsMailtoUrl(request.Url))
			{
				return CefReturnValue.Cancel;
			}

			AppendCustomHeaders(webBrowser, request);
			ReplaceSebScheme(request);

			return base.OnBeforeResourceLoad(webBrowser, browser, frame, request, callback);
		}

		protected override bool OnProtocolExecution(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request)
		{
			return true;
		}

		protected override void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
		{
			if (sessionMode == SessionMode.Server)
			{
				SearchUserIdentifier(request, response);
			}

			base.OnResourceRedirect(chromiumWebBrowser, browser, frame, request, response, ref newUrl);
		}

		protected override bool OnResourceResponse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			if (RedirectToDisablePdfReaderToolbar(request, response, out var url))
			{
				frame?.LoadUrl(url);

				return true;
			}

			if (sessionMode == SessionMode.Server)
			{
				SearchUserIdentifier(request, response);
			}

			return base.OnResourceResponse(webBrowser, browser, frame, request, response);
		}

		private void AppendCustomHeaders(IWebBrowser webBrowser, IRequest request)
		{
			Uri.TryCreate(webBrowser.Address, UriKind.Absolute, out var pageUrl);
			Uri.TryCreate(request.Url, UriKind.Absolute, out var requestUrl);

			if (request.ResourceType == ResourceType.MainFrame || pageUrl?.Host?.Equals(requestUrl?.Host) == true)
			{
				var headers = new NameValueCollection(request.Headers);

				if (settings.SendConfigurationKey)
				{
					headers["X-SafeExamBrowser-ConfigKeyHash"] = keyGenerator.CalculateConfigurationKeyHash(settings.ConfigurationKey, request.Url);
				}

				if (settings.SendBrowserExamKey)
				{
					headers["X-SafeExamBrowser-RequestHash"] = keyGenerator.CalculateBrowserExamKeyHash(settings.ConfigurationKey, settings.BrowserExamKeySalt, request.Url);
				}

				request.Headers = headers;
			}
		}

		private bool Block(IRequest request)
		{
			var block = false;
			var url = WebUtility.UrlDecode(request.Url);
			var isValidUri = Uri.TryCreate(url, UriKind.Absolute, out _);

			if (settings.Filter.ProcessContentRequests && isValidUri)
			{
				var result = filter.Process(new Request { Url = url });

				if (result == FilterResult.Block)
				{
					block = true;
					logger.Info($"Blocked content request{(windowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} ({request.ResourceType}, {request.TransitionType}).");
				}
			}
			else if (!isValidUri)
			{
				logger.Warn($"Filter could not process request{(windowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} ({request.ResourceType}, {request.TransitionType})!");
			}

			return block;
		}

		private bool IsMailtoUrl(string url)
		{
			return url.StartsWith(Uri.UriSchemeMailto);
		}

		private bool RedirectToDisablePdfReaderToolbar(IRequest request, IResponse response, out string url)
		{
			const string DISABLE_PDF_READER_TOOLBAR = "#toolbar=0";

			var isPdf = response.Headers["Content-Type"] == MediaTypeNames.Application.Pdf;
			var isMainFrame = request.ResourceType == ResourceType.MainFrame;
			var hasFragment = request.Url.Contains(DISABLE_PDF_READER_TOOLBAR);
			var redirect = settings.AllowPdfReader && !settings.AllowPdfReaderToolbar && isPdf && isMainFrame && !hasFragment;

			url = request.Url + DISABLE_PDF_READER_TOOLBAR;

			if (redirect)
			{
				logger.Info($"Redirecting{(windowSettings.UrlPolicy.CanLog() ? $" to '{url}'" : "")} to disable PDF reader toolbar.");
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
			if (contentHandler == default(IResourceHandler))
			{
				contentHandler = CefSharp.ResourceHandler.FromString(contentLoader.LoadBlockedContent());
			}

			if (pageHandler == default(IResourceHandler))
			{
				pageHandler = CefSharp.ResourceHandler.FromString(contentLoader.LoadBlockedPage());
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

		private void SearchUserIdentifier(IRequest request, IResponse response)
		{
			foreach (var integration in integrations)
			{
				var success = integration.TrySearchUserIdentifier(request, response, out var userIdentifier);

				if (success)
				{
					Task.Run(() => UserIdentifierDetected?.Invoke(userIdentifier));

					break;
				}
			}
		}
	}
}
