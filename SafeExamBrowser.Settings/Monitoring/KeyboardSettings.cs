/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Monitoring
{
	/// <summary>
	/// Defines all settings for monitoring keyboard input.
	/// </summary>
	[Serializable]
	public class KeyboardSettings
	{
		/// <summary>
		/// Determines whether the user may use the ALT+ESC shortcut.
		/// </summary>
		public bool AllowAltEsc { get; set; }

		/// <summary>
		/// Determines whether the user may use the ALT+F4 shortcut.
		/// </summary>
		public bool AllowAltF4 { get; set; }

		/// <summary>
		/// Determines whether the user may use the ALT+TAB shortcut.
		/// </summary>
		public bool AllowAltTab { get; set; }

		/// <summary>
		/// Determines whether the user may use the CTRL+ESC shortcut.
		/// </summary>
		public bool AllowCtrlEsc { get; set; }

		/// <summary>
		/// Determines whether the user may use the escape key.
		/// </summary>
		public bool AllowEsc { get; set; }

		/// <summary>
		/// Determines whether the user may use the F1 key.
		/// </summary>
		public bool AllowF1 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F2 key.
		/// </summary>
		public bool AllowF2 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F3 key.
		/// </summary>
		public bool AllowF3 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F4 key.
		/// </summary>
		public bool AllowF4 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F5 key.
		/// </summary>
		public bool AllowF5 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F6 key.
		/// </summary>
		public bool AllowF6 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F7 key.
		/// </summary>
		public bool AllowF7 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F8 key.
		/// </summary>
		public bool AllowF8 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F9 key.
		/// </summary>
		public bool AllowF9 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F10 key.
		/// </summary>
		public bool AllowF10 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F11 key.
		/// </summary>
		public bool AllowF11 { get; set; }

		/// <summary>
		/// Determines whether the user may use the F12 key.
		/// </summary>
		public bool AllowF12 { get; set; }

		/// <summary>
		/// Determines whether the user may use the print screen key.
		/// </summary>
		public bool AllowPrintScreen { get; set; }

		/// <summary>
		/// Determines whether the user may use the system key.
		/// </summary>
		public bool AllowSystemKey { get; set; }
	}
}
