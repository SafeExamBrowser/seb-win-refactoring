/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/57.0.0/html/T_CefSharp_ISchemeHandlerFactory.htm.
	/// </remarks>
	internal class SebSchemeHandlerFactory : ISchemeHandlerFactory
	{
		internal event ConfigurationDetectedEventHandler ConfigurationDetected;

		public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
		{
			var handler = new SebSchemeHandler();

			handler.ConfigurationDetected += (url) => ConfigurationDetected?.Invoke(url);

			return handler;
		}
	}
}
