/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
		/// Calculates the encrypted value of the app signature key.
		/// </summary>
		string CalculateAppSignatureKey(string connectionToken, string salt);

		/// <summary>
		/// Calculates the hash value of the browser exam key (BEK) for the given URL.
		/// </summary>
		string CalculateBrowserExamKeyHash(string configurationKey, byte[] salt, string url);

		/// <summary>
		/// Calculates the hash value of the configuration key (CK) for the given URL.
		/// </summary>
		string CalculateConfigurationKeyHash(string configurationKey, string url);

		/// <summary>
		/// Specifies that a custom browser exam key (BEK) should be used.
		/// </summary>
		void UseCustomBrowserExamKey(string browserExamKey);
	}
}
