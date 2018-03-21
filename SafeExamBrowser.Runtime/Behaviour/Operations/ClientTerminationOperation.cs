/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ClientTerminationOperation : ClientOperation
	{
		public ClientTerminationOperation(
			IConfigurationRepository configuration,
			ILogger logger,
			IProcessFactory processFactory,
			IProxyFactory proxyFactory,
			IRuntimeHost runtimeHost) : base(configuration, logger, processFactory, proxyFactory, runtimeHost)
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
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopClient, true);
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
