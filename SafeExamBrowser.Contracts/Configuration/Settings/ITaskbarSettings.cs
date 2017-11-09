/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	public interface ITaskbarSettings
	{
		/// <summary>
		/// Determines whether the user may switch the keyboard layout during runtime.
		/// </summary>
		bool AllowKeyboardLayout { get; }

		/// <summary>
		/// Determines whether the user may access the application log during runtime.
		/// </summary>
		bool AllowApplicationLog { get; }

		/// <summary>
		/// Determines whether the user may control the wireless network connection during runtime.
		/// </summary>
		bool AllowWirelessNetwork { get; }
	}
}
