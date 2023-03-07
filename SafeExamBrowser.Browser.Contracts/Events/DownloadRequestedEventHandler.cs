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
	/// Event handler used to control (e.g. allow or prohibit) download requests.
	/// </summary>
	public delegate void DownloadRequestedEventHandler(string fileName, DownloadEventArgs args);
}
