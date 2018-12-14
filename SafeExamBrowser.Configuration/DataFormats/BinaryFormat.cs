/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.DataCompression;
using SafeExamBrowser.Contracts.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public partial class BinaryFormat : IDataFormat
	{
		private const int PREFIX_LENGTH = 4;

		private IDataCompressor compressor;
		private IHashAlgorithm hashAlgorithm;
		private IModuleLogger logger;

		public BinaryFormat(IDataCompressor compressor, IHashAlgorithm hashAlgorithm, IModuleLogger logger)
		{
			this.compressor = compressor;
			this.hashAlgorithm = hashAlgorithm;
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

		public ParseResult TryParse(Stream data, PasswordParameters password)
		{
			var prefix = ParsePrefix(data);
			var success = TryDetermineFormat(prefix, out FormatType format);

			if (success)
			{
				if (compressor.IsCompressed(data))
				{
					data = compressor.Decompress(data);
				}

				logger.Debug($"Attempting to parse '{data}' with format '{prefix}'...");
				data = new SubStream(data, PREFIX_LENGTH, data.Length - PREFIX_LENGTH);

				switch (format)
				{
					case FormatType.Password:
					case FormatType.PasswordConfigureClient:
						return ParsePasswordBlock(data, format, password);
					case FormatType.PlainData:
						return ParsePlainDataBlock(data);
					case FormatType.PublicKeyHash:
						return ParsePublicKeyHashBlock(data, password);
					case FormatType.PublicKeyHashWithSymmetricKey:
						return ParsePublicKeyHashWithSymmetricKeyBlock(data, password);
				}
			}

			logger.Error($"'{data}' starting with '{prefix}' does not match the binary format!");

			return new ParseResult { Status = LoadStatus.InvalidData };
		}

		private ParseResult ParsePasswordBlock(Stream data, FormatType format, PasswordParameters password)
		{
			var encryption = new PasswordEncryption(logger.CloneFor(nameof(PasswordEncryption)));
			var encryptionParams = new PasswordParameters();
			var result = new ParseResult();

			if (format == FormatType.PasswordConfigureClient)
			{
				encryptionParams.Password = password.IsHash ? password.Password : hashAlgorithm.GenerateHashFor(password.Password);
				encryptionParams.IsHash = true;
			}
			else
			{
				encryptionParams.Password = password.Password;
				encryptionParams.IsHash = password.IsHash;
			}

			result.Status = encryption.Decrypt(data, encryptionParams.Password, out var decrypted);

			if (result.Status == LoadStatus.Success)
			{
				result = ParsePlainDataBlock(decrypted);
				result.Encryption = encryptionParams;
			}

			return result;
		}

		private ParseResult ParsePlainDataBlock(Stream data)
		{
			var xmlFormat = new XmlFormat(logger.CloneFor(nameof(XmlFormat)));

			if (compressor.IsCompressed(data))
			{
				data = compressor.Decompress(data);
			}

			return xmlFormat.TryParse(data);
		}

		private ParseResult ParsePublicKeyHashBlock(Stream data, PasswordParameters password)
		{
			var encryption = new PublicKeyHashEncryption(logger.CloneFor(nameof(PublicKeyHashEncryption)));
			var result = new ParseResult();

			result.Status = encryption.Decrypt(data, out var decrypted, out var certificate);

			if (result.Status == LoadStatus.Success)
			{
				result = TryParse(decrypted, password);
				result.Encryption = new PublicKeyHashParameters
				{
					Certificate = certificate,
					InnerEncryption = result.Encryption as PasswordParameters,
					SymmetricEncryption = false
				};
			}

			return result;
		}

		private ParseResult ParsePublicKeyHashWithSymmetricKeyBlock(Stream data, PasswordParameters password)
		{
			var passwordEncryption = new PasswordEncryption(logger.CloneFor(nameof(PasswordEncryption)));
			var encryption = new PublicKeyHashWithSymmetricKeyEncryption(logger.CloneFor(nameof(PublicKeyHashWithSymmetricKeyEncryption)), passwordEncryption);
			var result = new ParseResult();

			result.Status = encryption.Decrypt(data, out Stream decrypted, out X509Certificate2 certificate);

			if (result.Status == LoadStatus.Success)
			{
				result = TryParse(decrypted, password);
				result.Encryption = new PublicKeyHashParameters
				{
					Certificate = certificate,
					InnerEncryption = result.Encryption as PasswordParameters,
					SymmetricEncryption = true
				};
			}

			return result;
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
					format = FormatType.PublicKeyHashWithSymmetricKey;
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
			PublicKeyHashWithSymmetricKey
		}
	}
}
