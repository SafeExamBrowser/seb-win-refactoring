/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Browser.Data
{
	/// <summary>
	/// The data resulting from a JavaScript expression evaluation.
	/// </summary>
	public class JavascriptResult
	{
		/// <summary>
		/// Indicates if the JavaScript was evaluated successfully or not.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The error message, in case of an unsuccessful evaluation of the JavaScript expression.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The data item returned by the JavaScript expression.
		/// </summary>
		public object Result { get; set; }
	}
}
