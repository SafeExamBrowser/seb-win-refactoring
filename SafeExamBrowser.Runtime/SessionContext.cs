/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime
{
	/// <summary>
	/// Holds all configuration and runtime data required for the session handling.
	/// </summary>
	internal class SessionContext
	{
		/// <summary>
		/// The currently active <see cref="KioskMode"/>.
		/// </summary>
		internal KioskMode? ActiveMode { get; set; }

		/// <summary>
		/// The currently running client process.
		/// </summary>
		internal IProcess ClientProcess { get; set; }

		/// <summary>
		/// The communication proxy for the currently running client process.
		/// </summary>
		internal IClientProxy ClientProxy { get; set; }

		/// <summary>
		/// The configuration of the currently active session.
		/// </summary>
		internal SessionConfiguration Current { get; set; }

		/// <summary>
		/// The new desktop, if <see cref="KioskMode.CreateNewDesktop"/> is currently active.
		/// </summary>
		internal IDesktop NewDesktop { get; set; }

		/// <summary>
		/// The configuration of the next session to be activated.
		/// </summary>
		internal SessionConfiguration Next { get; set; }

		/// <summary>
		/// The original desktop, if <see cref="KioskMode.CreateNewDesktop"/> is currently active.
		/// </summary>
		internal IDesktop OriginalDesktop { get; set; }

		/// <summary>
		/// The path of the configuration file to be used for reconfiguration.
		/// </summary>
		internal string ReconfigurationFilePath { get; set; }
	}
}
