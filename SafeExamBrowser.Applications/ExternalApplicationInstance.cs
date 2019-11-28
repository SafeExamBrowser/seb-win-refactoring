/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Applications
{
	internal class ExternalApplicationInstance
	{
		private IconResource icon;
		private InstanceIdentifier id;
		private ILogger logger;
		private IProcess process;

		internal ExternalApplicationInstance(IconResource icon, InstanceIdentifier id, ILogger logger, IProcess process)
		{
			this.icon = icon;
			this.id = id;
			this.logger = logger;
			this.process = process;
		}

		internal void Initialize()
		{
			process.Terminated += Process_Terminated;
		}

		internal void Terminate()
		{
			const int MAX_ATTEMPTS = 5;
			const int TIMEOUT_MS = 500;

			var terminated = process.HasTerminated;

			if (!terminated)
			{
				process.Terminated -= Process_Terminated;

				for (var attempt = 0; attempt < MAX_ATTEMPTS && !terminated; attempt++)
				{
					terminated = process.TryClose(TIMEOUT_MS);
				}

				for (var attempt = 0; attempt < MAX_ATTEMPTS && !terminated; attempt++)
				{
					terminated = process.TryKill(TIMEOUT_MS);
				}

				if (terminated)
				{
					logger.Info("Successfully terminated application instance.");
				}
				else
				{
					logger.Warn("Failed to terminate application instance!");
				}
			}
		}

		private void Process_Terminated(int exitCode)
		{
			logger.Info($"Application instance has terminated with exit code {exitCode}.");
			// TODO: Terminated?.Invoke(Id); -> Remove from application!
		}
	}
}
