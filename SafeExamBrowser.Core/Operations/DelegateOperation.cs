/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Core.Operations
{
	/// <summary>
	/// A generic operation to allow for the (inline) definition of an operation via delegates. Useful if implementing a complete
	/// <see cref="IOperation"/> would be an unnecessary overhead.
	/// </summary>
	public class DelegateOperation : IRepeatableOperation
	{
		private Action perform;
		private Action repeat;
		private Action revert;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public DelegateOperation(Action perform, Action repeat = null, Action revert = null)
		{
			this.perform = perform;
			this.repeat = repeat;
			this.revert = revert;
		}

		public OperationResult Perform()
		{
			perform?.Invoke();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			repeat?.Invoke();

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			revert?.Invoke();

			return OperationResult.Success;
		}
	}
}
