/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings.Applications;

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Provides functionality to create external applications.
	/// </summary>
	public interface IApplicationFactory
	{
		/// <summary>
		/// Attempts to create an application according to the given settings.
		/// </summary>
		FactoryResult TryCreate(WhitelistApplication settings, out IApplication application);
	}
}
