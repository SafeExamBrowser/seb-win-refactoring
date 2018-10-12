/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.Communication.Events
{
	/// <summary>
	/// The event arguments used for the client configuration event fired by the <see cref="Hosts.IRuntimeHost"/>.
	/// </summary>
	public class ClientConfigurationEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The configuration to be sent to the client.
		/// </summary>
		public ClientConfiguration ClientConfiguration { get; set; }
	}
}
