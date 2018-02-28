/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Behaviour.Operations
{
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
