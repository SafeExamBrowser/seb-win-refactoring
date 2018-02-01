/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.Contracts.Behaviour.Operations
{
	public interface IOperationSequence
	{
		/// <summary>
		/// Tries to perform the given sequence of operations. Returns <c>true</c> if the procedure was successful, <c>false</c> otherwise.
		/// </summary>
		bool TryPerform(Queue<IOperation> operations);

		/// <summary>
		/// Tries to repeat all operations of this sequence. Returns <c>true</c> if the procedure was successful, <c>false</c> otherwise.
		/// </summary>
		bool TryRepeat();

		/// <summary>
		/// Tries to revert all operations of this sequence. Returns <c>true</c> if the procedure was successful, <c>false</c> otherwise.
		/// </summary>
		bool TryRevert();
	}
}
