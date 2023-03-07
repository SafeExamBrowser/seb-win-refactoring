/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SafeExamBrowser.Configuration.Contracts.Cryptography;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class HashAlgorithm : IHashAlgorithm
	{
		public string GenerateHashFor(string password)
		{
			using (var algorithm = new SHA256Managed())
			{
				var bytes = Encoding.UTF8.GetBytes(password);
				var hash = algorithm.ComputeHash(bytes);
				var hashString = String.Join(String.Empty, hash.Select(b => b.ToString("x2")));

				return hashString;
			}
		}
	}
}
