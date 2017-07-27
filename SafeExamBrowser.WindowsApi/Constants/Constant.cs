/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Constants
{
	internal static class Constant
	{
		/// <summary>
		/// A window has received mouse capture. This event is sent by the system, never by servers.
		/// 
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx.
		/// </summary>
		internal const uint EVENT_SYSTEM_CAPTURESTART = 0x8;

		/// <summary>
		/// The foreground window has changed. The system sends this event even if the foreground window has changed to another window in
		/// the same thread. Server applications never send this event.
		/// For this event, the WinEventProc callback function's hwnd parameter is the handle to the window that is in the foreground, the
		/// idObject parameter is OBJID_WINDOW, and the idChild parameter is CHILDID_SELF.
		/// 
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx.
		/// </summary>
		internal const uint EVENT_SYSTEM_FOREGROUND = 0x3;

		/// <summary>
		/// Minimize all open windows.
		/// </summary>
		internal const int MIN_ALL = 419;

		/// <summary>
		/// The callback function is not mapped into the address space of the process that generates the event. Because the hook function
		/// is called across process boundaries, the system must queue events. Although this method is asynchronous, events are guaranteed
		/// to be in sequential order.
		/// 
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd373640(v=vs.85).aspx.
		/// </summary>
		internal const uint WINEVENT_OUTOFCONTEXT = 0x0;

		/// <summary>
		/// Sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or
		/// when an accelerator keystroke is translated.
		/// 
		/// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms647591(v=vs.85).aspx.
		/// </summary>
		internal const int WM_COMMAND = 0x111;
	}
}
