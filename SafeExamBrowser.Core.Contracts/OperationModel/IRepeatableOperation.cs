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
	/// Defines an operation which can be executed multiple times as part of an <see cref="IRepeatableOperationSequence"/>.
	/// </summary>
	public interface IRepeatableOperation : IOperation
	{
		/// <summary>
		/// Repeats the operation.
		/// </summary>
		OperationResult Repeat();
	}
}
