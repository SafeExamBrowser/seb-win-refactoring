/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Browser.Events
{
	/// <summary>
	/// Indicates that a page search is being requested.
	/// </summary>
	public delegate void FindRequestedEventHandler(string term, bool isInitial, bool caseSensitive, bool forward = true);
}
