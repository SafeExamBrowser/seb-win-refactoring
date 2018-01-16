/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Monitoring
{
	public interface IKeyboardInterceptor
	{
		/// <summary>
		/// Returns <c>true</c> if the given key should be blocked, otherwise <c>false</c>. The key code corresponds to a Win32 Virtual-Key.
		/// </summary>
		bool Block(int keyCode, KeyModifier modifier, KeyState state);
	}
}
