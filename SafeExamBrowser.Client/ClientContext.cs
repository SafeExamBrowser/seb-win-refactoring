/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client
{
	/// <summary>
	/// Holds all configuration and session data for the client.
	/// </summary>
	internal class ClientContext
	{
		/// <summary>
		/// All activators for shell components.
		/// </summary>
		internal IList<IActivator> Activators { get; }

		/// <summary>
		/// All applications allowed for the current session.
		/// </summary>
		internal IList<IApplication> Applications { get; }

		/// <summary>
		/// The global application configuration.
		/// </summary>
		internal AppConfig AppConfig { get; set; }

		/// <summary>
		/// The browser application.
		/// </summary>
		internal IBrowserApplication Browser { get; set; }

		/// <summary>
		/// The client communication host.
		/// </summary>
		internal IClientHost ClientHost { get; set; }

		/// <summary>
		/// The integrity module.
		/// </summary>
		internal IIntegrityModule IntegrityModule { get; set; }

		/// <summary>
		/// The server proxy to be used if the current session mode is <see cref="SessionMode.Server"/>.
		/// </summary>
		internal IServerProxy Server { get; set; }

		/// <summary>
		/// The identifier of the current session.
		/// </summary>
		internal Guid SessionId { get; set; }

		/// <summary>
		/// The settings for the current session.
		/// </summary>
		internal AppSettings Settings { get; set; }

		internal ClientContext()
		{
			Activators = new List<IActivator>();
			Applications = new List<IApplication>();
		}
	}
}
