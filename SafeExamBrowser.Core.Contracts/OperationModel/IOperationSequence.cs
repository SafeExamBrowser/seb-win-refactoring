/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Core.Contracts.OperationModel
{
	/// <summary>
	/// A sequence of <see cref="IOperation"/> which can be used for sequential procedures, e.g. the initialization &amp; finalization of
	/// an application component. Each operation will be executed failsafe, i.e. the return value will indicate whether a procedure
	/// completed successfully or not.
	/// 
	/// Exemplary execution order for a sequence initialized with operations A, B, C, D:
	/// 
	/// <see cref="TryPerform"/>: A -> B -> C -> D.
	/// <see cref="TryRevert"/>: D -> C -> B -> A.
	/// </summary>
	public interface IOperationSequence
	{
		/// <summary>
		/// Event fired when an operation requires user interaction.
		/// </summary>
		event ActionRequiredEventHandler ActionRequired;

		/// <summary>
		/// Event fired when the progress of the sequence has changed.
		/// </summary>
		event ProgressChangedEventHandler ProgressChanged;

		/// <summary>
		/// Event fired when the status of an operation has changed.
		/// </summary>
		event StatusChangedEventHandler StatusChanged;

		/// <summary>
		/// Tries to perform the operations of this sequence according to their initialized order. If any operation fails, the already
		/// performed operations will be reverted.
		/// </summary>
		OperationResult TryPerform();

		/// <summary>
		/// Tries to revert the operations of this sequence in reversion of their initialized order. The reversion of all operations will
		/// continue, even if one or multiple operations fail to revert successfully.
		/// </summary>
		OperationResult TryRevert();
	}
}
