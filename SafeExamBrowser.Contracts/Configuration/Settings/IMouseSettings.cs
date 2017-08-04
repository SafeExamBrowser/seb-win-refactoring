/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	public interface IMouseSettings
	{
		/// <summary>
		/// Determines whether the user may use the middle mouse button.
		/// </summary>
		bool AllowMiddleButton { get; }

		/// <summary>
		/// Determines whether the user may use the right mouse button.
		/// </summary>
		bool AllowRightButton { get; }
	}
}
