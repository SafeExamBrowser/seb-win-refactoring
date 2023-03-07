/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;

namespace SafeExamBrowser.Configuration.Contracts.DataCompression
{
	/// <summary>
	/// Defines the functionality for data compression and decompression.
	/// </summary>
	public interface IDataCompressor
	{
		/// <summary>
		/// Compresses the data from the given stream.
		/// </summary>
		Stream Compress(Stream data);

		/// <summary>
		/// Decompresses the data from the given stream.
		/// </summary>
		Stream Decompress(Stream data);

		/// <summary>
		/// Indicates whether the given stream holds compressed data.
		/// </summary>
		bool IsCompressed(Stream data);

		/// <summary>
		/// Decompresses the specified number of bytes from the beginning of the given stream.
		/// </summary>
		byte[] Peek(Stream data, int count);
	}
}
