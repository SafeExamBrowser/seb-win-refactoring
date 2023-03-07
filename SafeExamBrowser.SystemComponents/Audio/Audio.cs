/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using NAudio.CoreAudioApi;
using SafeExamBrowser.Settings.SystemComponents;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Audio.Events;

namespace SafeExamBrowser.SystemComponents.Audio
{
	public class Audio : IAudio
	{
		private AudioSettings settings;
		private MMDevice audioDevice;
		private string audioDeviceFullName;
		private string audioDeviceShortName;
		private float originalVolume;
		private ILogger logger;

		public string DeviceFullName => audioDeviceFullName ?? string.Empty;
		public string DeviceShortName => audioDeviceShortName ?? string.Empty;
		public bool HasOutputDevice => audioDevice != default(MMDevice);
		public bool OutputMuted => audioDevice?.AudioEndpointVolume.Mute == true;
		public double OutputVolume => audioDevice?.AudioEndpointVolume.MasterVolumeLevelScalar ?? 0;

		public event VolumeChangedEventHandler VolumeChanged;

		public Audio(AudioSettings settings, ILogger logger)
		{
			this.settings = settings;
			this.logger = logger;
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

		public void Mute()
		{
			if (audioDevice != default(MMDevice))
			{
				audioDevice.AudioEndpointVolume.Mute = true;
			}
		}

		public void Unmute()
		{
			if (audioDevice != default(MMDevice))
			{
				audioDevice.AudioEndpointVolume.Mute = false;
			}
		}

		public void SetVolume(double value)
		{
			if (audioDevice != default(MMDevice))
			{
				audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float) value;
			}
		}

		public void Terminate()
		{
			if (audioDevice != default(MMDevice))
			{
				RevertSettings();
				FinalizeAudioDevice();
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
			audioDeviceFullName = audioDevice.FriendlyName;
			audioDeviceShortName = audioDevice.FriendlyName.Length > 25 ? audioDevice.FriendlyName.Split(' ').First() : audioDevice.FriendlyName;
			logger.Info("Started monitoring the audio device.");
		}

		private void FinalizeAudioDevice()
		{
			audioDevice.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
			audioDevice.Dispose();
			logger.Info("Stopped monitoring the audio device.");
		}

		private void InitializeSettings()
		{
			if (settings.InitializeVolume)
			{
				originalVolume = audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
				logger.Info($"Saved original volume of {Math.Round(originalVolume * 100)}%.");
				audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = settings.InitialVolume / 100f;
				logger.Info($"Set initial volume to {settings.InitialVolume}%.");
			}

			if (settings.MuteAudio)
			{
				audioDevice.AudioEndpointVolume.Mute = true;
				logger.Info("Muted audio device.");
			}
		}

		private void RevertSettings()
		{
			if (settings.InitializeVolume)
			{
				audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = originalVolume;
				logger.Info($"Reverted volume to original value of {Math.Round(originalVolume * 100)}%.");
			}

			if (settings.MuteAudio)
			{
				audioDevice.AudioEndpointVolume.Mute = false;
				logger.Info("Unmuted audio device.");
			}
		}

		private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
		{
			logger.Debug($"Volume is set to {data.MasterVolume * 100}%, audio device is {(data.Muted ? "muted" : "not muted")}.");
			VolumeChanged?.Invoke(data.MasterVolume, data.Muted);
		}
	}
}
