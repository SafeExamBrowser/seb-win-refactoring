/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Monitoring.Events
{
	/// <summary>
	/// Indicates that the input focus has changed to the window with the specified handle.
	/// </summary>
	public delegate void WindowChangedEventHandler(IntPtr window);
}
