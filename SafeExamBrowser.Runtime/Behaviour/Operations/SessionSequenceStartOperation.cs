/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class SessionSequenceStartOperation : SessionSequenceOperation
	{
		public SessionSequenceStartOperation(
			IClientProxy client,
			IConfigurationRepository configuration,
			ILogger logger,
			IProcessFactory processFactory,
			IRuntimeHost runtimeHost,
			IServiceProxy service) : base(client, configuration, logger, processFactory, runtimeHost, service)
		{
		}

		public override OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			return StopSession();
		}

		public override void Revert()
		{
			// Nothing to do here...
		}
	}
}
