﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.Win32;

namespace SafeExamBrowser.Monitoring.Contracts.System.Events
{
	/// <summary>
	/// Indicates that the active user session of the operating system has changed.
	/// </summary>
	public delegate void SessionChangedEventHandler(SessionSwitchReason reason);
}
