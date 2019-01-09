/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Browser;

namespace SafeExamBrowser.Contracts.Browser
{
	/// <summary>
	/// The event arguments used for all download events.
	/// </summary>
	public class DownloadEventArgs
	{
		/// <summary>
		/// Determines whether the specified download is allowed.
		/// </summary>
		public bool AllowDownload { get; set; }

		/// <summary>
		/// The browser window from which the download request originated.
		/// </summary>
		public IBrowserWindow BrowserWindow { get; set; }

		/// <summary>
		/// Callback executed once a download has been finished.
		/// </summary>
		public DownloadFinishedCallback Callback { get; set; }

		/// <summary>
		/// The full path under which the specified file should be saved.
		/// </summary>
		public string DownloadPath { get; set; }
	}
}
