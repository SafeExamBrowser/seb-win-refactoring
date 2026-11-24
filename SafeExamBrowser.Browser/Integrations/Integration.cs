/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using CefSharp;
using SafeExamBrowser.Browser.Integrations.Strategies;

namespace SafeExamBrowser.Browser.Integrations
{
	internal abstract class Integration
	{
		private static string activeUserIdentifier;

		protected virtual IEnumerable<CookieStrategy> CookieStrategies { get; }
		protected virtual IEnumerable<ResponseStrategy> ResponseStrategies { get; }

		protected Integration()
		{
			CookieStrategies = Enumerable.Empty<CookieStrategy>();
			ResponseStrategies = Enumerable.Empty<ResponseStrategy>();
		}

		internal bool TrySearchUserIdentifier(Cookie cookie, out string userIdentifier)
		{
			userIdentifier = default;

			foreach (var strategy in CookieStrategies)
			{
				if (strategy(cookie, out userIdentifier))
				{
					break;
				}
			}

			return userIdentifier != default;
		}

		internal bool TrySearchUserIdentifier(IRequest request, IResponse response, out string userIdentifier)
		{
			userIdentifier = default;

			foreach (var strategy in ResponseStrategies)
			{
				if (strategy(request, response, out userIdentifier))
				{
					break;
				}
			}

			return userIdentifier != default;
		}

		protected bool HasChanged(string userIdentifier)
		{
			var current = activeUserIdentifier;

			if (userIdentifier != default && activeUserIdentifier != userIdentifier)
			{
				activeUserIdentifier = userIdentifier;
			}

			return activeUserIdentifier != current;
		}
	}
}
