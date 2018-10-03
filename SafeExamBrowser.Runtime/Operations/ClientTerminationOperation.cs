/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ClientTerminationOperation : ClientOperation
	{
		public new event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public new event StatusChangedEventHandler StatusChanged;

		public ClientTerminationOperation(
			IConfigurationRepository configuration,
			ILogger logger,
			IProcessFactory processFactory,
			IProxyFactory proxyFactory,
			IRuntimeHost runtimeHost,
			int timeout_ms) : base(configuration, logger, processFactory, proxyFactory, runtimeHost, timeout_ms)
		{
		}

		public override OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			var success = true;

			if (ClientProcess != null && !ClientProcess.HasTerminated)
			{
				StatusChanged?.Invoke(TextKey.OperationStatus_StopClient);
				success = TryStopClient();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override void Revert()
		{
			// Nothing to do here...
		}
	}
}
