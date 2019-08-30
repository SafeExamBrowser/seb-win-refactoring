/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Shell.Events
{
	/// <summary>
	/// Indicates that the user would like to set the audio volume to the given value, where <c>0.0</c> is the lowest and <c>1.0</c> the highest
	/// possible value.
	/// </summary>
	public delegate void AudioVolumeSelectedEventHandler(double volume);
}
