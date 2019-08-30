/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// The control of the audio system component.
	/// </summary>
	public interface ISystemAudioControl : ISystemControl
	{
		/// <summary>
		/// Defines whether the computer has an audio output device.
		/// </summary>
		bool HasOutputDevice { set; }

		/// <summary>
		/// Indicates whether the current output device is muted.
		/// </summary>
		bool OutputDeviceMuted { set; }

		/// <summary>
		/// Shows the name of the currently active audio output device.
		/// </summary>
		string OutputDeviceName { set; }

		/// <summary>
		/// Shows the current audio output volume, where <c>0.0</c> is the lowest and <c>1.0</c> the highest possible value.
		/// </summary>
		double OutputDeviceVolume { set; }

		/// <summary>
		/// Event fired when the user requests to mute the current output device.
		/// </summary>
		event AudioMuteRequestedEventHandler MuteRequested;

		/// <summary>
		/// Event fired when the user requests to set the volume of the current output device.
		/// </summary>
		event AudioVolumeSelectedEventHandler VolumeSelected;
	}
}
