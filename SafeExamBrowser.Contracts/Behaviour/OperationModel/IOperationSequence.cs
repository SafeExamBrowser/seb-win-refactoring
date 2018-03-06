/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Behaviour.OperationModel
{
	/// <summary>
	/// A sequence of <see cref="IOperation"/>s which can be used for sequential procedures, e.g. the initialization &amp; finalization of an
	/// application component. Each operation will be executed failsafe, i.e. the return value will indicate whether a procedure completed
	/// successfully or not.
	/// 
	/// The execution order of the individual operations (for an exemplary sequence initialized with operations A, B, C, D) is as follows:
	/// 
	/// <see cref="TryPerform"/>: The operations will be performed according to their initialized order (A -> B -> C -> D).
	/// <see cref="TryRepeat"/>: The operations will be repeated according to their initialized order (A -> B -> C -> D).
	/// <see cref="TryRevert"/>: The operations will be reverted according to the reversed initial order (D -> C -> B -> A).
	/// </summary>
	public interface IOperationSequence
	{
		/// <summary>
		/// The progress indicator to be used when executing an operation. Will be ignored if <c>null</c>.
		/// </summary>
		IProgressIndicator ProgressIndicator { set; }

		/// <summary>
		/// Tries to perform the operations of this sequence.
		/// </summary>
		OperationResult TryPerform();

		/// <summary>
		/// Tries to repeat the operations of this sequence.
		/// </summary>
		OperationResult TryRepeat();

		/// <summary>
		/// Tries to revert the operations of this sequence. Returns <c>true</c> if all operations were reverted without errors,
		/// otherwise <c>false</c>.
		/// </summary>
		bool TryRevert();
	}
}
