/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Browser.Contracts.Filters
{
	/// <summary>
	/// Holds data relevant for filtering requests.
	/// </summary>
	public class Request
	{
		/// <summary>
		/// The full URL of the request.
		/// </summary>
		public string Url { get; set; }
	}
}
