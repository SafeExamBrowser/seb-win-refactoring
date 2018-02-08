/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class SessionSequenceStartOperation : SessionSequenceOperation
	{
		public SessionSequenceStartOperation(IConfigurationRepository configuration, ILogger logger, IServiceProxy serviceProxy) : base(configuration, logger, serviceProxy)
		{
		}

		public override void Perform()
		{
			// Nothing to do here...
		}

		public override void Repeat()
		{
			StopSession();
		}

		public override void Revert()
		{
			// Nothing to do here...
		}
	}
}
