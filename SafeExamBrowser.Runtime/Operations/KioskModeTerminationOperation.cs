/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class KioskModeTerminationOperation : KioskModeOperation, IRepeatableOperation
	{
		public KioskModeTerminationOperation(
			IDesktopFactory desktopFactory,
			IExplorerShell explorerShell,
			ILogger logger,
			IProcessFactory processFactory,
			SessionContext sessionContext) : base(desktopFactory, explorerShell, logger, processFactory, sessionContext)
		{
		}

		public override OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			var newMode = Context.Next.Settings.KioskMode;

			if (ActiveMode == newMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is the same as the currently active, skipping termination...");

				return OperationResult.Success;
			}

			return base.Revert();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
