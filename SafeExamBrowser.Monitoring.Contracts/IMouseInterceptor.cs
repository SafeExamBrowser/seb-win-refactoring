/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Monitoring.Contracts
{
	/// <summary>
	/// Intercepts all mouse input.
	/// </summary>
	public interface IMouseInterceptor
	{
		/// <summary>
		/// Returns <c>true</c> if the given button should be blocked, otherwise <c>false</c>.
		/// </summary>
		bool Block(MouseButton button, KeyState state);
	}
}
