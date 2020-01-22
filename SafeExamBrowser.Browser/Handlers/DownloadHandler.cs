/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using Syroot.Windows.IO;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class DownloadHandler : IDownloadHandler
	{
		private AppConfig appConfig;
		private BrowserSettings settings;
		private ConcurrentDictionary<int, DownloadFinishedCallback> callbacks;
		private ConcurrentDictionary<int, Guid> downloads;
		private ILogger logger;

		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		internal event DownloadUpdatedEventHandler DownloadUpdated;

		internal DownloadHandler(AppConfig appConfig, BrowserSettings settings, ILogger logger)
		{
			this.appConfig = appConfig;
			this.callbacks = new ConcurrentDictionary<int, DownloadFinishedCallback>();
			this.downloads = new ConcurrentDictionary<int, Guid>();
			this.logger = logger;
			this.settings = settings;
		}

		public void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var uri = new Uri(downloadItem.Url);
			var extension = Path.GetExtension(uri.AbsolutePath);
			var isConfigFile = String.Equals(extension, appConfig.ConfigurationFileExtension, StringComparison.OrdinalIgnoreCase);

			logger.Debug($"Detected download request for '{uri}'.");

			if (isConfigFile)
			{
				Task.Run(() => RequestConfigurationFileDownload(downloadItem, callback));
			}
			else if (settings.AllowDownloads)
			{
				Task.Run(() => HandleFileDownload(downloadItem, callback));
			}
			else
			{
				logger.Info($"Aborted download request for '{uri}', as downloading is not allowed.");
			}
		}

		public void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{
			var hasId = downloads.TryGetValue(downloadItem.Id, out var id);

			if (hasId)
			{
				var state = new DownloadItemState(id)
				{
					Completion = downloadItem.PercentComplete / 100.0,
					FullPath = downloadItem.FullPath,
					IsCancelled = downloadItem.IsCancelled,
					IsComplete = downloadItem.IsComplete,
					Url = downloadItem.Url
				};
				
				Task.Run(() => DownloadUpdated?.Invoke(state));
			}

			if (downloadItem.IsComplete || downloadItem.IsCancelled)
			{
				logger.Debug($"Download of '{downloadItem.Url}' {(downloadItem.IsComplete ? "is complete" : "was cancelled")}.");

				if (callbacks.TryRemove(downloadItem.Id, out DownloadFinishedCallback finished) && finished != null)
				{
					Task.Run(() => finished.Invoke(downloadItem.IsComplete, downloadItem.FullPath));
				}

				if (hasId)
				{
					downloads.TryRemove(downloadItem.Id, out _);
				}
			}
		}

		private void HandleFileDownload(DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var filePath = default(string);
			var showDialog = settings.AllowCustomDownloadLocation;

			logger.Debug($"Handling download of file '{downloadItem.SuggestedFileName}'.");

			if (!string.IsNullOrEmpty(settings.DownloadDirectory))
			{
				filePath = Path.Combine(Environment.ExpandEnvironmentVariables(settings.DownloadDirectory), downloadItem.SuggestedFileName);
			}
			else
			{
				filePath = Path.Combine(KnownFolders.Downloads.ExpandedPath, downloadItem.SuggestedFileName);
			}

			if (showDialog)
			{
				logger.Debug($"Allowing user to select custom download location, with '{filePath}' as suggestion.");
			}
			else
			{
				logger.Debug($"Automatically downloading file as '{filePath}'.");
			}

			downloads[downloadItem.Id] = Guid.NewGuid();

			using (callback)
			{
				callback.Continue(filePath, showDialog);
			}
		}

		private void RequestConfigurationFileDownload(DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var args = new DownloadEventArgs();

			logger.Debug($"Handling download of configuration file '{downloadItem.SuggestedFileName}'.");
			ConfigurationDownloadRequested?.Invoke(downloadItem.SuggestedFileName, args);

			if (args.AllowDownload)
			{
				if (args.Callback != null)
				{
					callbacks[downloadItem.Id] = args.Callback;
				}

				logger.Debug($"Starting download of configuration file '{downloadItem.SuggestedFileName}'...");

				using (callback)
				{
					callback.Continue(args.DownloadPath, false);
				}
			}
			else
			{
				logger.Debug($"Download of configuration file '{downloadItem.SuggestedFileName}' was cancelled.");
			}
		}
	}
}
