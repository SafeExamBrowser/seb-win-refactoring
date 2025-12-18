/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class DownloadResponsibility : WindowResponsibility
	{
		private readonly DownloadHandler downloadHandler;

		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		public DownloadResponsibility(BrowserWindowContext context, DownloadHandler downloadHandler) : base(context)
		{
			this.downloadHandler = downloadHandler;
		}

		public override void Assume(WindowTask task)
		{
			if (task == WindowTask.RegisterEvents)
			{
				RegisterEvents();
			}
		}

		private void RegisterEvents()
		{
			downloadHandler.ConfigurationDownloadRequested += DownloadHandler_ConfigurationDownloadRequested;
			downloadHandler.DownloadAborted += DownloadHandler_DownloadAborted;
			downloadHandler.DownloadUpdated += DownloadHandler_DownloadUpdated;
		}

		private void DownloadHandler_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			if (Settings.AllowConfigurationDownloads)
			{
				Logger.Debug($"Forwarding download request for configuration file '{fileName}'.");
				ConfigurationDownloadRequested?.Invoke(fileName, args);

				if (args.AllowDownload)
				{
					Logger.Debug($"Download request for configuration file '{fileName}' was granted.");
				}
				else
				{
					Logger.Debug($"Download request for configuration file '{fileName}' was denied.");
					MessageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle, parent: Window);
				}
			}
			else
			{
				Logger.Debug($"Discarded download request for configuration file '{fileName}'.");
			}
		}

		private void DownloadHandler_DownloadAborted()
		{
			ShowDownUploadNotAllowedMessage();
		}

		private void DownloadHandler_DownloadUpdated(DownloadItemState state)
		{
			Window.UpdateDownloadState(state);
		}
	}
}
