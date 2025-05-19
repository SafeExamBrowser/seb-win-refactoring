/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;

namespace SafeExamBrowser.Browser.Integrations
{
	internal abstract class Integration
	{
		private static string activeUserIdentifier;

		internal abstract bool TrySearchUserIdentifier(Cookie cookie, out string userIdentifier);
		internal abstract bool TrySearchUserIdentifier(IRequest request, IResponse response, out string userIdentifier);

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
