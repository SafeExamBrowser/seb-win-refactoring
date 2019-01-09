/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core;

namespace SafeExamBrowser.Contracts.Browser
{
	/// <summary>
	/// Controls the lifetime and functionality of the browser application.
	/// </summary>
	public interface IBrowserApplicationController : IApplicationController
	{
		/// <summary>
		/// Event fired when the browser application detects a download request for an application configuration file.
		/// </summary>
		event DownloadRequestedEventHandler ConfigurationDownloadRequested;
	}
}
