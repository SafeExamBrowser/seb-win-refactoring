/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Logging.Contracts;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	internal class Clipboard
	{
		private readonly ILogger logger;
		private readonly BrowserSettings settings;

		internal string Content { get; private set; }

		internal event ClipboardChangedEventHandler Changed;

		internal Clipboard(ILogger logger, BrowserSettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		internal void Process(JavascriptMessageReceivedEventArgs message)
		{
			if (settings.UseIsolatedClipboard)
			{
				try
				{
					var data = message.ConvertMessageTo<Data>();

					if (data != default && data.Type == "Clipboard" && TrySetContent(data.Content))
					{
						Task.Run(() => Changed?.Invoke(data.Id));
					}
				}
				catch (Exception e)
				{
					logger.Error($"Failed to process browser message '{message}'!", e);
				}
			}
		}

		private bool TrySetContent(object value)
		{
			var text = value as string;

			if (text != default)
			{
				Content = text;
			}

			return text != default;
		}

		private class Data
		{
			public string Content { get; set; }
			public long Id { get; set; }
			public string Type { get; set; }
		}
	}
}
