/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class RuntimeConnectionOperation : IOperation
	{
		private bool connected;
		private ILogger logger;
		private IRuntimeProxy runtime;
		private Guid token;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public RuntimeConnectionOperation(ILogger logger, IRuntimeProxy runtime, Guid token)
		{
			this.logger = logger;
			this.runtime = runtime;
			this.token = token;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing runtime connection...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeRuntimeConnection);

			try
			{
				connected = runtime.Connect(token);
			}
			catch (Exception e)
			{
				logger.Error("An unexpected error occurred while trying to connect to the runtime!", e);
			}

			if (!connected)
			{
				logger.Error("Failed to connect to the runtime. Aborting startup...");

				return OperationResult.Failed;
			}

			logger.Info("Successfully connected to the runtime.");

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Closing runtime connection...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_CloseRuntimeConnection);

			if (connected)
			{
				try
				{
					runtime.Disconnect();
				}
				catch (Exception e)
				{
					logger.Error("Failed to disconnect from runtime host!", e);
				}
			}
		}
	}
}
