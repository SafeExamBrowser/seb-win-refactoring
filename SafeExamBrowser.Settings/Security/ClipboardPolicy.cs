/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Security
{
	/// <summary>
	/// Defines all policies with respect to the usage of the clipboard.
	/// </summary>
	public enum ClipboardPolicy
	{
		/// <summary>
		/// Allows the usage of the system clipboard without restrictions.
		/// </summary>
		Allow,

		/// <summary>
		/// Completely blocks the usage of the system clipboard by continuously clearing its content and blocking all related keyboard shortcuts.
		/// </summary>
		Block,

		/// <summary>
		/// Continuously clears the content of the system clipboard and enables an isolated clipboard only working within the browser application.
		/// </summary>
		Isolated
	}
}
