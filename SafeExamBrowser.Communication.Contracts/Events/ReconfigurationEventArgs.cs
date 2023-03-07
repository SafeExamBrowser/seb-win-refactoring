/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the reconfiguration event fired by the <see cref="Hosts.IRuntimeHost"/>.
	/// </summary>
	public class ReconfigurationEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The full path to the configuration file to be used for reconfiguration.
		/// </summary>
		public string ConfigurationPath { get; set; }

		/// <summary>
		/// The original URL from where the configuration file was downloaded.
		/// </summary>
		public string ResourceUrl { get; set; }
	}
}
