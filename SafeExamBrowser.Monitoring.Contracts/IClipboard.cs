/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Monitoring.Contracts
{
	/// <summary>
	/// Monitors the system clipboard and provides related functionality.
	/// </summary>
	public interface IClipboard
	{
		/// <summary>
		/// Initializes the system clipboard according to the given policy.
		/// </summary>
		void Initialize(ClipboardPolicy policy);

		/// <summary>
		/// Finalizes the system clipboard.
		/// </summary>
		void Terminate();
	}
}
