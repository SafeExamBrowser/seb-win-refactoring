/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.ResourceLoaders
{
	public class NetworkResourceLoader : IResourceLoader
	{
		private AppConfig appConfig;
		private ILogger logger;

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
			if (SupportedSchemes.Contains(resource.Scheme) && IsAvailable(resource))
			{
				logger.Debug($"Can load '{resource}' as it references an existing network resource.");

				return true;
			}

			logger.Debug($"Can't load '{resource}' since its URI scheme is not supported or the resource is unavailable.");

			return false;
		}

		public byte[] Load(Uri resource)
		{
			var uri = BuildUriFor(resource);

			logger.Debug($"Downloading data from '{uri}'...");

			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			var response = Execute(request);

			logger.Debug($"Sent GET request for '{uri}', received response '{(int) response.StatusCode} - {response.ReasonPhrase}'.");

			var data = Extract(response.Content);

			logger.Debug($"Extracted {data.Length / 1000.0} KB data from response.");

			return data;
		}

		private bool IsAvailable(Uri resource)
		{
			try
			{
				var uri = BuildUriFor(resource);
				var request = new HttpRequestMessage(HttpMethod.Head, uri);
				var response = Execute(request);

				logger.Debug($"Sent HEAD request for '{uri}', received response '{(int) response.StatusCode} - {response.ReasonPhrase}'.");

				return response.IsSuccessStatusCode;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to check availability of '{resource}'!", e);

				return false;
			}
		}

		private Uri BuildUriFor(Uri resource)
		{
			var scheme = GetSchemeFor(resource);
			var builder = new UriBuilder(resource) { Scheme = scheme };

			return builder.Uri;
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

		private byte[] Extract(HttpContent content)
		{
			var task = Task.Run(async () =>
			{
				return await content.ReadAsByteArrayAsync();
			});

			return task.GetAwaiter().GetResult();
		}
	}
}
