/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class AudioDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Audio.InitialVolumeLevel:
					MapInitialVolumeLevel(settings, value);
					break;
				case Keys.Audio.MuteAudio:
					MapMuteAudio(settings, value);
					break;
				case Keys.Audio.SetInitialVolumeLevel:
					MapSetInitialVolumeLevel(settings, value);
					break;
			}
		}

		private void MapInitialVolumeLevel(AppSettings settings, object value)
		{
			if (value is int volume)
			{
				settings.Audio.InitialVolume = volume;
			}
		}

		private void MapMuteAudio(AppSettings settings, object value)
		{
			if (value is bool mute)
			{
				settings.Audio.MuteAudio = mute;
			}
		}

		private void MapSetInitialVolumeLevel(AppSettings settings, object value)
		{
			if (value is bool initialize)
			{
				settings.Audio.InitializeVolume = initialize;
			}
		}
	}
}
