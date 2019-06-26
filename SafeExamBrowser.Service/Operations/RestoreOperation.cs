/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.Lockdown;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Operations
{
	internal class RestoreOperation : IOperation
	{
		private readonly IFeatureConfigurationBackup backup;
		private ILogger logger;
		private readonly SessionContext sessionContext;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public RestoreOperation(IFeatureConfigurationBackup backup, ILogger logger, SessionContext sessionContext)
		{
			this.backup = backup;
			this.logger = logger;
			this.sessionContext = sessionContext;
		}

		public OperationResult Perform()
		{
			logger.Info("Starting auto-restore mechanism...");
			sessionContext.AutoRestoreMechanism.Start();

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Stopping auto-restore mechanism...");
			sessionContext.AutoRestoreMechanism.Stop();

			return OperationResult.Success;
		}
	}
}
