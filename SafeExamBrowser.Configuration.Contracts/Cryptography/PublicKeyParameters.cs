/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Security.Cryptography.X509Certificates;

namespace SafeExamBrowser.Configuration.Contracts.Cryptography
{
	/// <summary>
	/// Holds all parameters for data encryption by certificate.
	/// </summary>
	public class PublicKeyParameters : EncryptionParameters
	{
		/// <summary>
		/// The certificate holding the public key used for encryption.
		/// </summary>
		public X509Certificate2 Certificate { get; set; }

		/// <summary>
		/// The encryption parameters of the inner data, if available.
		/// </summary>
		public PasswordParameters InnerEncryption { get; set; }

		/// <summary>
		/// Determines the usage of symmetric vs. asymmetric encryption.
		/// </summary>
		public bool SymmetricEncryption { get; set; }
	}
}
