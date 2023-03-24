/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.ActionCenter
{
	internal partial class AudioControl : UserControl, ISystemControl
	{
		private readonly IAudio audio;
		private readonly IText text;
		private bool muted;
		private IconResource MutedIcon;
		private IconResource NoDeviceIcon;

		internal AudioControl(IAudio audio, IText text)
		{
			this.audio = audio;
			this.text = text;

			InitializeComponent();
			InitializeAudioControl();
		}

		public void Close()
		{
			Popup.IsOpen = false;
		}

		private void InitializeAudioControl()
		{
			var originalBrush = Grid.Background;

			audio.VolumeChanged += Audio_VolumeChanged;
			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			var lastOpenedBySpacePress = false;
			Button.PreviewKeyDown += (o, args) =>
			{
				// For some reason, the popup immediately closes again if opened by a Space Bar key event - as a mitigation,
				// we record the space bar event and leave the popup open for at least 3 seconds.
				if (args.Key == System.Windows.Input.Key.Space)
				{
					lastOpenedBySpacePress = true;
				}
			};
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() =>
			{
				if (Popup.IsOpen && lastOpenedBySpacePress)
				{
					return;
				}
				Popup.IsOpen = Popup.IsMouseOver;
			}));
			MuteButton.Click += MuteButton_Click;
			MutedIcon = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/Audio_Muted.xaml") };
			NoDeviceIcon = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/Audio_Light_NoDevice.xaml") };
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() =>
			{
				if (Popup.IsOpen && lastOpenedBySpacePress)
				{
					return;
				}
				Popup.IsOpen = IsMouseOver;
			}));
			Popup.Opened += (o, args) => Grid.Background = Brushes.Gray;
			Popup.Closed += (o, args) =>
			{
				Grid.Background = originalBrush;
				lastOpenedBySpacePress = false;
			};
			Volume.ValueChanged += Volume_ValueChanged;

			if (audio.HasOutputDevice)
			{
				AudioDeviceName.Text = audio.DeviceFullName;
				Button.IsEnabled = true;
				UpdateVolume(audio.OutputVolume, audio.OutputMuted);
			}
			else
			{
				AudioDeviceName.Text = text.Get(TextKey.SystemControl_AudioDeviceNotFound);
				Button.IsEnabled = false;
				Button.ToolTip = text.Get(TextKey.SystemControl_AudioDeviceNotFound);
				ButtonIcon.Content = IconResourceLoader.Load(NoDeviceIcon);
			}
		}

		private void Audio_VolumeChanged(double volume, bool muted)
		{
			Dispatcher.InvokeAsync(() => UpdateVolume(volume, muted));
		}

		private void MuteButton_Click(object sender, RoutedEventArgs e)
		{
			if (muted)
			{
				audio.Unmute();
			}
			else
			{
				audio.Mute();
			}
		}

		private void Volume_DragStarted(object sender, DragStartedEventArgs e)
		{
			Volume.ValueChanged -= Volume_ValueChanged;
		}

		private void Volume_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			audio.SetVolume(Volume.Value / 100);
			Volume.ValueChanged += Volume_ValueChanged;
		}

		private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			audio.SetVolume(Volume.Value / 100);
		}

		private void UpdateVolume(double volume, bool muted)
		{
			var info = BuildInfoText(volume, muted);

			this.muted = muted;

			Button.ToolTip = info;
			Text.Text = info;
			Volume.ValueChanged -= Volume_ValueChanged;
			Volume.Value = Math.Round(volume * 100);
			Volume.ValueChanged += Volume_ValueChanged;

			AutomationProperties.SetName(Button, info);

			if (muted)
			{
				var tooltip = text.Get(TextKey.SystemControl_AudioDeviceUnmuteTooltip);

				MuteButton.ToolTip = tooltip;
				ButtonIcon.Content = IconResourceLoader.Load(MutedIcon);
				PopupIcon.Content = IconResourceLoader.Load(MutedIcon);

				AutomationProperties.SetName(MuteButton, tooltip);
			}
			else
			{
				var tooltip = text.Get(TextKey.SystemControl_AudioDeviceMuteTooltip);

				MuteButton.ToolTip = tooltip;
				ButtonIcon.Content = LoadIcon(volume);
				PopupIcon.Content = LoadIcon(volume);

				AutomationProperties.SetName(MuteButton, tooltip);
			}
		}

		private string BuildInfoText(double volume, bool muted)
		{
			var info = text.Get(muted ? TextKey.SystemControl_AudioDeviceInfoMuted : TextKey.SystemControl_AudioDeviceInfo);

			info = info.Replace("%%NAME%%", audio.DeviceShortName);
			info = info.Replace("%%VOLUME%%", Convert.ToString(Math.Round(volume * 100)));

			return info;
		}

		private UIElement LoadIcon(double volume)
		{
			var icon = volume > 0.66 ? "100" : (volume > 0.33 ? "66" : "33");
			var uri = new Uri($"pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/Audio_Light_{icon}.xaml");
			var resource = new XamlIconResource { Uri = uri };

			return IconResourceLoader.Load(resource);
		}
	}
}
