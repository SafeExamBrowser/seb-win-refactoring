/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Communication.Contracts.Data
{
	/// <summary>
	/// This message is transmitted to the runtime to request that the application be reconfigured.
	/// </summary>
	[Serializable]
	public class ReconfigurationMessage : Message
	{
		/// <summary>
		/// The full path of the configuration file to be used.
		/// </summary>
		public string ConfigurationPath { get; private set; }

		/// <summary>
		/// The original URL from where the configuration file was downloaded.
		/// </summary>
		public string ResourceUrl { get; set; }

		public ReconfigurationMessage(string path, string url)
		{
			ConfigurationPath = path;
			ResourceUrl = url;
		}
	}
}
