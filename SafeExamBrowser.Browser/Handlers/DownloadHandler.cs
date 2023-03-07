/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using Syroot.Windows.IO;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class DownloadHandler : IDownloadHandler
	{
		private readonly AppConfig appConfig;
		private readonly ConcurrentDictionary<int, DownloadFinishedCallback> callbacks;
		private readonly ConcurrentDictionary<int, Guid> downloads;
		private readonly ILogger logger;
		private readonly BrowserSettings settings;
		private readonly WindowSettings windowSettings;

		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		internal event DownloadAbortedEventHandler DownloadAborted;
		internal event DownloadUpdatedEventHandler DownloadUpdated;

		internal DownloadHandler(AppConfig appConfig, ILogger logger, BrowserSettings settings, WindowSettings windowSettings)
		{
			this.appConfig = appConfig;
			this.callbacks = new ConcurrentDictionary<int, DownloadFinishedCallback>();
			this.downloads = new ConcurrentDictionary<int, Guid>();
			this.logger = logger;
			this.settings = settings;
			this.windowSettings = windowSettings;
		}

		public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
		{
			return true;
		}

		public void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			var fileExtension = Path.GetExtension(downloadItem.SuggestedFileName);
			var isConfigurationFile = false;
			var url = downloadItem.Url;
			var urlExtension = default(string);

			if (downloadItem.Url.StartsWith("data:"))
			{
				url = downloadItem.Url.Length <= 100 ? downloadItem.Url : downloadItem.Url.Substring(0, 100) + "...";
			}

			if (Uri.TryCreate(downloadItem.Url, UriKind.RelativeOrAbsolute, out var uri))
			{
				urlExtension = Path.GetExtension(uri.AbsolutePath);
			}

			isConfigurationFile |= string.Equals(appConfig.ConfigurationFileExtension, fileExtension, StringComparison.OrdinalIgnoreCase);
			isConfigurationFile |= string.Equals(appConfig.ConfigurationFileExtension, urlExtension, StringComparison.OrdinalIgnoreCase);
			isConfigurationFile |= string.Equals(appConfig.ConfigurationFileMimeType, downloadItem.MimeType, StringComparison.OrdinalIgnoreCase);

			logger.Debug($"Detected download request{(windowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")}.");

			if (isConfigurationFile)
			{
				Task.Run(() => RequestConfigurationFileDownload(downloadItem, callback));
			}
			else if (settings.AllowDownloads)
			{
				Task.Run(() => HandleFileDownload(downloadItem, callback));
			}
			else
			{
				logger.Info($"Aborted download request{(windowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")}, as downloading is not allowed.");
				Task.Run(() => DownloadAborted?.Invoke());
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
				logger.Debug($"Download of '{downloadItem.FullPath}' {(downloadItem.IsComplete ? "is complete" : "was cancelled")}.");

				if (callbacks.TryRemove(downloadItem.Id, out var finished) && finished != null)
				{
					Task.Run(() => finished.Invoke(downloadItem.IsComplete, downloadItem.Url, downloadItem.FullPath));
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
			var showDialog = settings.AllowCustomDownAndUploadLocation;

			logger.Debug($"Handling download of file '{downloadItem.SuggestedFileName}'.");

			if (!string.IsNullOrEmpty(settings.DownAndUploadDirectory))
			{
				filePath = Path.Combine(Environment.ExpandEnvironmentVariables(settings.DownAndUploadDirectory), downloadItem.SuggestedFileName);
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
			var args = new DownloadEventArgs { Url = downloadItem.Url };

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
