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
	/// Defines the operation result of the execution of an <see cref="IOperation"/> resp. <see cref="IOperationSequence"/>.
	/// </summary>
	public enum OperationResult
	{
		/// <summary>
		/// Indicates that the operation has been aborted due to an expected condition, e.g. as result of a user decision.
		/// </summary>
		Aborted = 1,

		/// <summary>
		/// Indicates that the operation has failed due to an invalid or unexpected condition.
		/// </summary>
		Failed,

		/// <summary>
		/// Indicates that the operation has been executed successfully.
		/// </summary>
		Success
	}
}
