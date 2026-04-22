/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;

namespace SafeExamBrowser.Monitoring
{
	public class RemoteSessionDetector : IRemoteSessionDetector
	{
		private readonly IIntegrityModule integrityModule;
		private readonly ILogger logger;

		public RemoteSessionDetector(IIntegrityModule integrityModule, ILogger logger)
		{
			this.logger = logger;
			this.integrityModule = integrityModule;
		}

		public bool IsRemoteSession()
		{
			var isRemoteSession = SystemInformation.TerminalServerSession || integrityModule.IsRemoteSession();

			logger.Debug($"Current user session appears {(isRemoteSession ? "" : "not ")}to be a remote session.");

			return isRemoteSession;
		}
	}
}
