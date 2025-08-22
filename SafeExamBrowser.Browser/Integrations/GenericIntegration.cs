/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using CefSharp;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Browser.Integrations
{
	internal class GenericIntegration : Integration
	{
		private readonly ILogger logger;

		public GenericIntegration(ILogger logger)
		{
			this.logger = logger;
		}

		internal override bool TrySearchUserIdentifier(Cookie cookie, out string userIdentifier)
		{
			userIdentifier = default;

			return false;
		}

		internal override bool TrySearchUserIdentifier(IRequest request, IResponse response, out string userIdentifier)
		{
			var ids = response.Headers.GetValues("X-LMS-USER-ID");
			var id = ids?.FirstOrDefault();

			userIdentifier = default;

			if (HasChanged(id))
			{
				userIdentifier = id;
				logger.Info($"User identifier '{id}' detected by header of response.");
			}

			return userIdentifier != default;
		}
	}
}
