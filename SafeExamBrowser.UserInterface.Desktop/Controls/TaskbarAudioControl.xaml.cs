/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class TaskbarAudioControl : UserControl, ISystemAudioControl
	{
		private readonly IText text;
		private bool muted;
		private XamlIconResource MutedIcon;
		private XamlIconResource NoDeviceIcon;

		public event AudioMuteRequestedEventHandler MuteRequested;
		public event AudioVolumeSelectedEventHandler VolumeSelected;

		public TaskbarAudioControl(IText text)
		{
			this.text = text;

			InitializeComponent();
			InitializeAudioControl();
		}

		public bool HasOutputDevice
		{
			set
			{
				Dispatcher.InvokeAsync(() =>
				{
					Button.IsEnabled = value;

					if (!value)
					{
						TaskbarIcon.Content = IconResourceLoader.Load(NoDeviceIcon);
					}
				});
			}
		}

		public bool OutputDeviceMuted
		{
			set
			{
				Dispatcher.InvokeAsync(() =>
				{
					muted = value;

					if (value)
					{
						MuteButton.ToolTip = text.Get(TextKey.SystemControl_AudioDeviceUnmuteTooltip);
						PopupIcon.Content = IconResourceLoader.Load(MutedIcon);
						TaskbarIcon.Content = IconResourceLoader.Load(MutedIcon);
					}
					else
					{
						MuteButton.ToolTip = text.Get(TextKey.SystemControl_AudioDeviceMuteTooltip);
						TaskbarIcon.Content = LoadIcon(Volume.Value / 100);
					}
				});
			}
		}

		public string OutputDeviceName
		{
			set
			{
				Dispatcher.InvokeAsync(() => AudioDeviceName.Text = value);
			}
		}

		public double OutputDeviceVolume
		{
			set
			{
				Dispatcher.InvokeAsync(() =>
				{
					Volume.ValueChanged -= Volume_ValueChanged;
					Volume.Value = Math.Round(value * 100);
					Volume.ValueChanged += Volume_ValueChanged;

					if (!muted)
					{
						PopupIcon.Content = LoadIcon(value);
						TaskbarIcon.Content = LoadIcon(value);
					}
				});
			}
		}

		public void Close()
		{
			Popup.IsOpen = false;
		}

		public void SetInformation(string text)
		{
			Dispatcher.InvokeAsync(() => Button.ToolTip = text);
		}

		private void InitializeAudioControl()
		{
			var originalBrush = Button.Background;

			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = Popup.IsMouseOver));
			MuteButton.Click += (o, args) => MuteRequested?.Invoke(!muted);
			MutedIcon = new XamlIconResource(new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/AudioMuted.xaml"));
			NoDeviceIcon = new XamlIconResource(new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/AudioNoDevice.xaml"));
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = IsMouseOver));
			Volume.ValueChanged += Volume_ValueChanged;

			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				Button.Background = Brushes.LightGray;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
			};
		}

		private void Volume_DragStarted(object sender, DragStartedEventArgs e)
		{
			Volume.ValueChanged -= Volume_ValueChanged;
		}

		private void Volume_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			VolumeSelected?.Invoke(Volume.Value / 100);
			Volume.ValueChanged += Volume_ValueChanged;
		}

		private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			VolumeSelected?.Invoke(Volume.Value / 100);
		}

		private UIElement LoadIcon(double volume)
		{
			var icon = volume > 0.66 ? "100" : (volume > 0.33 ? "66" : "33");
			var uri = new Uri($"pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/Audio_{icon}.xaml");
			var resource = new XamlIconResource(uri);

			return IconResourceLoader.Load(resource);
		}
	}
}
