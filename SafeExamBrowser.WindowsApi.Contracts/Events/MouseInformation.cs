/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Contracts.Events
{
	/// <summary>
	/// The mouse information which can be detected by a mouse hook.
	/// </summary>
	public class MouseInformation
	{
		/// <summary>
		/// Indicates whether the mouse event originates from a touch input by the user.
		/// </summary>
		public bool IsTouch { get; set; }

		/// <summary>
		/// The X coordinate of the current mouse position.
		/// </summary>
		public int X { get; set; }

		/// <summary>
		/// The Y coordinate of the current mouse position.
		/// </summary>
		public int Y { get; set; }
	}
}
