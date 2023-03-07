/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Types
{
	/// <remarks>
	/// See http://www.pinvoke.net/default.aspx/Structures/KBDLLHOOKSTRUCT.html.
	/// </remarks>
	internal enum KBDLLHOOKSTRUCTFlags
	{
		/// <summary>
		/// Test the extended-key flag. 
		/// </summary>
		LLKHF_EXTENDED = 0x01,

		/// <summary>
		/// Test the event-injected (from any process) flag.
		/// </summary>
		LLKHF_INJECTED = 0x10,

		/// <summary>
		/// Test the context code. 
		/// </summary>
		LLKHF_ALTDOWN = 0x20,

		/// <summary>
		/// Test the transition-state flag. 
		/// </summary>
		LLKHF_UP = 0x80
	}
}
