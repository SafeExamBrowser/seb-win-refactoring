/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
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
		/// Tries to initialize the application. Returns <c>true</c> if the initialization was successful,
		/// <c>false</c> otherwise. All operations performed during the startup procedure will be registered
		/// to the given <c>out</c> parameter.
		/// </summary>
		bool TryInitializeApplication(out Stack<IOperation> operations);
	}
}
