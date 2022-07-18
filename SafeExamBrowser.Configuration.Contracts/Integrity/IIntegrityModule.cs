/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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
		/// Attempts to calculate the browser exam key.
		/// </summary>
		bool TryCalculateBrowserExamKey(string configurationKey, string salt, out string browserExamKey);

		/// <summary>
		/// Attempts to verify the code signature.
		/// </summary>
		bool TryVerifyCodeSignature(out bool isValid);
	}
}
