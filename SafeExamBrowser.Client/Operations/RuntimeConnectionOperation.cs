/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.Operations
{
	internal class RuntimeConnectionOperation : IOperation
	{
		private bool connected;
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
			StatusChanged?.Invoke(TextKey.ProgressIndicator_InitializeRuntimeConnection);

			connected = runtime.Connect(token);

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

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Closing runtime connection...");
			StatusChanged?.Invoke(TextKey.ProgressIndicator_CloseRuntimeConnection);

			if (connected)
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
			}
		}
	}
}
