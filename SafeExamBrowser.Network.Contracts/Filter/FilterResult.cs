/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Network.Contracts.Filter
{
	/// <summary>
	/// Defines all possible results of a request filter operation.
	/// </summary>
	public enum FilterResult
	{
		/// <summary>
		/// Indicates that a request should be allowed.
		/// </summary>
		Allow,

		/// <summary>
		/// Indicates that a request should be blocked.
		/// </summary>
		Block
	}
}
