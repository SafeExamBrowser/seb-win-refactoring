/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Configuration.Contracts.Integrity
{
	/// <summary>
	/// Provides functionality related to application integrity.
	/// </summary>
	public interface IIntegrityModule
	{
		/// <summary>
		/// Caches the specified session for later integrity verification.
		/// </summary>
		void CacheSession(string configurationKey, string startUrl);

		/// <summary>
		/// Removes the specified session from the integrity verification cache.
		/// </summary>
		void ClearSession(string configurationKey, string startUrl);

		/// <summary>
		/// Attempts to calculate the app signature key.
		/// </summary>
		bool TryCalculateAppSignatureKey(string connectionToken, string salt, out string appSignatureKey);

		/// <summary>
		/// Attempts to calculate the browser exam key.
		/// </summary>
		bool TryCalculateBrowserExamKey(string configurationKey, string salt, out string browserExamKey);

		/// <summary>
		/// Attempts to verify the code signature.
		/// </summary>
		bool TryVerifyCodeSignature(out bool isValid);

		/// <summary>
		/// Attempts to verify the integrity for the specified session.
		/// </summary>
		bool TryVerifySessionIntegrity(string configurationKey, string startUrl, out bool isValid);
	}
}
