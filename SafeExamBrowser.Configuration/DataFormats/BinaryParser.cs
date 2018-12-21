/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SafeExamBrowser.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.DataCompression;
using SafeExamBrowser.Contracts.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class BinaryParser : IDataParser
	{
		private const int PREFIX_LENGTH = 4;

		private IDataCompressor compressor;
		private IHashAlgorithm hashAlgorithm;
		private IModuleLogger logger;

		public BinaryParser(IDataCompressor compressor, IHashAlgorithm hashAlgorithm, IModuleLogger logger)
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
					var prefix = ReadPrefix(data);
					var isValid = IsValid(prefix);

					logger.Debug($"'{data}' starting with '{prefix}' does {(isValid ? string.Empty : "not ")}match the {FormatType.Binary} format.");

					return isValid;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the {FormatType.Binary} format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data.Length / 1000.0} KB data matches the {FormatType.Binary} format!", e);
			}

			return false;
		}

		public ParseResult TryParse(Stream data, PasswordParameters password = null)
		{
			var prefix = ReadPrefix(data);
			var isValid = IsValid(prefix);

			if (isValid)
			{
				data = compressor.IsCompressed(data) ? compressor.Decompress(data) : data;
				data = new SubStream(data, PREFIX_LENGTH, data.Length - PREFIX_LENGTH);

				switch (prefix)
				{
					case BinaryBlock.Password:
					case BinaryBlock.PasswordConfigureClient:
						return ParsePasswordBlock(data, prefix, password);
					case BinaryBlock.PlainData:
						return ParsePlainDataBlock(data);
					case BinaryBlock.PublicKeyHash:
					case BinaryBlock.PublicKeyHashWithSymmetricKey:
						return ParsePublicKeyHashBlock(data, prefix, password);
				}
			}

			logger.Error($"'{data}' starting with '{prefix}' does not match the {FormatType.Binary} format!");

			return new ParseResult { Status = LoadStatus.InvalidData };
		}

		private ParseResult ParsePasswordBlock(Stream data, string prefix, PasswordParameters password = null)
		{
			var encryption = new PasswordEncryption(logger.CloneFor(nameof(PasswordEncryption)));
			var result = new ParseResult();

			if (password != null)
			{
				var encryptionParameters = DetermineEncryptionParametersFor(prefix, password);

				logger.Debug($"Attempting to parse password block with prefix '{prefix}'...");
				result.Status = encryption.Decrypt(data, encryptionParameters.Password, out var decrypted);

				if (result.Status == LoadStatus.Success)
				{
					result = ParsePlainDataBlock(decrypted);
					result.Encryption = encryptionParameters;
				}
			}
			else
			{
				result.Status = LoadStatus.PasswordNeeded;
			}

			return result;
		}

		private ParseResult ParsePlainDataBlock(Stream data)
		{
			var xmlFormat = new XmlParser(logger.CloneFor(nameof(XmlParser)));

			data = compressor.IsCompressed(data) ? compressor.Decompress(data) : data;
			logger.Debug("Attempting to parse plain data block...");

			var result = xmlFormat.TryParse(data);
			result.Format = FormatType.Binary;

			return result;
		}

		private ParseResult ParsePublicKeyHashBlock(Stream data, string prefix, PasswordParameters password = null)
		{
			var encryption = DetermineEncryptionForPublicKeyHashBlock(prefix);
			var result = new ParseResult();

			logger.Debug($"Attempting to parse public key hash block with prefix '{prefix}'...");
			result.Status = encryption.Decrypt(data, out var decrypted, out var certificate);

			if (result.Status == LoadStatus.Success)
			{
				result = TryParse(decrypted, password);
				result.Encryption = new PublicKeyHashParameters
				{
					Certificate = certificate,
					InnerEncryption = result.Encryption as PasswordParameters,
					SymmetricEncryption = prefix == BinaryBlock.PublicKeyHashWithSymmetricKey
				};
			}

			return result;
		}

		private PublicKeyHashEncryption DetermineEncryptionForPublicKeyHashBlock(string prefix)
		{
			var passwordEncryption = new PasswordEncryption(logger.CloneFor(nameof(PasswordEncryption)));

			if (prefix == BinaryBlock.PublicKeyHash)
			{
				return new PublicKeyHashEncryption(logger.CloneFor(nameof(PublicKeyHashEncryption)));
			}

			return new PublicKeyHashWithSymmetricKeyEncryption(logger.CloneFor(nameof(PublicKeyHashWithSymmetricKeyEncryption)), passwordEncryption);
		}

		private PasswordParameters DetermineEncryptionParametersFor(string prefix, PasswordParameters password)
		{
			var parameters = new PasswordParameters();

			if (prefix == BinaryBlock.PasswordConfigureClient)
			{
				parameters.Password = password.IsHash ? password.Password : hashAlgorithm.GenerateHashFor(password.Password);
				parameters.IsHash = true;
			}
			else
			{
				parameters.Password = password.Password;
				parameters.IsHash = password.IsHash;
			}

			return parameters;
		}

		private string ReadPrefix(Stream data)
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

		private bool IsValid(string prefix)
		{
			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

			return typeof(BinaryBlock).GetFields(bindingFlags).Any(f => f.GetRawConstantValue() as string == prefix);
		}
	}
}
