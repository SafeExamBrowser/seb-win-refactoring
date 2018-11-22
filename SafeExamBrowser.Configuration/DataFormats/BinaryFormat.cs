/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Text;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public partial class BinaryFormat : IDataFormat
	{
		private const int PREFIX_LENGTH = 4;

		private IDataCompressor compressor;
		private ILogger logger;

		public BinaryFormat(IDataCompressor compressor, ILogger logger)
		{
			this.compressor = compressor;
			this.logger = logger;
		}

		public bool CanParse(Stream data)
		{
			try
			{
				var longEnough = data.Length > PREFIX_LENGTH;

				if (longEnough)
				{
					var prefix = ParsePrefix(data);
					var success = TryDetermineFormat(prefix, out FormatType format);

					logger.Debug($"'{data}' starting with '{prefix}' does {(success ? string.Empty : "not ")}match the binary format.");

					return success;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the binary format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data.Length / 1000.0} KB data matches the binary format!", e);
			}

			return false;
		}

		public LoadStatus TryParse(Stream data, out Settings settings, string password = null)
		{
			var prefix = ParsePrefix(data);
			var success = TryDetermineFormat(prefix, out FormatType format);

			settings = default(Settings);

			if (success)
			{
				if (compressor.IsCompressed(data))
				{
					data = compressor.Decompress(data);
				}

				data = new SubStream(data, PREFIX_LENGTH, data.Length - PREFIX_LENGTH);

				// TODO: Try to abstract (Parser -> Binary, Xml, ...; DataBlock -> Password, PlainData, ...) once fully implemented!
				switch (format)
				{
					case FormatType.Password:
					case FormatType.PasswordConfigureClient:
						return ParsePassword(data, format, out settings, password);
					case FormatType.PlainData:
						return ParsePlainData(data, out settings);
					case FormatType.PublicKeyHash:
						return ParsePublicKeyHash(data, out settings, password);
					case FormatType.PublicKeyHashSymmetricKey:
						return ParsePublicKeyHashWithSymmetricKey(data, out settings, password);
				}
			}

			logger.Error($"'{data}' starting with '{prefix}' does not match the binary format!");

			return LoadStatus.InvalidData;
		}

		private string ParsePrefix(Stream data)
		{
			var prefixData = new byte[PREFIX_LENGTH];

			if (compressor.IsCompressed(data))
			{
				prefixData = compressor.Peek(data, PREFIX_LENGTH);
			}
			else
			{
				data.Seek(0, SeekOrigin.Begin);
				data.Read(prefixData, 0, PREFIX_LENGTH);
			}

			return Encoding.UTF8.GetString(prefixData);
		}

		private bool TryDetermineFormat(string prefix, out FormatType format)
		{
			format = default(FormatType);

			switch (prefix)
			{
				case "pswd":
					format = FormatType.Password;
					return true;
				case "pwcc":
					format = FormatType.PasswordConfigureClient;
					return true;
				case "plnd":
					format = FormatType.PlainData;
					return true;
				case "pkhs":
					format = FormatType.PublicKeyHash;
					return true;
				case "phsk":
					format = FormatType.PublicKeyHashSymmetricKey;
					return true;
			}

			return false;
		}

		private enum FormatType
		{
			Password = 1,
			PasswordConfigureClient,
			PlainData,
			PublicKeyHash,
			PublicKeyHashSymmetricKey
		}
	}
}
