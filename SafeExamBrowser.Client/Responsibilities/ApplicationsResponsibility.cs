/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class ApplicationsResponsibility : ClientResponsibility
	{
		public ApplicationsResponsibility(ClientContext context, ILogger logger) : base(context, logger)
		{
		}

		public override void Assume(ClientTask task)
		{
			if (task == ClientTask.AutoStartApplications)
			{
				AutoStart();
			}
		}

		private void AutoStart()
		{
			foreach (var application in Context.Applications)
			{
				if (application.AutoStart)
				{
					Logger.Info($"Auto-starting '{application.Name}'...");
					application.Start();
				}
			}
		}
	}
}
