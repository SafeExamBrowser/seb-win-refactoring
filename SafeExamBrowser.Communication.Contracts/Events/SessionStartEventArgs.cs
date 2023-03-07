/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the session start event fired by the <see cref="Hosts.IServiceHost"/>.
	/// </summary>
	public class SessionStartEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The configuration to be used for the new session.
		/// </summary>
		public ServiceConfiguration Configuration { get; set; }
	}
}
