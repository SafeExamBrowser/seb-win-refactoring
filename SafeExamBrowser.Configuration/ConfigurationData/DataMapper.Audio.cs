/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapInitialVolumeLevel(ApplicationSettings settings, object value)
		{
			if (value is int volume)
			{
				settings.Audio.InitialVolume = volume;
			}
		}

		private void MapMuteAudio(ApplicationSettings settings, object value)
		{
			if (value is bool mute)
			{
				settings.Audio.MuteAudio = mute;
			}
		}

		private void MapSetInitialVolumeLevel(ApplicationSettings settings, object value)
		{
			if (value is bool initialize)
			{
				settings.Audio.InitializeVolume = initialize;
			}
		}
	}
}
