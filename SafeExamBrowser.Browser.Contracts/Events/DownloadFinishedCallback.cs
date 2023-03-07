/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Browser.Contracts.Events
{
	/// <summary>
	/// Defines the method signature for callbacks to be executed once a download has been finished. Indicates the URL of the resource,
	/// whether the download was successful, and if so, where it was saved.
	/// </summary>
	public delegate void DownloadFinishedCallback(bool success, string url, string filePath = null);
}
