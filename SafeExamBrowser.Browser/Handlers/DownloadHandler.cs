/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.Logging;
using BrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/67.0.0/html/T_CefSharp_IDownloadHandler.htm.
	/// </remarks>
	internal class DownloadHandler : IDownloadHandler
	{
		private AppConfig appConfig;
		private BrowserSettings settings;
		private ConcurrentDictionary<int, DownloadFinishedCallback> callbacks;
		private ILogger logger;

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		public DownloadHandler(AppConfig appConfig, BrowserSettings settings, ILogger logger)
		{
			this.appConfig = appConfig;
			this.callbacks = new ConcurrentDictionary<int, DownloadFinishedCallback>();
			this.logger = logger;
			this.settings = settings;
		}

		public void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var uri = new Uri(downloadItem.Url);
			var extension = Path.GetExtension(uri.AbsolutePath);
			var isConfigFile = String.Equals(extension, appConfig.ConfigurationFileExtension, StringComparison.OrdinalIgnoreCase);

			logger.Debug($"Handling download request for '{uri}'.");

			if (isConfigFile)
			{
				Task.Run(() => RequestConfigurationFileDownload(downloadItem, callback));
			}
			else if (settings.AllowDownloads)
			{
				logger.Debug($"Starting download of '{uri}'...");

				using (callback)
				{
					callback.Continue(null, true);
				}
			}
			else
			{
				logger.Info($"Aborted download request for '{uri}', as downloading is not allowed.");
			}
		}

		public void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{
			if (downloadItem.IsComplete || downloadItem.IsCancelled)
			{
				if (callbacks.TryRemove(downloadItem.Id, out DownloadFinishedCallback finished) && finished != null)
				{
					Task.Run(() => finished.Invoke(downloadItem.IsComplete, downloadItem.FullPath));
				}

				logger.Debug($"Download of '{downloadItem.Url}' {(downloadItem.IsComplete ? "is complete" : "was cancelled")}.");
			}
		}

		private void RequestConfigurationFileDownload(DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var args = new DownloadEventArgs();

			logger.Debug($"Detected download request for configuration file '{downloadItem.Url}'.");
			ConfigurationDownloadRequested?.Invoke(downloadItem.SuggestedFileName, args);
			logger.Debug($"Download of configuration file '{downloadItem.Url}' was {(args.AllowDownload ? "granted" : "denied")}.");

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
