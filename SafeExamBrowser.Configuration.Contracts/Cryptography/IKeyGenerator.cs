/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Configuration.Contracts.Cryptography
{
	/// <summary>
	/// Provides funcionality to calculate keys for integrity checks.
	/// </summary>
	public interface IKeyGenerator
	{
		/// <summary>
		/// Calculates the hash value of the browser exam key (BEK) for the given URL.
		/// </summary>
		string CalculateBrowserExamKeyHash(string url);

		/// <summary>
		/// Calculates the hash value of the configuration key (CK) for the given URL.
		/// </summary>
		string CalculateConfigurationKeyHash(string url);
	}
}
