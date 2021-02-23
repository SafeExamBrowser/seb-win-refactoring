/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Pages;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class ResourceHandler : CefSharp.Handler.ResourceRequestHandler
	{
		private SHA256Managed algorithm;
		private AppConfig appConfig;
		private string browserExamKey;
		private IResourceHandler contentHandler;
		private IRequestFilter filter;
		private HtmlLoader htmlLoader;
		private ILogger logger;
		private IResourceHandler pageHandler;
		private BrowserSettings settings;
		private WindowSettings windowSettings;
		private IText text;

		internal event SessionIdentifierDetectedEventHandler SessionIdentifierDetected;

		internal ResourceHandler(
			AppConfig appConfig,
			IRequestFilter filter,
			ILogger logger,
			BrowserSettings settings,
			WindowSettings windowSettings,
			IText text)
		{
			this.appConfig = appConfig;
			this.algorithm = new SHA256Managed();
			this.filter = filter;
			this.htmlLoader = new HtmlLoader(text);
			this.logger = logger;
			this.settings = settings;
			this.windowSettings = windowSettings;
			this.text = text;
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
			SearchSessionIdentifiers(request, response);

			base.OnResourceRedirect(chromiumWebBrowser, browser, frame, request, response, ref newUrl);
		}

		protected override bool OnResourceResponse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			if (RedirectToDisablePdfToolbar(request, response, out var url))
			{
				webBrowser.Load(url);

				return true;
			}

			SearchSessionIdentifiers(request, response);

			return base.OnResourceResponse(webBrowser, browser, frame, request, response);
		}

		private void AppendCustomHeaders(IWebBrowser webBrowser, IRequest request)
		{
			Uri.TryCreate(webBrowser.Address, UriKind.Absolute, out var pageUrl);
			Uri.TryCreate(request.Url, UriKind.Absolute, out var requestUrl);

			if (pageUrl?.Host?.Equals(requestUrl?.Host) == true)
			{
				var headers = new NameValueCollection(request.Headers);
				var urlWithoutFragment = request.Url.Split('#')[0];

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
					logger.Info($"Blocked content request{(windowSettings.UrlPolicy.CanLog() ? $" for '{request.Url}'" : "")} ({request.ResourceType}, {request.TransitionType}).");
				}
			}

			return block;
		}

		private string ComputeBrowserExamKey()
		{
			var salt = settings.ExamKeySalt;

			if (salt == default(byte[]))
			{
				salt = new byte[0];
				logger.Warn("The current configuration does not contain a salt value for the browser exam key!");
			}

			using (var algorithm = new HMACSHA256(salt))
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(appConfig.CodeSignatureHash + appConfig.ProgramBuildVersion + settings.ConfigurationKey));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				browserExamKey = key;

				return browserExamKey;
			}
		}

		private bool IsMailtoUrl(string url)
		{
			return url.StartsWith(Uri.UriSchemeMailto);
		}

		private bool RedirectToDisablePdfToolbar(IRequest request, IResponse response, out string url)
		{
			const string DISABLE_PDF_TOOLBAR = "#toolbar=0";
			var isPdf = response.Headers["Content-Type"] == MediaTypeNames.Application.Pdf;
			var isMainFrame = request.ResourceType == ResourceType.MainFrame;
			var hasFragment = request.Url.Contains(DISABLE_PDF_TOOLBAR);
			var redirect = settings.AllowPdfReader && !settings.AllowPdfReaderToolbar && isPdf && isMainFrame && !hasFragment;

			url = request.Url + DISABLE_PDF_TOOLBAR;

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
				contentHandler = CefSharp.ResourceHandler.FromString(htmlLoader.LoadBlockedContent());
			}

			if (pageHandler == default(IResourceHandler))
			{
				pageHandler = CefSharp.ResourceHandler.FromString(htmlLoader.LoadBlockedPage());
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
			SearchEdxIdentifier(response);
			SearchMoodleIdentifier(request, response);
		}

		private void SearchEdxIdentifier(IResponse response)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");

			if (cookies != default(string[]))
			{
				try
				{
					var userInfo = cookies.FirstOrDefault(c => c.Contains("edx-user-info"));

					if (userInfo != default(string))
					{
						var start = userInfo.IndexOf("=") + 1;
						var end = userInfo.IndexOf("; expires");
						var cookie = userInfo.Substring(start, end - start);
						var sanitized = cookie.Replace("\\\"", "\"").Replace("\\054", ",").Trim('"');
						var json = JsonConvert.DeserializeObject(sanitized) as JObject;
						var userName = json["username"].Value<string>();

						Task.Run(() => SessionIdentifierDetected?.Invoke(userName));
						logger.Info("EdX session detected.");
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

					if (location != default(string))
					{
						var userId = location.Substring(location.IndexOf("=") + 1);

						Task.Run(() => SessionIdentifierDetected?.Invoke(userId));
						logger.Info("Moodle session detected.");

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

		private bool TrySearchBySession(IRequest request, IResponse response)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");

			if (cookies != default(string[]))
			{
				var session = cookies.FirstOrDefault(c => c.Contains("MoodleSession"));

				if (session != default(string))
				{
					try
					{
						var start = session.IndexOf("=") + 1;
						var end = session.IndexOf(";");
						var value = session.Substring(start, end - start);
						var uri = new Uri(request.Url);
						var message = new HttpRequestMessage(HttpMethod.Get, $"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}/user/view.php");

						var task = Task.Run(async () =>
						{
							using (var handler = new HttpClientHandler { UseCookies = false })
							using (var client = new HttpClient(handler))
							{
								message.Headers.Add("Cookie", $"MoodleSession={value}");

								return await client.SendAsync(message);
							}
						});

						var result = task.GetAwaiter().GetResult();
						var id = "id=";

						if (result.RequestMessage.RequestUri.Query.Contains(id))
						{
							var index = result.RequestMessage.RequestUri.Query.IndexOf(id) + id.Length;
							var userId = result.RequestMessage.RequestUri.Query.Substring(index);

							Task.Run(() => SessionIdentifierDetected?.Invoke(userId));
							logger.Info("Moodle session detected.");

							return true;
						}
					}
					catch (Exception e)
					{
						logger.Error("Failed to parse Moodle session identifier!", e);
					}
				}
			}

			return false;
		}
	}
}
