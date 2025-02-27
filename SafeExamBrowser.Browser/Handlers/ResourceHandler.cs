/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
		private string userIdentifier;

		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;

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
			var success = TrySearchGenericUserIdentifier(response);

			if (!success)
			{
				success = TrySearchEdxUserIdentifier(response);
			}

			if (!success)
			{
				TrySearchMoodleUserIdentifier(request, response);
			}
		}

		private bool TrySearchGenericUserIdentifier(IResponse response)
		{
			var ids = response.Headers.GetValues("X-LMS-USER-ID");
			var success = false;

			if (ids != default(string[]))
			{
				var userId = ids.FirstOrDefault();

				if (userId != default && userIdentifier != userId)
				{
					userIdentifier = userId;
					Task.Run(() => UserIdentifierDetected?.Invoke(userIdentifier));
					logger.Info("Generic LMS user identifier detected.");
					success = true;
				}
			}

			return success;
		}

		private bool TrySearchEdxUserIdentifier(IResponse response)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");
			var success = false;

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

						if (userIdentifier != userName)
						{
							userIdentifier = userName;
							Task.Run(() => UserIdentifierDetected?.Invoke(userIdentifier));
							logger.Info("EdX user identifier detected.");
							success = true;
						}
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to parse edX user identifier!", e);
				}
			}

			return success;
		}

		private bool TrySearchMoodleUserIdentifier(IRequest request, IResponse response)
		{
			var success = TrySearchMoodleUserIdentifierByLocation(response);

			if (!success)
			{
				success = TrySearchMoodleUserIdentifierByRequest(MoodleRequestType.Plugin, request, response);
			}

			if (!success)
			{
				success = TrySearchMoodleUserIdentifierByRequest(MoodleRequestType.Theme, request, response);
			}

			return success;
		}

		private bool TrySearchMoodleUserIdentifierByLocation(IResponse response)
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

						if (userIdentifier != userId)
						{
							userIdentifier = userId;
							Task.Run(() => UserIdentifierDetected?.Invoke(userIdentifier));
							logger.Info("Moodle user identifier detected by location.");
						}

						return true;
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to parse Moodle user identifier by location!", e);
				}
			}

			return false;
		}

		private bool TrySearchMoodleUserIdentifierByRequest(MoodleRequestType type, IRequest request, IResponse response)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");
			var success = false;

			if (cookies != default(string[]))
			{
				var session = cookies.FirstOrDefault(c => c.Contains("MoodleSession"));

				if (session != default)
				{
					var userId = ExecuteMoodleUserIdentifierRequest(request.Url, session, type);

					if (int.TryParse(userId, out var id) && id > 0 && userIdentifier != userId)
					{
						userIdentifier = userId;
						Task.Run(() => UserIdentifierDetected?.Invoke(userIdentifier));
						logger.Info($"Moodle user identifier detected by request ({type}).");
						success = true;
					}
				}
			}

			return success;
		}

		private string ExecuteMoodleUserIdentifierRequest(string requestUrl, string session, MoodleRequestType type)
		{
			var userId = default(string);

			try
			{
				Task.Run(async () =>
				{
					try
					{
						var endpointUrl = default(string);
						var start = session.IndexOf("=") + 1;
						var end = session.IndexOf(";");
						var name = session.Substring(0, start - 1);
						var value = session.Substring(start, end - start);
						var uri = new Uri(requestUrl);

						if (type == MoodleRequestType.Plugin)
						{
							endpointUrl = $"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}/mod/quiz/accessrule/sebserver/classes/external/user.php";
						}
						else
						{
							endpointUrl = $"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}/theme/boost_ethz/sebuser.php";
						}

						var message = new HttpRequestMessage(HttpMethod.Get, endpointUrl);

						using (var handler = new HttpClientHandler { UseCookies = false })
						using (var client = new HttpClient(handler))
						{
							message.Headers.Add("Cookie", $"{name}={value}");

							var result = await client.SendAsync(message);

							if (result.IsSuccessStatusCode)
							{
								userId = await result.Content.ReadAsStringAsync();
							}
							else if (result.StatusCode != HttpStatusCode.NotFound)
							{
								logger.Error($"Failed to retrieve Moodle user identifier by request ({type})! Response: {(int) result.StatusCode} {result.ReasonPhrase}");
							}
						}
					}
					catch (Exception e)
					{
						logger.Error($"Failed to parse Moodle user identifier by request ({type})!", e);
					}
				}).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				logger.Error($"Failed to execute Moodle user identifier request ({type})!", e);
			}

			return userId;
		}

		private enum MoodleRequestType
		{
			Plugin,
			Theme
		}
	}
}
