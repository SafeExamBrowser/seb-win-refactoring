/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace SafeExamBrowser.WindowsApi.Types
{
	/// <remarks>
	/// See http://www.pinvoke.net/default.aspx/Structures/KBDLLHOOKSTRUCT.html.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct KBDLLHOOKSTRUCT
	{
		/// <summary>
		/// A virtual-key code. The code must be a value in the range 1 to 254. 
		/// </summary>
		internal uint KeyCode;

		/// <summary>
		/// A hardware scan code for the key. 
		/// </summary>
		internal uint ScanCode;

		/// <summary>
		/// The extended-key flag, event-injected flags, context code, and transition-state flag. This member is specified as follows. An
		/// application can use the following values to test the keystroke flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the
		/// event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected
		/// from a process running at lower integrity level.
		/// </summary>
		internal KBDLLHOOKSTRUCTFlags Flags;

		/// <summary>
		/// The time stamp for this message, equivalent to what <c>GetMessageTime</c> would return for this message.
		/// </summary>
		internal uint Time;

		/// <summary>
		/// Additional information associated with the message. 
		/// </summary>
		internal IntPtr DwExtraInfo;
	}
}
