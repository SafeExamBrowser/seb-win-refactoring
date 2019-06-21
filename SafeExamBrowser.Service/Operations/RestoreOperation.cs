/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Operations
{
	internal class RestoreOperation : IOperation
	{
		private ILogger logger;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public RestoreOperation(ILogger logger)
		{
			this.logger = logger;
		}

		public OperationResult Perform()
		{
			// TODO: Must not delay startup! If restore does not succeed on first attempt, try again in separate thread!
			//         -> Ensure session cannot be started until values are restored or alike!

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
