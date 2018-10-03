/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel.Events;

namespace SafeExamBrowser.Contracts.Core.OperationModel
{
	/// <summary>
	/// Defines an operation which will be executed as part of an <see cref="IOperationSequence"/>.
	/// </summary>
	public interface IOperation
	{
		/// <summary>
		/// Event fired when the operation requires user interaction.
		/// </summary>
		event ActionRequiredEventHandler ActionRequired;

		/// <summary>
		/// Event fired when the status of the operation has changed.
		/// </summary>
		event StatusChangedEventHandler StatusChanged;

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
