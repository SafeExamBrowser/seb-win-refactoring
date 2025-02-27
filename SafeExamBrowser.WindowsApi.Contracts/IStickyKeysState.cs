/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Represents a state of the sticky keys feature of the operating system.
	/// </summary>
	public interface IStickyKeysState
	{
		/// <summary>
		/// Indicates whether the feature is available.
		/// </summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Indicates whether the feature is enabled.
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Indicates whether the hotkey for the feature is active.
		/// </summary>
		bool IsHotkeyActive { get; }
	}
}
