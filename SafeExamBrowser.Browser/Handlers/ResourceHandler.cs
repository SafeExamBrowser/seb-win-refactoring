/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using CefSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Browser.Content;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Filters;
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
		private readonly IKeyGenerator keyGenerator;
		private readonly ILogger logger;
		private readonly SessionMode sessionMode;
		private readonly BrowserSettings settings;
		private readonly WindowSettings windowSettings;

		private IResourceHandler contentHandler;
		private IResourceHandler pageHandler;
		private string sessionIdentifier;

		internal event SessionIdentifierDetectedEventHandler SessionIdentifierDetected;

		internal ResourceHandler(
			AppConfig appConfig,
			IRequestFilter filter,
			IKeyGenerator keyGenerator,
			ILogger logger,
			SessionMode sessionMode,
			BrowserSettings settings,
			WindowSettings windowSettings,
			IText text)
		{
			this.appConfig = appConfig;
			this.filter = filter;
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
				SearchSessionIdentifiers(request, response);
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
				SearchSessionIdentifiers(request, response);
			}

			return base.OnResourceResponse(webBrowser, browser, frame, request, response);
		}

		private void AppendCustomHeaders(IWebBrowser webBrowser, IRequest request)
		{
			Uri.TryCreate(webBrowser.Address, UriKind.Absolute, out var pageUrl);
			Uri.TryCreate(request.Url, UriKind.Absolute, out var requestUrl);

			if (pageUrl?.Host?.Equals(requestUrl?.Host) == true)
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

		private void SearchSessionIdentifiers(IRequest request, IResponse response)
		{
			var success = TrySearchGenericSessionIdentifier(response);

			if (!success)
			{
				SearchEdxIdentifier(response);
				SearchMoodleIdentifier(request, response);
			}
		}

		private bool TrySearchGenericSessionIdentifier(IResponse response)
		{
			var ids = response.Headers.GetValues("X-LMS-USER-ID");

			if (ids != default(string[]))
			{
				var userId = ids.FirstOrDefault();

				if (userId != default && sessionIdentifier != userId)
				{
					sessionIdentifier = userId;
					Task.Run(() => SessionIdentifierDetected?.Invoke(sessionIdentifier));
					logger.Info("Generic LMS session detected.");

					return true;
				}
			}

			return false;
		}

		private void SearchEdxIdentifier(IResponse response)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");

			if (cookies != default(string[]))
			{
				try
				{
					var userInfo = cookies.FirstOrDefault(c => c.Contains("edx-user-info"));

					if (userInfo != default)
					{
						var start = userInfo.IndexOf("=") + 1;
						var end = userInfo.IndexOf("; expires");
						var cookie = userInfo.Substring(start, end - start);
						var sanitized = cookie.Replace("\\\"", "\"").Replace("\\054", ",").Trim('"');
						var json = JsonConvert.DeserializeObject(sanitized) as JObject;
						var userName = json["username"].Value<string>();

						if (sessionIdentifier != userName)
						{
							sessionIdentifier = userName;
							Task.Run(() => SessionIdentifierDetected?.Invoke(sessionIdentifier));
							logger.Info("EdX session detected.");
						}
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to parse edX session identifier!", e);
				}
			}
		}

		private void SearchMoodleIdentifier(IRequest request, IResponse response)
		{
			var success = TrySearchByLocation(response);

			if (!success)
			{
				TrySearchBySession(request, response);
			}
		}

		private bool TrySearchByLocation(IResponse response)
		{
			var locations = response.Headers.GetValues("Location");

			if (locations != default(string[]))
			{
				try
				{
					var location = locations.FirstOrDefault(l => l.Contains("/login/index.php?testsession"));

					if (location != default)
					{
						var userId = location.Substring(location.IndexOf("=") + 1);

						if (sessionIdentifier != userId)
						{
							sessionIdentifier = userId;
							Task.Run(() => SessionIdentifierDetected?.Invoke(sessionIdentifier));
							logger.Info("Moodle session detected.");
						}

						return true;
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to parse Moodle session identifier!", e);
				}
			}

			return false;
		}

		private void TrySearchBySession(IRequest request, IResponse response)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");

			if (cookies != default(string[]))
			{
				var session = cookies.FirstOrDefault(c => c.Contains("MoodleSession"));

				if (session != default)
				{
					var requestUrl = request.Url;

					Task.Run(async () =>
					{
						try
						{
							var start = session.IndexOf("=") + 1;
							var end = session.IndexOf(";");
							var value = session.Substring(start, end - start);
							var uri = new Uri(requestUrl);
							var message = new HttpRequestMessage(HttpMethod.Get, $"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}/theme/boost_ethz/sebuser.php");

							using (var handler = new HttpClientHandler { UseCookies = false })
							using (var client = new HttpClient(handler))
							{
								message.Headers.Add("Cookie", $"MoodleSession={value}");

								var result = await client.SendAsync(message);

								if (result.IsSuccessStatusCode)
								{
									var userId = await result.Content.ReadAsStringAsync();

									if (int.TryParse(userId, out var id) && id > 0 && sessionIdentifier != userId)
									{
#pragma warning disable CS4014
										sessionIdentifier = userId;
										Task.Run(() => SessionIdentifierDetected?.Invoke(sessionIdentifier));
										logger.Info("Moodle session detected.");
#pragma warning restore CS4014
									}
								}
								else
								{
									logger.Error($"Failed to retrieve Moodle session identifier! Response: {result.StatusCode} {result.ReasonPhrase}");
								}
							}
						}
						catch (Exception e)
						{
							logger.Error("Failed to parse Moodle session identifier!", e);
						}
					});
				}
			}
		}
	}
}
