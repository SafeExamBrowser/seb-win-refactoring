/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.Operations
{
	internal class InitializationOperation : IOperation
	{
		private ILogger logger;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public InitializationOperation(ILogger logger)
		{
			this.logger = logger;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing client application...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeClient);

			ConfigureSecurityProtocols();

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private void ConfigureSecurityProtocols()
		{
			// Enables the security protocols specified below for all web requests which are made during application runtime.
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
		}
	}
}
