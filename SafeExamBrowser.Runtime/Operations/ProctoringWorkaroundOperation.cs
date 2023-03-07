/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ProctoringWorkaroundOperation : SessionOperation
	{
		private readonly ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public ProctoringWorkaroundOperation(ILogger logger, SessionContext context) : base(context)
		{
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			if (Context.Next.Settings.Proctoring.Enabled && Context.Next.Settings.Security.KioskMode == KioskMode.CreateNewDesktop)
			{
				Context.Next.Settings.Security.KioskMode = KioskMode.DisableExplorerShell;
				logger.Info("Switched kiosk mode to Disable Explorer Shell due to remote proctoring being enabled.");
			}

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			return Perform();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
