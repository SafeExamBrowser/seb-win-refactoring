/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Integrations;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class CookieVisitor : ICookieVisitor
	{
		private readonly IEnumerable<Integration> integrations;

		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;

		internal CookieVisitor(IEnumerable<Integration> integrations)
		{
			this.integrations = integrations;
		}

		public void Dispose()
		{
		}

		public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
		{
			foreach (var integration in integrations)
			{
				var success = integration.TrySearchUserIdentifier(cookie, out var userIdentifier);

				if (success)
				{
					Task.Run(() => UserIdentifierDetected?.Invoke(userIdentifier));

					break;
				}
			}

			return true;
		}
	}
}
