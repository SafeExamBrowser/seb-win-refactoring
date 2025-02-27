/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.System
{
	/// <summary>
	/// Defines all settings related to functionality of the operating system.
	/// </summary>
	[Serializable]
	public class SystemSettings
	{
		/// <summary>
		/// Determines whether the system will remain always on or not (i.e. potentially entering sleep mode or standby). This does not prevent the
		/// display(s) from turning off, see <see cref="Monitoring.DisplaySettings.AlwaysOn"/>.
		/// </summary>
		public bool AlwaysOn { get; set; }
	}
}
