/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Proctoring
{
	/// <summary>
	/// All settings for the screen proctoring.
	/// </summary>
	[Serializable]
	public class ScreenProctoringSettings
	{
		/// <summary>
		/// Determines whether the name of the active application shall be captured and transmitted as part of the image meta data.
		/// </summary>
		public bool CaptureApplicationName { get; set; }

		/// <summary>
		/// Determines whether the URL of the currently opened web page shall be captured and transmitted as part of the image meta data.
		/// </summary>
		public bool CaptureBrowserUrl { get; set; }

		/// <summary>
		/// Determines whether the title of the currently active window shall be captured and transmitted as part of the image meta data.
		/// </summary>
		public bool CaptureWindowTitle { get; set; }

		/// <summary>
		/// The client identifier used for authentication with the screen proctoring service.
		/// </summary>
		public string ClientId { get; set; }

		/// <summary>
		/// The client secret used for authentication with the screen proctoring service.
		/// </summary>
		public string ClientSecret { get; set; }

		/// <summary>
		/// Determines whether the screen proctoring is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// The identifier of the group to which the user belongs.
		/// </summary>
		public string GroupId { get; set; }

		/// <summary>
		/// Defines the factor to be used for downscaling of the screen shots.
		/// </summary>
		public double ImageDownscaling { get; set; }

		/// <summary>
		/// Defines the image format to be used for the screen shots.
		/// </summary>
		public ImageFormat ImageFormat { get; set; }

		/// <summary>
		/// Defines the algorithm to be used for quantization of the screen shots.
		/// </summary>
		public ImageQuantization ImageQuantization { get; set; }

		/// <summary>
		/// The maximum time interval in milliseconds between screen shot transmissions.
		/// </summary>
		public int MaxInterval { get; set; }

		/// <summary>
		/// The minimum time interval in milliseconds between screen shot transmissions.
		/// </summary>
		public int MinInterval { get; set; }

		/// <summary>
		/// The URL of the screen proctoring service.
		/// </summary>
		public string ServiceUrl { get; set; }
	}
}
