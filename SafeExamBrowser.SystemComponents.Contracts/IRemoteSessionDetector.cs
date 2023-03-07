/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts
{
	/// <summary>
	/// Provides functionality related to remote session detection.
	/// </summary>
	public interface IRemoteSessionDetector
	{
		/// <summary>
		/// Indicates whether the system is currently running in a remote session.
		/// </summary>
		bool IsRemoteSession();
	}
}
