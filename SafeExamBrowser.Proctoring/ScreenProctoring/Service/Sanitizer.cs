/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Reflection;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service
{
	internal class Sanitizer
	{
		internal Uri Sanitize(string serviceUrl)
		{
			return new Uri(serviceUrl.EndsWith("/") ? serviceUrl : $"{serviceUrl}/");
		}

		internal void Sanitize(Api api)
		{
			foreach (var property in api.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var value = property.GetValue(api) as string;
				var sanitized = value.TrimStart('/');

				property.SetValue(api, sanitized);
			}
		}
	}
}
