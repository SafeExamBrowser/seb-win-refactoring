/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-stickykeys#members
	/// </remarks>
	[Flags]
	internal enum StickyKeysFlags : int
	{
		/// <summary>
		/// If this flag is set, the StickyKeys feature is on.
		/// </summary>
		ON = 0x1,

		/// <summary>
		/// If this flag is set, the StickyKeys feature is available.
		/// </summary>
		AVAILABLE = 0x2,

		/// <summary>
		/// If this flag is set, the user can turn the StickyKeys feature on and off by pressing the SHIFT key five times.
		/// </summary>
		HOTKEYACTIVE = 0x4,

		/// <summary>
		/// A confirmation dialog appears when the StickyKeys feature is activated by using the hot key.
		/// </summary>
		CONFIRMHOTKEY = 0x8,

		/// <summary>
		/// If this flag is set, the system plays a siren sound when the user turns the StickyKeys feature on or off by using the hot key.
		/// </summary>
		HOTKEYSOUND = 0x10,

		/// <summary>
		/// A visual indicator should be displayed when the StickyKeys feature is on.
		/// </summary>
		INDICATOR = 0x20,

		/// <summary>
		/// If this flag is set, the system plays a sound when the user latches, locks, or releases modifier keys using the StickyKeys feature.
		/// </summary>
		AUDIBLEFEEDBACK = 0x40,

		/// <summary>
		/// If this flag is set, pressing a modifier key twice in a row locks down the key until the user presses it a third time.
		/// </summary>
		TRISTATE = 0x80,

		/// <summary>
		/// If this flag is set, releasing a modifier key that has been pressed in combination with any other key turns off the StickyKeys feature. 
		/// </summary>
		TWOKEYSOFF = 0x100,
	}
}
