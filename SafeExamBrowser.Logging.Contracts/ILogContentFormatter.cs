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
	/// Defines a formatter to be used to unify the look of and information rendered in an application log.
	/// </summary>
	public interface ILogContentFormatter
	{
		/// <summary>
		/// Formats the given log content and returns it as a <c>string</c>.
		/// </summary>
		string Format(ILogContent content);
	}
}
