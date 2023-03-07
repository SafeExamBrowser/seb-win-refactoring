/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataCompression;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class BinaryParser : IDataParser
	{
		private const int PREFIX_LENGTH = 4;

		private IDataCompressor compressor;
		private IHashAlgorithm hashAlgorithm;
		private ILogger logger;
		private IPasswordEncryption passwordEncryption;
		private IPublicKeyEncryption publicKeyEncryption;
		private IPublicKeyEncryption publicKeySymmetricEncryption;
		private readonly IDataParser xmlParser;

		public BinaryParser(
			IDataCompressor compressor,
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			IPasswordEncryption passwordEncryption,
			IPublicKeyEncryption publicKeyEncryption,
			IPublicKeyEncryption publicKeySymmetricEncryption,
			IDataParser xmlParser)
		{
			this.compressor = compressor;
			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
			this.passwordEncryption = passwordEncryption;
			this.publicKeyEncryption = publicKeyEncryption;
			this.publicKeySymmetricEncryption = publicKeySymmetricEncryption;
			this.xmlParser = xmlParser;
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

					logger.Debug($"'{data}' starting with '{prefix}' {(isValid ? "matches" : "does not match")} the {FormatType.Binary} format.");

					return isValid;
				}

				logger.Debug($"'{data}' is not long enough ({data.Length} bytes) to match the {FormatType.Binary} format.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to determine whether '{data}' with {data?.Length / 1000.0} KB data matches the {FormatType.Binary} format!", e);
			}

			return false;
		}

		public ParseResult TryParse(Stream data, PasswordParameters password = null)
		{
			var prefix = ReadPrefix(data);
			var isValid = IsValid(prefix);
			var result = new ParseResult { Status = LoadStatus.InvalidData };

			if (isValid)
			{
				data = compressor.IsCompressed(data) ? compressor.Decompress(data) : data;
				data = new SubStream(data, PREFIX_LENGTH, data.Length - PREFIX_LENGTH);

				switch (prefix)
				{
					case BinaryBlock.Password:
					case BinaryBlock.PasswordConfigureClient:
						result = ParsePasswordBlock(data, prefix, password);
						break;
					case BinaryBlock.PlainData:
						result = ParsePlainDataBlock(data);
						break;
					case BinaryBlock.PublicKey:
					case BinaryBlock.PublicKeySymmetric:
						result = ParsePublicKeyBlock(data, prefix, password);
						break;
				}

				result.Format = FormatType.Binary;
			}
			else
			{
				logger.Error($"'{data}' starting with '{prefix}' does not match the {FormatType.Binary} format!");
			}

			return result;
		}

		private ParseResult ParsePasswordBlock(Stream data, string prefix, PasswordParameters password = null)
		{
			var result = new ParseResult();

			if (password != null)
			{
				var parameters = DetermineEncryptionParametersFor(prefix, password);

				logger.Debug($"Attempting to parse password block with prefix '{prefix}'...");
				result.Status = passwordEncryption.Decrypt(data, parameters.Password, out var decrypted);

				if (result.Status == LoadStatus.Success)
				{
					result = ParsePlainDataBlock(decrypted);
					result.Encryption = parameters;
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
			data = compressor.IsCompressed(data) ? compressor.Decompress(data) : data;
			logger.Debug("Attempting to parse plain data block...");

			return xmlParser.TryParse(data);
		}

		private ParseResult ParsePublicKeyBlock(Stream data, string prefix, PasswordParameters password = null)
		{
			var encryption = prefix == BinaryBlock.PublicKey ? publicKeyEncryption : publicKeySymmetricEncryption;
			var result = new ParseResult();

			logger.Debug($"Attempting to parse public key hash block with prefix '{prefix}'...");
			result.Status = encryption.Decrypt(data, out var decrypted, out var certificate);

			if (result.Status == LoadStatus.Success)
			{
				result = TryParse(decrypted, password);
				result.Encryption = new PublicKeyParameters
				{
					Certificate = certificate,
					InnerEncryption = result.Encryption as PasswordParameters,
					SymmetricEncryption = prefix == BinaryBlock.PublicKeySymmetric
				};
			}

			return result;
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
			var prefix = new byte[PREFIX_LENGTH];

			if (compressor.IsCompressed(data))
			{
				prefix = compressor.Peek(data, PREFIX_LENGTH);
			}
			else
			{
				data.Seek(0, SeekOrigin.Begin);
				data.Read(prefix, 0, PREFIX_LENGTH);
			}

			return Encoding.UTF8.GetString(prefix);
		}

		private bool IsValid(string prefix)
		{
			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

			return typeof(BinaryBlock).GetFields(bindingFlags).Any(f => f.GetRawConstantValue() as string == prefix);
		}
	}
}
