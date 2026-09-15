/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class Encryptor
	{
		private const int IV_BYTES = 16;
		private const int MAC_BITS = 128;

		private readonly Lazy<byte[]> encryptionSecret;

		internal Encryptor(ScreenProctoringSettings settings)
		{
			// Security fix (CWE-321): Use PBKDF2 to derive a proper 256-bit AES key from the
			// password instead of directly using the UTF-8 bytes, which may be too short.
			encryptionSecret = new Lazy<byte[]>(() => DeriveKey(settings.EncryptionSecret));
		}

		internal byte[] Decrypt(byte[] data)
		{
			var (iv, encrypted) = Split(data);
			var cipher = new GcmBlockCipher(new AesEngine());
			var key = new KeyParameter(encryptionSecret.Value);
			var parameters = new AeadParameters(key, MAC_BITS, iv);

			cipher.Init(false, parameters);

			var outputSize = cipher.GetOutputSize(encrypted.Length);
			var decrypted = new byte[outputSize];
			var offset = cipher.ProcessBytes(encrypted, 0, encrypted.Length, decrypted, 0);

			cipher.DoFinal(decrypted, offset);

			return decrypted;
		}

		internal byte[] Encrypt(byte[] data)
		{
			var cipher = new GcmBlockCipher(new AesEngine());
			var iv = GenerateInitializationVector();
			var key = new KeyParameter(encryptionSecret.Value);
			var parameters = new AeadParameters(key, MAC_BITS, iv);

			cipher.Init(true, parameters);

			var outputSize = cipher.GetOutputSize(data.Length);
			var encrypted = new byte[outputSize];
			var offset = cipher.ProcessBytes(data, 0, data.Length, encrypted, 0);

			cipher.DoFinal(encrypted, offset);

			return Merge(iv, encrypted);
		}

		private byte[] GenerateInitializationVector()
		{
			// Security fix (CWE-338): Use cryptographically secure RNG instead of System.Random.
			// System.Random is time-seeded and predictable — AES-GCM requires unpredictable IVs.
			var vector = new byte[IV_BYTES];

			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(vector);
			}

			return vector;
		}

		/// <summary>
		/// Security fix (CWE-321): Derives a 256-bit AES key from a password using PBKDF2.
		/// This prevents weak keys when the password is shorter than 32 bytes.
		/// Uses a fixed salt derived from the SEB proctoring context — in production, a
		/// proper random salt should be stored alongside the encrypted data.
		/// </summary>
		private static byte[] DeriveKey(string password)
		{
			const int KEY_BYTES = 32; // 256-bit AES key
			const int ITERATIONS = 100_000;
			var salt = Encoding.UTF8.GetBytes("SEB.ScreenProctoring.v1");

			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS, HashAlgorithmName.SHA256))
			{
				return pbkdf2.GetBytes(KEY_BYTES);
			}
		}

		private byte[] Merge(byte[] iv, byte[] encrypted)
		{
			return iv.Concat(encrypted).ToArray();
		}

		private (byte[] iv, byte[] encrypted) Split(byte[] data)
		{
			var iv = data.Take(IV_BYTES).ToArray();
			var encrypted = data.Skip(IV_BYTES).ToArray();

			return (iv, encrypted);
		}
	}
}
