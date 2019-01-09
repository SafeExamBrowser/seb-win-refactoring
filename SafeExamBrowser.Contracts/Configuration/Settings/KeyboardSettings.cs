/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	/// <summary>
	/// Defines all configuration options for the <see cref="Monitoring.IKeyboardInterceptor"/>.
	/// </summary>
	[Serializable]
	public class KeyboardSettings
	{
		/// <summary>
		/// Determines whether the user may use the ALT+TAB shortcut.
		/// </summary>
		public bool AllowAltTab { get; set; }

		/// <summary>
		/// Determines whether the user may use the escape key.
		/// </summary>
		public bool AllowEsc { get; set; }

		/// <summary>
		/// Determines whether the user may use the F5 key.
		/// </summary>
		public bool AllowF5 { get; set; }
	}
}
