/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.IO.Compression;
using SafeExamBrowser.Configuration.Contracts.DataCompression;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataCompression
{
	/// <summary>
	/// Data compression using the GNU-Zip format (see https://en.wikipedia.org/wiki/Gzip).
	/// </summary>
	public class GZipCompressor : IDataCompressor
	{
		private const int ID1 = 0x1F;
		private const int ID2 = 0x8B;
		private const int CM = 8;
		private const int FOOTER_LENGTH = 8;
		private const int HEADER_LENGTH = 10;

		private ILogger logger;

		public GZipCompressor(ILogger logger)
		{
			this.logger = logger;
		}

		public Stream Compress(Stream data)
		{
			var compressed = new MemoryStream();
			var originalSize = data.Length / 1000.0;

			logger.Debug($"Starting compression of '{data}' with {originalSize} KB data...");
			data.Seek(0, SeekOrigin.Begin);

			using (var stream = new GZipStream(compressed, CompressionMode.Compress, true))
			{
				data.CopyTo(stream);
			}

			logger.Debug($"Successfully compressed {originalSize} KB to {compressed.Length / 1000.0} KB data.");

			return compressed;
		}

		public Stream Decompress(Stream data)
		{
			var decompressed = new MemoryStream();
			var originalSize = data.Length / 1000.0;

			logger.Debug($"Starting decompression of '{data}' with {originalSize} KB data...");
			data.Seek(0, SeekOrigin.Begin);

			using (var stream = new GZipStream(data, CompressionMode.Decompress))
			{
				stream.CopyTo(decompressed);
			}

			logger.Debug($"Successfully decompressed {originalSize} KB to {decompressed.Length / 1000.0} KB data.");

			return decompressed;
		}

		/// <remarks>
		/// All gzip-compressed data has a 10-byte header and 8-byte footer. The header starts with two magic numbers (ID1 and ID2) and
		/// the used compression method (CM), which normally denotes the DEFLATE algorithm. See https://tools.ietf.org/html/rfc1952 for
		/// the original data format specification.
		/// </remarks>
		public bool IsCompressed(Stream data)
		{
			try
			{
				var longEnough = data.Length > HEADER_LENGTH + FOOTER_LENGTH;

				data.Seek(0, SeekOrigin.Begin);

				if (longEnough)
				{
					var id1 = data.ReadByte();
					var id2 = data.ReadByte();
					var cm = data.ReadByte();
					var compressed = id1 == ID1 && id2 == ID2 && cm == CM;

					return compressed;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to be a gzip-compressed stream.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to check whether '{data}' with {data?.Length / 1000.0} KB data is a gzip-compressed stream!", e);
			}

			return false;
		}

		public byte[] Peek(Stream data, int count)
		{
			data.Seek(0, SeekOrigin.Begin);

			using (var stream = new GZipStream(data, CompressionMode.Decompress, true))
			using (var decompressed = new MemoryStream())
			{
				var buffer = new byte[count];
				var bytesRead = stream.Read(buffer, 0, buffer.Length);

				decompressed.Write(buffer, 0, bytesRead);

				return decompressed.ToArray();
			}
		}
	}
}
