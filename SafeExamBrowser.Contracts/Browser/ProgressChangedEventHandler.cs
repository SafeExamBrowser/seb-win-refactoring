/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Browser
{
	/// <summary>
	/// Event handler used to indicate the current progress value of the page load process (from <c>0.0</c> to <c>1.0</c>).
	/// </summary>
	public delegate void ProgressChangedEventHandler(double value);
}
