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
	/// Provides functionality to calculate hash codes of different objects.
	/// </summary>
	public interface IHashAlgorithm
	{
		/// <summary>
		/// Computes a hash code for the given password.
		/// </summary>
		string GenerateHashFor(string password);
	}
}
