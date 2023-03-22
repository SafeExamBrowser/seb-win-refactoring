/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.DataResources;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataResources
{
	public class NetworkResourceLoader : IResourceLoader
	{
		private readonly AppConfig appConfig;
		private readonly ILogger logger;

		/// <remarks>
		/// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types.
		/// </remarks>
		private string[] SupportedContentTypes => new[]
		{
			MediaTypeNames.Application.Octet,
			MediaTypeNames.Text.Xml
		};

		private string[] SupportedSchemes => new[]
		{
			appConfig.SebUriScheme,
			appConfig.SebUriSchemeSecure,
			Uri.UriSchemeHttp,
			Uri.UriSchemeHttps
		};

		public NetworkResourceLoader(AppConfig appConfig, ILogger logger)
		{
			this.appConfig = appConfig;
			this.logger = logger;
		}

		public bool CanLoad(Uri resource)
		{
			var available = SupportedSchemes.Contains(resource.Scheme) && IsAvailable(resource);

			if (available)
			{
				logger.Debug($"Can load '{resource}' as it references an existing network resource.");
			}
			else
			{
				logger.Debug($"Can't load '{resource}' since its URI scheme is not supported or the resource is unavailable.");
			}

			return available;
		}

		public LoadStatus TryLoad(Uri resource, out Stream data)
		{
			var uri = BuildUriFor(resource);

			logger.Debug($"Sending GET request for '{uri}'...");

			var request = Build(HttpMethod.Get, uri);
			var response = Execute(request);

			logger.Debug($"Received response '{ToString(response)}'.");

			if (IsUnauthorized(response) || HasHtmlContent(response))
			{
				return HandleBrowserResource(response, out data);
			}

			logger.Debug($"Trying to extract response data...");
			data = Extract(response.Content);
			logger.Debug($"Created '{data}' for {data.Length / 1000.0} KB data of response body.");

			return LoadStatus.Success;
		}

		private HttpRequestMessage Build(HttpMethod method, Uri uri)
		{
			var request = new HttpRequestMessage(method, uri);
			var success = request.Headers.TryAddWithoutValidation("User-Agent", $"SEB/{appConfig.ProgramInformationalVersion}");

			if (!success)
			{
				logger.Warn("Failed to add user agent header to request!");
			}

			return request;
		}

		private Uri BuildUriFor(Uri resource)
		{
			var scheme = GetSchemeFor(resource);
			var builder = new UriBuilder(resource) { Scheme = scheme };

			return builder.Uri;
		}

		private HttpResponseMessage Execute(HttpRequestMessage request)
		{
			var task = Task.Run(async () =>
			{
				using (var client = new HttpClient())
				{
					return await client.SendAsync(request);
				}
			});

			return task.GetAwaiter().GetResult();
		}

		private Stream Extract(HttpContent content)
		{
			var task = Task.Run(async () =>
			{
				return await content.ReadAsStreamAsync();
			});

			return task.GetAwaiter().GetResult();
		}

		private string GetSchemeFor(Uri resource)
		{
			if (resource.Scheme == appConfig.SebUriScheme)
			{
				return Uri.UriSchemeHttp;
			}

			if (resource.Scheme == appConfig.SebUriSchemeSecure)
			{
				return Uri.UriSchemeHttps;
			}

			return resource.Scheme;
		}

		private LoadStatus HandleBrowserResource(HttpResponseMessage response, out Stream data)
		{
			data = default;

			logger.Debug($"The {(IsUnauthorized(response) ? "resource needs authentication" : " response data is HTML")}.");

			return LoadStatus.LoadWithBrowser;
		}

		/// <remarks>
		/// See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type.
		/// </remarks>
		private bool HasHtmlContent(HttpResponseMessage response)
		{
			return response.Content.Headers.ContentType?.MediaType == MediaTypeNames.Text.Html;
		}

		private bool IsAvailable(Uri resource)
		{
			var isAvailable = false;
			var uri = BuildUriFor(resource);

			try
			{
				logger.Debug($"Sending HEAD request for '{uri}'...");

				var request = Build(HttpMethod.Head, uri);
				var response = Execute(request);

				isAvailable = response.IsSuccessStatusCode || IsUnauthorized(response);
				logger.Debug($"Received response '{ToString(response)}'.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to check availability of '{resource}' via HEAD request!", e);
			}

			if (!isAvailable)
			{
				try
				{
					logger.Debug($"HEAD request was not successful, trying GET request for '{uri}'...");

					var request = Build(HttpMethod.Get, uri);
					var response = Execute(request);

					isAvailable = response.IsSuccessStatusCode || IsUnauthorized(response);
					logger.Debug($"Received response '{ToString(response)}'.");
				}
				catch (Exception e)
				{
					logger.Error($"Failed to check availability of '{resource}' via GET request!", e);
				}
			}

			return isAvailable;
		}

		private bool IsUnauthorized(HttpResponseMessage response)
		{
			return response.StatusCode == HttpStatusCode.Unauthorized;
		}

		private string ToString(HttpResponseMessage response)
		{
			return $"{(int) response.StatusCode} - {response.ReasonPhrase}";
		}
	}
}
