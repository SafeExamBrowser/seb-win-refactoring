/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Windows.Data
{
	/// <summary>
	/// Defines the user interaction result of an <see cref="IServerFailureDialog"/>.
	/// </summary>
	public class ServerFailureDialogResult
	{
		/// <summary>
		/// Indicates whether the user wants to abort the operation.
		/// </summary>
		public bool Abort { get; set; }

		/// <summary>
		/// Indicates whether the user wants to performa a fallback.
		/// </summary>
		public bool Fallback { get; set; }

		/// <summary>
		/// Indicates whether the user wants to retry the operation.
		/// </summary>
		public bool Retry { get; set; }

		/// <summary>
		/// Indicates whether the user confirmed the dialog or not.
		/// </summary>
		public bool Success { get; set; }
	}
}
