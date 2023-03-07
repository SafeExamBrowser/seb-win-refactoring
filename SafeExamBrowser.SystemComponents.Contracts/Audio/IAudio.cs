/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.SystemComponents.Contracts.Audio.Events;

namespace SafeExamBrowser.SystemComponents.Contracts.Audio
{
	/// <summary>
	/// Defines the functionality of the audio system component.
	/// </summary>
	public interface IAudio : ISystemComponent
	{
		/// <summary>
		/// The full name of the audio device, or an empty string if not available.
		/// </summary>
		string DeviceFullName { get; }

		/// <summary>
		/// The short audio device name, or an empty string if not available.
		/// </summary>
		string DeviceShortName { get; }

		/// <summary>
		/// Indicates whether an audio output device is available.
		/// </summary>
		bool HasOutputDevice { get; }

		/// <summary>
		/// Indicates whether the audio output is currently muted.
		/// </summary>
		bool OutputMuted { get; }

		/// <summary>
		/// The current audio output volume.
		/// </summary>
		double OutputVolume { get; }

		/// <summary>
		/// Fired when the volume of the audio device has changed.
		/// </summary>
		event VolumeChangedEventHandler VolumeChanged;

		/// <summary>
		/// Mutes the currently active audio device.
		/// </summary>
		void Mute();

		/// <summary>
		/// Unmutes the currently active audio device.
		/// </summary>
		void Unmute();

		/// <summary>
		/// Sets the volume of the currently active audio device to the given value.
		/// </summary>
		void SetVolume(double value);
	}
}
