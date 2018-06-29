/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Configuration;
using BrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/63.0.0/html/T_CefSharp_IDownloadHandler.htm.
	/// </remarks>
	internal class DownloadHandler : IDownloadHandler
	{
		private AppConfig appConfig;
		private BrowserSettings settings;
		private ConcurrentDictionary<int, DownloadFinishedCallback> callbacks;

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		public DownloadHandler(AppConfig appConfig, BrowserSettings settings)
		{
			this.appConfig = appConfig;
			this.callbacks = new ConcurrentDictionary<int, DownloadFinishedCallback>();
			this.settings = settings;
		}

		public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var uri = new Uri(downloadItem.Url);
			var extension = Path.GetExtension(uri.AbsolutePath);
			var isConfigFile = String.Equals(extension, appConfig.ConfigurationFileExtension, StringComparison.InvariantCultureIgnoreCase);

			if (isConfigFile)
			{
				Task.Run(() => RequestConfigurationFileDownload(downloadItem, callback));
			}
			else if (!isConfigFile && settings.AllowDownloads)
			{
				using (callback)
				{
					callback.Continue(null, true);
				}
			}
		}

		public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{
			if (downloadItem.IsComplete || downloadItem.IsCancelled)
			{
				if (callbacks.TryRemove(downloadItem.Id, out DownloadFinishedCallback finished) && finished != null)
				{
					Task.Run(() => finished.Invoke(downloadItem.IsComplete, downloadItem.FullPath));
				}
			}
		}

		private void RequestConfigurationFileDownload(DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var args = new DownloadEventArgs();

			ConfigurationDownloadRequested?.Invoke(downloadItem.SuggestedFileName, args);

			if (args.AllowDownload)
			{
				if (args.Callback != null)
				{
					callbacks[downloadItem.Id] = args.Callback;
				}

				using (callback)
				{
					callback.Continue(args.DownloadPath, false);
				}
			}
		}
	}
}
