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
	public interface IOperation
	{
		/// <summary>
		/// The progress indicator to be used to show status information to the user. Will be ignored if <c>null</c>.
		/// </summary>
		IProgressIndicator ProgressIndicator { set; }

		/// <summary>
		/// Performs the operation.
		/// </summary>
		OperationResult Perform();

		/// <summary>
		/// Repeats the operation.
		/// </summary>
		OperationResult Repeat();

		/// <summary>
		/// Reverts all changes which were made when executing the operation.
		/// </summary>
		void Revert();
	}
}
