/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Security.Cryptography.X509Certificates;

namespace SafeExamBrowser.Contracts.Configuration.Cryptography
{
	/// <summary>
	/// Provides functionality to load certificates installed on the computer.
	/// </summary>
	public interface ICertificateStore
	{
		/// <summary>
		/// Attempts to retrieve the certificate which matches the specified public key hash value.
		/// Returns <c>true</c> if the certificate was found, otherwise <c>false</c>.
		/// </summary>
		bool TryGetCertificateWith(byte[] keyHash, out X509Certificate2 certificate);
	}
}
