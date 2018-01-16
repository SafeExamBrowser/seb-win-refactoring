/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.Contracts.Behaviour
{
	public interface IStartupController
	{
		/// <summary>
		/// Tries to initialize the application according to the given queue of operations.
		/// Returns <c>true</c> if the initialization was successful, <c>false</c> otherwise.
		/// </summary>
		bool TryInitializeApplication(Queue<IOperation> operations);
	}
}
