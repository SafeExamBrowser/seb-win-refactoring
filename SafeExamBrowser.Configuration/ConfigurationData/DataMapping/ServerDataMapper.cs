/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class ServerDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Server.Configuration:
					MapConfiguration(settings, value);
					break;
				case Keys.Server.FallbackPasswordHash:
					MapFallbackPasswordHash(settings, value);
					break;
				case Keys.Server.PerformFallback:
					MapPerformFallback(settings, value);
					break;
				case Keys.Server.RequestAttempts:
					MapRequestAttempts(settings, value);
					break;
				case Keys.Server.RequestAttemptInterval:
					MapRequestAttemptInterval(settings, value);
					break;
				case Keys.Server.RequestTimeout:
					MapRequestTimeout(settings, value);
					break;
				case Keys.Server.ServerUrl:
					MapServerUrl(settings, value);
					break;
			}
		}

		private void MapConfiguration(AppSettings settings, object value)
		{
			if (value is IDictionary<string, object> configuration)
			{
				if (configuration.TryGetValue(Keys.Server.ApiUrl, out var v) && v is string url)
				{
					settings.Server.ApiUrl = url;
				}

				if (configuration.TryGetValue(Keys.Server.ClientName, out v) && v is string name)
				{
					settings.Server.ClientName = name;
				}

				if (configuration.TryGetValue(Keys.Server.ClientSecret, out v) && v is string secret)
				{
					settings.Server.ClientSecret = secret;
				}

				if (configuration.TryGetValue(Keys.Server.ExamId, out v) && v is string examId)
				{
					settings.Server.ExamId = examId;
				}

				if (configuration.TryGetValue(Keys.Server.Institution, out v) && v is string institution)
				{
					settings.Server.Institution = institution;
				}

				if (configuration.TryGetValue(Keys.Server.PingInterval, out v) && v is int interval)
				{
					settings.Server.PingInterval = interval;
				}
			}
		}

		private void MapFallbackPasswordHash(AppSettings settings, object value)
		{
			if (value is string hash)
			{
				settings.Server.FallbackPasswordHash = hash;
			}
		}

		private void MapPerformFallback(AppSettings settings, object value)
		{
			if (value is bool perform)
			{
				settings.Server.PerformFallback = perform;
			}
		}

		private void MapRequestAttempts(AppSettings settings, object value)
		{
			if (value is int attempts)
			{
				settings.Server.RequestAttempts = attempts;
			}
		}

		private void MapRequestAttemptInterval(AppSettings settings, object value)
		{
			if (value is int interval)
			{
				settings.Server.RequestAttemptInterval = interval;
			}
		}

		private void MapRequestTimeout(AppSettings settings, object value)
		{
			if (value is int timeout)
			{
				settings.Server.RequestTimeout = timeout;
			}
		}

		private void MapServerUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Server.ServerUrl = url;
			}
		}
	}
}
