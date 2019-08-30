/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class RuntimeConnectionOperation : IOperation
	{
		private ILogger logger;
		private IRuntimeProxy runtime;
		private Guid token;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public RuntimeConnectionOperation(ILogger logger, IRuntimeProxy runtime, Guid token)
		{
			this.logger = logger;
			this.runtime = runtime;
			this.token = token;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing runtime connection...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeRuntimeConnection);

			var connected = runtime.Connect(token);

			if (connected)
			{
				logger.Info("Successfully connected to the runtime.");
			}
			else
			{
				logger.Error("Failed to connect to the runtime. Aborting startup...");
			}

			return connected ? OperationResult.Success : OperationResult.Failed;
		}

		public OperationResult Revert()
		{
			logger.Info("Closing runtime connection...");
			StatusChanged?.Invoke(TextKey.OperationStatus_CloseRuntimeConnection);

			if (runtime.IsConnected)
			{
				var success = runtime.Disconnect();

				if (success)
				{
					logger.Info("Successfully disconnected from the runtime.");
				}
				else
				{
					logger.Error("Failed to disconnect from the runtime!");
				}

				return success ? OperationResult.Success : OperationResult.Failed;
			}

			return OperationResult.Success;
		}
	}
}
