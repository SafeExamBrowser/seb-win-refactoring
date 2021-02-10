/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class ProctoringOperation : ClientOperation
	{
		private readonly ILogger logger;
		private readonly IProctoringController controller;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ProctoringOperation(ClientContext context, ILogger logger, IProctoringController controller) : base(context)
		{
			this.controller = controller;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
