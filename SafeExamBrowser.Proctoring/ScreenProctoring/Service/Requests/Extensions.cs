/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Http;
using System.Text;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests
{
	internal static class Extensions
	{
		internal static string ToLogString(this HttpResponseMessage response)
		{
			return response == default ? "No Response" : $"{(int) response.StatusCode} {response.StatusCode} {response.ReasonPhrase}";
		}

		internal static string ToSummary(this Exception exception)
		{
			var trimChars = new[] { '.', '!' };
			var summary = new StringBuilder(exception.Message?.TrimEnd(trimChars));

			for (var inner = exception.InnerException; inner != default; inner = inner.InnerException)
			{
				summary.Append($" -> {inner.Message?.TrimEnd(trimChars)}");
			}

			return summary.ToString();
		}

		internal static long ToUnixTimestamp(this DateTime date)
		{
			return new DateTimeOffset(date).ToUnixTimeMilliseconds();
		}
	}
}
