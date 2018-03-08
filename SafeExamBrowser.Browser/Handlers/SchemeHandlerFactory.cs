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
	/// See https://cefsharp.github.io/api/63.0.0/html/T_CefSharp_ISchemeHandlerFactory.htm.
	/// </remarks>
	internal class SchemeHandlerFactory : ISchemeHandlerFactory
	{
		public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
		{
			var page = "<html><body style=\"height: 90%; display: flex; align-items: center; justify-content: center\"><progress /></body></html>";
			var handler = ResourceHandler.FromString(page);

			return handler;
		}
	}
}
