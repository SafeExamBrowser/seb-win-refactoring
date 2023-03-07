/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Contracts.Events
{
	/// <summary>
	/// Defines the callback for system event hooks.
	/// </summary>
	public delegate void SystemEventCallback(IntPtr window);
}
