/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Service.Operations
{
	/// <summary>
	/// The base implementation to be used for all operations in the session operation sequence.
	/// </summary>
	internal abstract class SessionOperation : IOperation
	{
		protected SessionContext Context { get; private set; }

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public SessionOperation(SessionContext sessionContext)
		{
			Context = sessionContext;
		}

		public abstract OperationResult Perform();
		public abstract OperationResult Revert();
	}
}
