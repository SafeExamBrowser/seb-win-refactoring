/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class VirtualMachineOperation : SessionOperation
	{
		//private IVirtualMachineDetector detector;

		public VirtualMachineOperation(/*IVirtualMachineDetector detector, */SessionContext context) : base(context)
		{
			//this.detector = detector;
		}

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public override OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
