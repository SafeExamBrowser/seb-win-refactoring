/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts.OperationModel
{
	/// <summary>
	/// A sequence of <see cref="IRepeatableOperation"/> which can be used for repeatable sequential procedures.
	/// 
	/// Exemplary execution order for a sequence initialized with operations A, B, C, D:
	/// 
	/// <see cref="TryRepeat()"/>: A -> B -> C -> D.
	/// </summary>
	public interface IRepeatableOperationSequence : IOperationSequence
	{
		/// <summary>
		/// Tries to repeat the operations of this sequence according to their initialized order. If any operation fails, the already
		/// repeated operations will not be reverted.
		/// </summary>
		OperationResult TryRepeat();
	}
}
