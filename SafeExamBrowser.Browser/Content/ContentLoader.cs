﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Reflection;
using SafeExamBrowser.I18n.Contracts;

namespace SafeExamBrowser.Browser.Content
{
	internal class ContentLoader
	{
		private readonly IText text;

		private string api;
		private string clipboard;
		private string pageZoom;

		internal ContentLoader(IText text)
		{
			this.text = text;
		}

		internal string LoadApi(string browserExamKey, string configurationKey, string version)
		{
			if (api == default)
			{
				var assembly = Assembly.GetAssembly(typeof(ContentLoader));
				var path = $"{typeof(ContentLoader).Namespace}.Api.js";

				using (var stream = assembly.GetManifestResourceStream(path))
				using (var reader = new StreamReader(stream))
				{
					api = reader.ReadToEnd();
				}
			}

			var js = api;

			js = js.Replace("%%_BEK_%%", browserExamKey);
			js = js.Replace("%%_CK_%%", configurationKey);
			js = js.Replace("%%_VERSION_%%", version);

			return js;
		}

		internal string LoadBlockedContent()
		{
			var assembly = Assembly.GetAssembly(typeof(ContentLoader));
			var path = $"{typeof(ContentLoader).Namespace}.BlockedContent.html";

			using (var stream = assembly.GetManifestResourceStream(path))
			using (var reader = new StreamReader(stream))
			{
				var html = reader.ReadToEnd();

				html = html.Replace("%%MESSAGE%%", text.Get(TextKey.Browser_BlockedContentMessage));

				return html;
			}
		}

		internal string LoadBlockedPage()
		{
			var assembly = Assembly.GetAssembly(typeof(ContentLoader));
			var path = $"{typeof(ContentLoader).Namespace}.BlockedPage.html";

			using (var stream = assembly.GetManifestResourceStream(path))
			using (var reader = new StreamReader(stream))
			{
				var html = reader.ReadToEnd();

				html = html.Replace("%%BACK_BUTTON%%", text.Get(TextKey.Browser_BlockedPageButton));
				html = html.Replace("%%MESSAGE%%", text.Get(TextKey.Browser_BlockedPageMessage));
				html = html.Replace("%%TITLE%%", text.Get(TextKey.Browser_BlockedPageTitle));

				return html;
			}
		}

		internal string LoadClipboard()
		{
			if (clipboard == default)
			{
				var assembly = Assembly.GetAssembly(typeof(ContentLoader));
				var path = $"{typeof(ContentLoader).Namespace}.Clipboard.js";

				using (var stream = assembly.GetManifestResourceStream(path))
				using (var reader = new StreamReader(stream))
				{
					clipboard = reader.ReadToEnd();
				}
			}

			return clipboard;
		}

		internal string LoadPageZoom()
		{
			if (pageZoom == default)
			{
				var assembly = Assembly.GetAssembly(typeof(ContentLoader));
				var path = $"{typeof(ContentLoader).Namespace}.PageZoom.js";

				using (var stream = assembly.GetManifestResourceStream(path))
				using (var reader = new StreamReader(stream))
				{
					pageZoom = reader.ReadToEnd();
				}
			}

			return pageZoom;
		}
	}
}
