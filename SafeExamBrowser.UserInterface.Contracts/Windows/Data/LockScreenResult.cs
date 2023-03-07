/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.UserInterface.Contracts.Windows.Data
{
	/// <summary>
	/// Defines the result of a lock screen interaction by the user.
	/// </summary>
	public class LockScreenResult
	{
		/// <summary>
		/// Indicates that the lock screen has been canceled (e.g. via a server instruction).
		/// </summary>
		public bool Canceled { get; set; }

		/// <summary>
		/// The identifier of the option selected by the user, if available.
		/// </summary>
		public Guid? OptionId { get; set; }

		/// <summary>
		/// The password entered by the user.
		/// </summary>
		public string Password { get; set; }
	}
}
