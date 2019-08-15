/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.SystemComponents
{
	public class Audio : ISystemComponent<ISystemAudioControl>
	{
		private readonly object @lock = new object();

		private MMDevice audioDevice;
		private string audioDeviceShortName;
		private List<ISystemAudioControl> controls;
		private ILogger logger;
		private IText text;

		public Audio(ILogger logger, IText text)
		{
			this.controls = new List<ISystemAudioControl>();
			this.logger = logger;
			this.text = text;
		}

		public void Initialize()
		{
			if (TryLoadAudioDevice())
			{
				InitializeAudioDevice();
				InitializeSettings();
			}
			else
			{
				logger.Warn("Could not find an active audio device!");
			}
		}

		public void Register(ISystemAudioControl control)
		{
			lock (@lock)
			{
				controls.Add(control);
			}

			UpdateControls();
		}

		public void Terminate()
		{
			if (audioDevice != default(MMDevice))
			{
				audioDevice.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
				audioDevice.Dispose();
				logger.Info("Stopped monitoring the audio device.");
			}

			foreach (var control in controls)
			{
				control.Close();
			}
		}

		private bool TryLoadAudioDevice()
		{
			using (var enumerator = new MMDeviceEnumerator())
			{
				if (enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
				{
					audioDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
				}
				else
				{
					audioDevice = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).FirstOrDefault();
				}
			}

			return audioDevice != default(MMDevice);
		}

		private void InitializeAudioDevice()
		{
			logger.Info($"Found '{audioDevice}' to be the active audio device.");
			audioDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
			audioDeviceShortName = audioDevice.FriendlyName.Length > 25 ? audioDevice.FriendlyName.Split(' ').First() : audioDevice.FriendlyName;
			logger.Info("Started monitoring the audio device.");
		}

		private void InitializeSettings()
		{
			// TODO: Mute on startup & initial volume!
		}

		private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
		{
			lock (@lock)
			{
				var info = BuildInfoText(data.MasterVolume, data.Muted);

				foreach (var control in controls)
				{
					control.OutputDeviceMuted = data.Muted;
					control.OutputDeviceVolume = data.MasterVolume;
					control.SetInformation(info);
				}
			}
		}

		private void UpdateControls()
		{
			lock (@lock)
			{
				try
				{
					var info = BuildInfoText(audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar, audioDevice.AudioEndpointVolume.Mute);

					foreach (var control in controls)
					{
						if (audioDevice != default(MMDevice))
						{
							control.HasOutputDevice = true;
							control.OutputDeviceMuted = audioDevice.AudioEndpointVolume.Mute;
							control.OutputDeviceName = audioDevice.FriendlyName;
							control.OutputDeviceVolume = audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
							control.SetInformation(info);
						}
						else
						{
							control.HasOutputDevice = false;
							control.SetInformation(text.Get(TextKey.SystemControl_AudioDeviceNotFound));
						}
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to update audio device status!", e);
				}
			}
		}

		private string BuildInfoText(float volume, bool muted)
		{
			var info = text.Get(muted ? TextKey.SystemControl_AudioDeviceInfoMuted : TextKey.SystemControl_AudioDeviceInfo);

			info = info.Replace("%%NAME%%", audioDeviceShortName);
			info = info.Replace("%%VOLUME%%", Convert.ToString(volume * 100));

			return info;
		}
	}
}
