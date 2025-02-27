/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
		/// The maximum size of the locally cached data per session in megabytes.
		/// </summary>
		public int CacheSize { get; set; }

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
		/// The encryption secret to be used for the locally cached data.
		/// </summary>
		public string EncryptionSecret { get; set; }

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
		public int IntervalMaximum { get; set; }

		/// <summary>
		/// The minimum time interval in milliseconds between screen shot transmissions.
		/// </summary>
		public int IntervalMinimum { get; set; }

		/// <summary>
		/// All settings related to the metadata capturing of the screen proctoring.
		/// </summary>
		public MetaDataSettings MetaData { get; set; }

		/// <summary>
		/// The URL of the screen proctoring service.
		/// </summary>
		public string ServiceUrl { get; set; }

		public ScreenProctoringSettings()
		{
			MetaData = new MetaDataSettings();
		}
	}
}
