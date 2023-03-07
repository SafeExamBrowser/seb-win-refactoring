/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See http://www.pinvoke.net/default.aspx/Enums/HookType.html.
	/// </remarks>
	internal enum HookType
	{
		/// <summary>
		/// Installs a hook procedure that records input messages posted to the system message queue. This hook is useful for recording
		/// macros. For more information, see the JournalRecordProc hook procedure.
		/// </summary>
		WH_JOURNALRECORD = 0,

		/// <summary>
		/// Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD hook procedure. For more information,
		/// see the JournalPlaybackProc hook procedure.
		/// </summary>
		WH_JOURNALPLAYBACK = 1,

		/// <summary>
		/// Installs a hook procedure that monitors keystroke messages. For more information, see the KeyboardProc hook procedure.
		/// </summary>
		WH_KEYBOARD = 2,

		/// <summary>
		/// Installs a hook procedure that monitors messages posted to a message queue. For more information, see the GetMsgProc hook
		/// procedure.
		/// </summary>
		WH_GETMESSAGE = 3,

		/// <summary>
		/// Installs a hook procedure that monitors messages before the system sends them to the destination window procedure. For more
		/// information, see the CallWndProc hook procedure.
		/// </summary>
		WH_CALLWNDPROC = 4,

		/// <summary>
		/// Installs a hook procedure that receives notifications useful to a CBT application. For more information, see the CBTProc hook
		/// procedure.
		/// </summary>
		WH_CBT = 5,

		/// <summary>
		/// Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box, message box,  menu,
		/// or scroll bar. The hook procedure monitors these messages for all applications in the same desktop as the calling thread. For
		/// more information, see the SysMsgProc hook procedure.
		/// </summary>
		WH_SYSMSGFILTER = 6,

		/// <summary>
		/// Installs a hook procedure that monitors mouse messages. For more information, see the MouseProc hook procedure.
		/// </summary>
		WH_MOUSE = 7,

		WH_HARDWARE = 8,

		/// <summary>
		/// Installs a hook procedure useful for debugging other hook procedures. For more information, see the DebugProc hook procedure.
		/// </summary>
		WH_DEBUG = 9,

		/// <summary>
		/// Installs a hook procedure that receives notifications useful to shell applications. For more information, see the ShellProc
		/// hook procedure.
		/// </summary>
		WH_SHELL = 10,

		/// <summary>
		/// Installs a hook procedure that will be called when the application's foreground thread is about to become idle. This hook is
		/// useful for performing low priority tasks during idle time. For more information, see the ForegroundIdleProc hook procedure. 
		/// </summary>
		WH_FOREGROUNDIDLE = 11,

		/// <summary>
		/// Installs a hook procedure that monitors messages after they have been processed by the destination window procedure. For more
		/// information, see the CallWndRetProc hook procedure.
		/// </summary>
		WH_CALLWNDPROCRET = 12,

		/// <summary>
		/// Installs a hook procedure that monitors low-level keyboard input events. For more information, see the LowLevelKeyboardProc
		/// hook procedure.
		/// </summary>
		WH_KEYBOARD_LL = 13,

		/// <summary>
		/// Installs a hook procedure that monitors low-level mouse input events. For more information, see the LowLevelMouseProc hook
		/// procedure.
		/// </summary>
		WH_MOUSE_LL = 14
	}
}
