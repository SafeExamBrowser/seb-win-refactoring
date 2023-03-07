/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using SafeExamBrowser.Configuration.Contracts.Cryptography;

namespace SafeExamBrowser.Configuration.Contracts.DataFormats
{
	/// <summary>
	/// Provides functionality to parse configuration data with a particular format.
	/// </summary>
	public interface IDataParser
	{
		/// <summary>
		/// Indicates whether the given data complies with the required format.
		/// </summary>
		bool CanParse(Stream data);

		/// <summary>
		/// Tries to parse the given data.
		/// </summary>
		ParseResult TryParse(Stream data, PasswordParameters password = null);
	}
}
