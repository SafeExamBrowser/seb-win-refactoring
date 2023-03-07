/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.SystemComponents
{
	/// <summary>
	/// Defines all settings for the audio device of the computer.
	/// </summary>
	[Serializable]
	public class AudioSettings
	{
		/// <summary>
		/// Defines whether the audio volume should be initialized to the value of <see cref="InitialVolume"/> during application startup.
		/// </summary>
		public bool InitializeVolume { get; set; }

		/// <summary>
		/// Defines the initial audio volume (from 0 to 100) to be used if <see cref="InitializeVolume"/> is active.
		/// </summary>
		public int InitialVolume { get; set; }

		/// <summary>
		/// Defines whether the audio device should be muted during application startup.
		/// </summary>
		public bool MuteAudio { get; set; }
	}
}
