/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace SafeExamBrowser.Browser.Handlers
{
	internal delegate void ConfigurationDetectedEventHandler(string url);

	/// <remarks>
	/// See https://cefsharp.github.io/api/57.0.0/html/T_CefSharp_ResourceHandler.htm.
	/// </remarks>
	internal class SebSchemeHandler : ResourceHandler
	{
		internal event ConfigurationDetectedEventHandler ConfigurationDetected;

		public override bool ProcessRequestAsync(IRequest request, ICallback callback)
		{
			Task.Run(() =>
			{
				using (callback)
				{
					var page = "<html><body style=\"height: 90%; display: flex; align-items: center; justify-content: center\"><progress /></body></html>";
					var stream = GetMemoryStream(page, Encoding.UTF8);

					ResponseLength = stream.Length;
					MimeType = "text/html";
					StatusCode = (int) HttpStatusCode.OK;
					Stream = stream;

					callback.Continue();
					ConfigurationDetected?.Invoke(request.Url);
				}
			});

			return true;
		}
	}
}
