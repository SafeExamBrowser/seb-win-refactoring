/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Lockdown.Contracts
{
	/// <summary>
	/// Provides functionality to update and enforce the system configuration.
	/// </summary>
	public interface ISystemConfigurationUpdate
	{
		/// <summary>
		/// Executes the update synchronously.
		/// </summary>
		void Execute();

		/// <summary>
		/// Executes the udpate asynchronously.
		/// </summary>
		void ExecuteAsync();
	}
}
