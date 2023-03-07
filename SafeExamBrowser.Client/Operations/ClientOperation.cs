/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Client.Operations
{
	/// <summary>
	/// The base implementation to be used for all operations in the client operation sequence.
	/// </summary>
	internal abstract class ClientOperation : IOperation
	{
		protected ClientContext Context { get; private set; }

		public abstract event ActionRequiredEventHandler ActionRequired;
		public abstract event StatusChangedEventHandler StatusChanged;

		public ClientOperation(ClientContext context)
		{
			Context = context;
		}

		public abstract OperationResult Perform();
		public abstract OperationResult Revert();
	}
}
