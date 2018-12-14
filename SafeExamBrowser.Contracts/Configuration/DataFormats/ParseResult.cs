/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Cryptography;

namespace SafeExamBrowser.Contracts.Configuration.DataFormats
{
	/// <summary>
	/// Defines the result of a data parsing operation by an <see cref="IDataFormat"/>.
	/// </summary>
	public class ParseResult
	{
		/// <summary>
		/// The encryption parameters which were used to decrypt the data, or <c>null</c> if it was not encrypted.
		/// </summary>
		public EncryptionParameters Encryption { get; set; }

		/// <summary>
		/// The original format of the data.
		/// </summary>
		public Format Format { get; set; }

		/// <summary>
		/// The parsed settings data. Might be <c>null</c> or in an undefinable state, depending on <see cref="Status"/>.
		/// </summary>
		public IDictionary<string, object> RawData { get; set; }

		/// <summary>
		/// The status result of a parsing operation.
		/// </summary>
		public LoadStatus Status { get; set; }
	}
}
