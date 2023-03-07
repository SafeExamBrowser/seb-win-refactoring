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
	/// Defines an option for the user to select on the <see cref="ILockScreen"/>.
	/// </summary>
	public class LockScreenOption
	{
		/// <summary>
		/// The unique identifier for this option.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The text to be displayed to the user.
		/// </summary>
		public string Text { get; set; }

		public LockScreenOption()
		{
			Id = Guid.NewGuid();
		}
	}
}
