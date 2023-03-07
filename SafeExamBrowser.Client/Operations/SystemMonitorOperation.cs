/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System;

namespace SafeExamBrowser.Client.Operations
{
	internal class SystemMonitorOperation : ClientOperation
	{
		private readonly ILogger logger;
		private readonly ISystemMonitor systemMonitor;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public SystemMonitorOperation(ClientContext context, ISystemMonitor systemMonitor, ILogger logger) : base(context)
		{
			this.logger = logger;
			this.systemMonitor = systemMonitor;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing system events...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeSystemEvents);

			systemMonitor.Start();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info("Finalizing system events...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeSystemEvents);

			systemMonitor.Stop();

			return OperationResult.Success;
		}
	}
}
