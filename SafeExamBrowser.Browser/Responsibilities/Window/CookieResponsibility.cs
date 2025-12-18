/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Browser.Integrations;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class CookieResponsibility : WindowResponsibility
	{
		private readonly IEnumerable<Integration> integrations;

		internal event UserIdentifierDetectedEventHandler UserIdentifierDetected;

		public CookieResponsibility(BrowserWindowContext context, IEnumerable<Integration> integrations) : base(context)
		{
			this.integrations = integrations;
		}

		public override void Assume(WindowTask task)
		{
			if (task == WindowTask.InitiateCookieTraversal)
			{
				InitiateCookieTraversal();
			}
		}

		private void InitiateCookieTraversal()
		{
			var visitor = new CookieVisitor(integrations);

			visitor.UserIdentifierDetected += (id) => UserIdentifierDetected?.Invoke(id);

			if (Cef.GetGlobalCookieManager().VisitAllCookies(visitor))
			{
				Logger.Debug("Successfully initiated cookie traversal.");
			}
			else
			{
				Logger.Warn("Failed to initiate cookie traversal!");
			}
		}
	}
}
