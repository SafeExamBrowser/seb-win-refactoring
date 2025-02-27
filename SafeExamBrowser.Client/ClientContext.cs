/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;

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
		internal IList<IApplication<IApplicationWindow>> Applications { get; }

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
		/// The hash algorithm.
		/// </summary>
		internal IHashAlgorithm HashAlgorithm { get; set; }

		/// <summary>
		/// The integrity module.
		/// </summary>
		internal IIntegrityModule IntegrityModule { get; set; }

		/// <summary>
		/// The currently active lock screen, or <c>default</c> if no lock screen is active.
		/// </summary>
		internal ILockScreen LockScreen { get; set; }

		/// <summary>
		/// The message box.
		/// </summary>
		internal IMessageBox MessageBox { get; set; }

		/// <summary>
		/// The proctoring controller to be used if the current session has proctoring enabled.
		/// </summary>
		internal IProctoringController Proctoring { get; set; }

		/// <summary>
		/// The client responsibilities.
		/// </summary>
		internal IResponsibilityCollection<ClientTask> Responsibilities { get; set; }

		/// <summary>
		/// The runtime communication proxy.
		/// </summary>
		internal IRuntimeProxy Runtime { get; set; }

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

		/// <summary>
		/// The user interface factory.
		/// </summary>
		internal IUserInterfaceFactory UserInterfaceFactory { get; set; }

		internal ClientContext()
		{
			Activators = new List<IActivator>();
			Applications = new List<IApplication<IApplicationWindow>>();
		}
	}
}
