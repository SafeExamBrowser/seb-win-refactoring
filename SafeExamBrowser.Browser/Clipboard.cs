/*
 * Copyright (c) 2026 ETH Zürich, IT Services
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
		private const int MaxContentLength = 16 * 1024 * 1024;

		private readonly object syncLock = new object();
		private readonly ILogger logger;
		private readonly BrowserSettings settings;

		private string content;

		internal string Content
		{
			get
			{
				lock (syncLock)
				{
					return content;
				}
			}
			private set
			{
				lock (syncLock)
				{
					content = value;
				}
			}
		}

		internal event ClipboardChangedEventHandler Changed;

		internal Clipboard(ILogger logger, BrowserSettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		internal void Clear()
		{
			lock (syncLock)
			{
				content = default;
			}
		}

		internal void Update(JavascriptMessageReceivedEventArgs message)
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
					logger.Error($"Failed to process browser message '{message?.Message}'!", e);
				}
			}
		}

		private bool TrySetContent(object value)
		{
			if (value is string text)
			{
				if (text.Length > MaxContentLength)
				{
					logger.Warn($"Clipboard content of {text.Length} characters exceeds maximum allowed length of {MaxContentLength} characters; ignored.");
					return false;
				}

				Content = text;
				return true;
			}

			return false;
		}

		private class Data
		{
			public string Content { get; set; }
			public string Id { get; set; }
			public string Type { get; set; }
		}
	}
}
