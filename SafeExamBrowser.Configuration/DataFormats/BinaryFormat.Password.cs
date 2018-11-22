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
using System.Security.Cryptography;
using System.Text;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public partial class BinaryFormat
	{
		private const int BLOCK_SIZE = 16;
		private const int HEADER_SIZE = 2;
		private const int ITERATIONS = 10000;
		private const int KEY_SIZE = 32;
		private const int OPTIONS = 0x1;
		private const int SALT_SIZE = 8;
		private const int VERSION = 0x2;

		private LoadStatus ParsePassword(Stream data, FormatType format, out Settings settings, string password = null)
		{
			settings = default(Settings);

			if (password == null)
			{
				return LoadStatus.PasswordNeeded;
			}

			var (version, options) = ParseHeader(data);

			if (version != VERSION || options != OPTIONS)
			{
				return FailForInvalidPasswordHeader(version, options);
			}

			if (format == FormatType.PasswordConfigureClient)
			{
				// TODO: Shouldn't this not only be done for admin password, and not settings password?!?
				password = GeneratePasswordHash(password);
			}

			var (authenticationKey, encryptionKey) = GenerateKeys(data, password);
			var (originalHmac, computedHmac) = GenerateHmac(data, authenticationKey);

			if (!computedHmac.SequenceEqual(originalHmac))
			{
				return FailForInvalidPasswordHmac();
			}

			using (var plainData = Decrypt(data, encryptionKey, originalHmac.Length))
			{
				return ParsePlainData(plainData, out settings);
			}
		}

		private (int version, int options) ParseHeader(Stream data)
		{
			data.Seek(0, SeekOrigin.Begin);

			var version = data.ReadByte();
			var options = data.ReadByte();

			return (version, options);
		}

		private LoadStatus FailForInvalidPasswordHeader(int version, int options)
		{
			logger.Error($"Invalid encryption header! Expected: [{VERSION},{OPTIONS},...] - Actual: [{version},{options},...]");

			return LoadStatus.InvalidData;
		}

		private string GeneratePasswordHash(string input)
		{
			using (var algorithm = new SHA256Managed())
			{
				var bytes = Encoding.UTF8.GetBytes(input);
				var hash = algorithm.ComputeHash(bytes);
				var @string = String.Join(String.Empty, hash.Select(b => b.ToString("x2")));

				return @string;
			}
		}

		private (byte[] authenticationKey, byte[] encryptionKey) GenerateKeys(Stream data, string password)
		{
			var authenticationSalt = new byte[SALT_SIZE];
			var encryptionSalt = new byte[SALT_SIZE];

			data.Seek(HEADER_SIZE, SeekOrigin.Begin);
			data.Read(encryptionSalt, 0, SALT_SIZE);
			data.Read(authenticationSalt, 0, SALT_SIZE);

			using (var authenticationGenerator = new Rfc2898DeriveBytes(password, authenticationSalt, ITERATIONS))
			using (var encryptionGenerator = new Rfc2898DeriveBytes(password, encryptionSalt, ITERATIONS))
			{
				var authenticationKey = authenticationGenerator.GetBytes(KEY_SIZE);
				var encryptionKey = encryptionGenerator.GetBytes(KEY_SIZE);

				return (authenticationKey, encryptionKey);
			}
		}

		private (byte[] originalHmac, byte[] computedHmac) GenerateHmac(Stream data, byte[] authenticationKey)
		{
			using (var hmac = new HMACSHA256(authenticationKey))
			{
				var originalHmac = new byte[hmac.HashSize / 8];
				var hashStream = new SubStream(data, 0, data.Length - originalHmac.Length);
				var computedHmac = hmac.ComputeHash(hashStream);

				data.Seek(originalHmac.Length, SeekOrigin.End);
				data.Read(originalHmac, 0, originalHmac.Length);

				return (originalHmac, computedHmac);
			}
		}

		private LoadStatus FailForInvalidPasswordHmac()
		{
			logger.Warn($"The authentication failed due to an invalid password or corrupted data!");

			return LoadStatus.PasswordNeeded;
		}

		private Stream Decrypt(Stream data, byte[] encryptionKey, int hmacLength)
		{
			var initializationVector = new byte[BLOCK_SIZE];

			data.Seek(HEADER_SIZE + 2 * SALT_SIZE, SeekOrigin.Begin);
			data.Read(initializationVector, 0, BLOCK_SIZE);

			var decryptedData = new MemoryStream();
			var encryptedData = new SubStream(data, data.Position, data.Length - data.Position - hmacLength);

			using (var aes = new AesManaged { KeySize = KEY_SIZE * 8, BlockSize = BLOCK_SIZE * 8, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
			using (var decryptor = aes.CreateDecryptor(encryptionKey, initializationVector))
			using (var cryptoStream = new CryptoStream(encryptedData, decryptor, CryptoStreamMode.Read))
			{
				cryptoStream.CopyTo(decryptedData);
			}

			return decryptedData;
		}
	}
}
