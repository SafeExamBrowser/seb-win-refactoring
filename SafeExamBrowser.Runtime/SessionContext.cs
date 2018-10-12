/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime
{
	/// <summary>
	/// Holds all configuration and runtime data required for the session handling.
	/// </summary>
	internal class SessionContext
	{
		/// <summary>
		/// The currently running client process.
		/// </summary>
		public IProcess ClientProcess { get; set; }

		/// <summary>
		/// The communication proxy for the currently running client process.
		/// </summary>
		public IClientProxy ClientProxy { get; set; }

		/// <summary>
		/// The configuration of the currently active session.
		/// </summary>
		public ISessionConfiguration Current { get; set; }

		/// <summary>
		/// The configuration of the next session to be activated.
		/// </summary>
		public ISessionConfiguration Next { get; set; }

		/// <summary>
		/// The path of the configuration file to be used for reconfiguration.
		/// </summary>
		public string ReconfigurationFilePath { get; set; }
	}
}
