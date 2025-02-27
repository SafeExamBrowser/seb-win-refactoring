/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class ProctoringDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Proctoring.ForceRaiseHandMessage:
					MapForceRaiseHandMessage(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.CacheSize:
					MapCacheSize(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.ClientId:
					MapClientId(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.ClientSecret:
					MapClientSecret(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.Enabled:
					MapScreenProctoringEnabled(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.GroupId:
					MapGroupId(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.ImageDownscaling:
					MapImageDownscaling(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.ImageFormat:
					MapImageFormat(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.ImageQuantization:
					MapImageQuantization(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.IntervalMaximum:
					MapIntervalMaximum(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.IntervalMinimum:
					MapIntervalMinimum(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.MetaData.CaptureApplicationData:
					MapCaptureApplicationData(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.MetaData.CaptureBrowserData:
					MapCaptureBrowserData(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.MetaData.CaptureWindowTitle:
					MapCaptureWindowTitle(settings, value);
					break;
				case Keys.Proctoring.ScreenProctoring.ServiceUrl:
					MapServiceUrl(settings, value);
					break;
				case Keys.Proctoring.ShowRaiseHand:
					MapShowRaiseHand(settings, value);
					break;
				case Keys.Proctoring.ShowTaskbarNotification:
					MapShowTaskbarNotification(settings, value);
					break;
			}
		}

		private void MapForceRaiseHandMessage(AppSettings settings, object value)
		{
			if (value is bool force)
			{
				settings.Proctoring.ForceRaiseHandMessage = force;
			}
		}

		private void MapCacheSize(AppSettings settings, object value)
		{
			if (value is int size)
			{
				settings.Proctoring.ScreenProctoring.CacheSize = size;
			}
		}

		private void MapCaptureApplicationData(AppSettings settings, object value)
		{
			if (value is bool capture)
			{
				settings.Proctoring.ScreenProctoring.MetaData.CaptureApplicationData = capture;
			}
		}

		private void MapCaptureBrowserData(AppSettings settings, object value)
		{
			if (value is bool capture)
			{
				settings.Proctoring.ScreenProctoring.MetaData.CaptureBrowserData = capture;
			}
		}

		private void MapCaptureWindowTitle(AppSettings settings, object value)
		{
			if (value is bool capture)
			{
				settings.Proctoring.ScreenProctoring.MetaData.CaptureWindowTitle = capture;
			}
		}

		private void MapClientId(AppSettings settings, object value)
		{
			if (value is string clientId)
			{
				settings.Proctoring.ScreenProctoring.ClientId = clientId;
			}
		}

		private void MapClientSecret(AppSettings settings, object value)
		{
			if (value is string secret)
			{
				settings.Proctoring.ScreenProctoring.ClientSecret = secret;
			}
		}

		private void MapGroupId(AppSettings settings, object value)
		{
			if (value is string groupId)
			{
				settings.Proctoring.ScreenProctoring.GroupId = groupId;
			}
		}

		private void MapImageDownscaling(AppSettings settings, object value)
		{
			if (value is double downscaling)
			{
				settings.Proctoring.ScreenProctoring.ImageDownscaling = downscaling;
			}
		}

		private void MapImageFormat(AppSettings settings, object value)
		{
			if (value is string s && Enum.TryParse<ImageFormat>(s, true, out var format))
			{
				settings.Proctoring.ScreenProctoring.ImageFormat = format;
			}
		}

		private void MapImageQuantization(AppSettings settings, object value)
		{
			if (value is int quantization)
			{
				switch (quantization)
				{
					case 0:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.BlackAndWhite1bpp;
						break;
					case 1:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.Grayscale2bpp;
						break;
					case 2:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.Grayscale4bpp;
						break;
					case 3:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.Grayscale8bpp;
						break;
					case 4:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.Color8bpp;
						break;
					case 5:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.Color16bpp;
						break;
					case 6:
						settings.Proctoring.ScreenProctoring.ImageQuantization = ImageQuantization.Color24bpp;
						break;
				}

			}
		}

		private void MapIntervalMaximum(AppSettings settings, object value)
		{
			if (value is int interval)
			{
				settings.Proctoring.ScreenProctoring.IntervalMaximum = interval;
			}
		}

		private void MapIntervalMinimum(AppSettings settings, object value)
		{
			if (value is int interval)
			{
				settings.Proctoring.ScreenProctoring.IntervalMinimum = interval;
			}
		}

		private void MapScreenProctoringEnabled(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Proctoring.ScreenProctoring.Enabled = enabled;
			}
		}

		private void MapServiceUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Proctoring.ScreenProctoring.ServiceUrl = url;
			}
		}

		private void MapShowRaiseHand(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Proctoring.ShowRaiseHandNotification = show;
			}
		}

		private void MapShowTaskbarNotification(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Proctoring.ShowTaskbarNotification = show;
			}
		}
	}
}
