/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.IO;
using System.Text;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataCompression;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class BinarySerializer : IDataSerializer
	{
		private IDataCompressor compressor;
		private ILogger logger;
		private IPasswordEncryption passwordEncryption;
		private IPublicKeyEncryption publicKeyEncryption;
		private IPublicKeyEncryption symmetricEncryption;
		private IDataSerializer xmlSerializer;

		public BinarySerializer(
			IDataCompressor compressor,
			ILogger logger,
			IPasswordEncryption passwordEncryption,
			IPublicKeyEncryption publicKeyEncryption,
			IPublicKeyEncryption symmetricEncryption,
			IDataSerializer xmlSerializer)
		{
			this.compressor = compressor;
			this.logger = logger;
			this.passwordEncryption = passwordEncryption;
			this.publicKeyEncryption = publicKeyEncryption;
			this.symmetricEncryption = symmetricEncryption;
			this.xmlSerializer = xmlSerializer;
		}

		public bool CanSerialize(FormatType format)
		{
			return format == FormatType.Binary;
		}

		public SerializeResult TrySerialize(IDictionary<string, object> data, EncryptionParameters encryption = null)
		{
			var result = new SerializeResult();

			switch (encryption)
			{
				case PasswordParameters p:
					result = SerializePasswordBlock(data, p);
					break;
				case PublicKeyParameters p:
					result = SerializePublicKeyHashBlock(data, p);
					break;
				default:
					result = SerializePlainDataBlock(data, true);
					break;
			}

			if (result.Status == SaveStatus.Success)
			{
				result.Data = compressor.Compress(result.Data);
			}

			return result;
		}

		private SerializeResult SerializePasswordBlock(IDictionary<string, object> data, PasswordParameters password)
		{
			var result = SerializePlainDataBlock(data);

			if (result.Status == SaveStatus.Success)
			{
				var prefix = password.IsHash ? BinaryBlock.PasswordConfigureClient : BinaryBlock.Password;

				logger.Debug("Attempting to serialize password block...");

				var status = passwordEncryption.Encrypt(result.Data, password.Password, out var encrypted);

				if (status == SaveStatus.Success)
				{
					result.Data = WritePrefix(prefix, encrypted);
				}

				result.Status = status;
			}

			return result;
		}

		private SerializeResult SerializePlainDataBlock(IDictionary<string, object> data, bool writePrefix = false)
		{
			logger.Debug("Attempting to serialize plain data block...");

			var result = xmlSerializer.TrySerialize(data);

			if (result.Status == SaveStatus.Success)
			{
				if (writePrefix)
				{
					result.Data = WritePrefix(BinaryBlock.PlainData, result.Data);
				}

				result.Data = compressor.Compress(result.Data);
			}

			return result;
		}

		private SerializeResult SerializePublicKeyHashBlock(IDictionary<string, object> data, PublicKeyParameters parameters)
		{
			var result = SerializePublicKeyHashInnerBlock(data, parameters);

			if (result.Status == SaveStatus.Success)
			{
				var encryption = parameters.SymmetricEncryption ? symmetricEncryption : publicKeyEncryption;
				var prefix = parameters.SymmetricEncryption ? BinaryBlock.PublicKeySymmetric : BinaryBlock.PublicKey;

				logger.Debug("Attempting to serialize public key hash block...");

				var status = encryption.Encrypt(result.Data, parameters.Certificate, out var encrypted);

				if (status == SaveStatus.Success)
				{
					result.Data = WritePrefix(prefix, encrypted);
				}
			}

			return result;
		}

		private SerializeResult SerializePublicKeyHashInnerBlock(IDictionary<string, object> data, PublicKeyParameters parameters)
		{
			if (parameters.InnerEncryption is PasswordParameters password)
			{
				return SerializePasswordBlock(data, password);
			}

			return SerializePlainDataBlock(data, true);
		}

		private Stream WritePrefix(string prefix, Stream data)
		{
			var prefixBytes = Encoding.UTF8.GetBytes(prefix);
			var stream = new MemoryStream();

			stream.Write(prefixBytes, 0, prefixBytes.Length);

			data.Seek(0, SeekOrigin.Begin);
			data.CopyTo(stream);

			return stream;
		}
	}
}
