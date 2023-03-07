/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Logging.Contracts
{
	/// <summary>
	/// Defines raw text data as content element of the application log.
	/// </summary>
	public interface ILogText : ILogContent
	{
		/// <summary>
		/// The raw text to be appended to the application log.
		/// </summary>
		string Text { get; }
	}
}
