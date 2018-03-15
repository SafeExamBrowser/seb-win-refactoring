/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Communication.Data
{
	/// <summary>
	/// This message is transmitted to the runtime to request that the application be reconfigured.
	/// </summary>
	[Serializable]
	public class ReconfigurationMessage : Message
	{
		/// <summary>
		/// The locator of the new configuration to be used.
		/// </summary>
		public string ConfigurationUrl { get; private set; }

		public ReconfigurationMessage(string url)
		{
			ConfigurationUrl = url;
		}
	}
}
