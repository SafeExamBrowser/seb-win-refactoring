/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class KioskModeTerminationOperation : KioskModeOperation, IRepeatableOperation
	{
		private IConfigurationRepository configuration;
		private KioskMode kioskMode;
		private ILogger logger;

		public KioskModeTerminationOperation(
			IConfigurationRepository configuration,
			IDesktopFactory desktopFactory,
			IExplorerShell explorerShell,
			ILogger logger,
			IProcessFactory processFactory) : base(configuration, desktopFactory, explorerShell, logger, processFactory)
		{
			this.configuration = configuration;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			kioskMode = configuration.CurrentSettings.KioskMode;

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			var oldMode = kioskMode;
			var newMode = configuration.CurrentSettings.KioskMode;

			if (newMode == oldMode)
			{
				logger.Info($"New kiosk mode '{newMode}' is equal to the currently active '{oldMode}', skipping termination...");

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
