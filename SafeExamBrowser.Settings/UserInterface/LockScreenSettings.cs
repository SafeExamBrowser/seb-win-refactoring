/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.UserInterface
{
	/// <summary>
	/// Defines all settings for the lock screen.
	/// </summary>
	[Serializable]
	public class LockScreenSettings
	{
		/// <summary>
		/// The background color as hexadecimal color code.
		/// </summary>
		public string BackgroundColor { get; set; }
	}
}
