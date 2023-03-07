/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Configuration.Contracts.Cryptography;

namespace SafeExamBrowser.Configuration.Contracts.DataFormats
{
	/// <summary>
	/// Provides functionality to serialize configuration data to a particular format.
	/// </summary>
	public interface IDataSerializer
	{
		/// <summary>
		/// Indicates whether data can be serialized to the given format.
		/// </summary>
		bool CanSerialize(FormatType format);

		/// <summary>
		/// Tries to serialize the given data.
		/// </summary>
		SerializeResult TrySerialize(IDictionary<string, object> data, EncryptionParameters encryption = null);
	}
}
