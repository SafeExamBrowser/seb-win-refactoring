using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

//
//  SEBProtectionController.cs
//  SafeExamBrowser
//
//  SafeExamBrowser
//
//  Copyright (c) 2010-2020 Daniel R. Schneider, 
//  ETH Zurich, IT Services,
//  based on the original idea of Safe Exam Browser
//  by Stefan Schneider, University of Giessen
//  Project concept: Thomas Piendl, Daniel R. Schneider,
//  Dirk Bauer, Kai Reuter, Tobias Halbherr, Karsten Burger, Marco Lehre,
//  Brigitte Schmucki, Oliver Rahs. French localization: Nicolas Dunand
//
//  ``The contents of this file are subject to the Mozilla Public License
//  Version 1.1 (the "License"); you may not use this file except in
//  compliance with the License. You may obtain a copy of the License at
//  http://www.mozilla.org/MPL/
//
//  Software distributed under the License is distributed on an "AS IS"
//  basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//  License for the specific language governing rights and limitations
//  under the License.
//
//  The Original Code is Safe Exam Browser for Windows.
//
//  The Initial Developer of the Original Code is Daniel R. Schneider.
//  Portions created by Daniel R. Schneider
//  are Copyright (c) 2010-2020 Daniel R. Schneider, 
//  ETH Zurich, IT Services, 
//  based on the original idea of Safe Exam Browser
//  by Stefan Schneider, University of Giessen. All Rights Reserved.
//
//  Contributor(s): ______________________________________.
//

namespace SebWindowsConfig.Utilities
{
	public class SEBProtectionController
	{
		const string DLL_NAME =
#if X86
		"seb_x86.dll";
#else
		"seb_x64.dll";
#endif

		// Prefix
		private const int PREFIX_LENGTH = 4;
		private const string PUBLIC_KEY_HASH_MODE = "pkhs";
		private const string PASSWORD_MODE = "pswd";
		private const string PLAIN_DATA_MODE = "plnd";
		private const string PASSWORD_CONFIGURING_CLIENT_MODE = "pwcc";

		// Public key
		private const int PUBLIC_KEY_HASH_LENGTH = 20;

		// RNCryptor non-secret payload (header)
		// First byte: Data format version. Currently 2.
		// Second byte: Options, bit 0 - uses password (so currently 1).
		private static readonly byte[] RNCRYPTOR_HEADER = new byte[] { 0x02, 0x01 };

		enum EncryptionT
		{
			pkhs,
			pswd,
			plnd,
			pwcc,
			unknown
		};

		/// ------------------------------------------------------------------------------------------
		/// <summary>
		///  Get array of certificate references and the according names from both certificate stores.
		/// </summary>
		/// ------------------------------------------------------------------------------------------
		public static ArrayList GetCertificatesAndNames(ref ArrayList certificateNames)
		{
			ArrayList certificates = new ArrayList();

			// First search the Personal (standard) certificate store for the current user
			X509Store store = new X509Store(StoreLocation.CurrentUser);
			certificates = GetCertificatesAndNamesFromStore(ref certificateNames, store);

			// Also search the store for Trusted Users
			ArrayList certificateNamesTrustedUsers = new ArrayList();
			store = new X509Store(StoreName.TrustedPeople);
			certificates.AddRange(GetCertificatesAndNamesFromStore(ref certificateNamesTrustedUsers, store));
			certificateNames.AddRange(certificateNamesTrustedUsers);

			// Also search the Personal store for the local machine
			ArrayList certificateNamesLocalMachine = new ArrayList();
			store = new X509Store(StoreLocation.LocalMachine);
			certificates.AddRange(GetCertificatesAndNamesFromStore(ref certificateNamesLocalMachine, store));
			certificateNames.AddRange(certificateNamesLocalMachine);

			return certificates;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Helper method: Get array of certificate references and the according names from the passed certificate store.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static ArrayList GetCertificatesAndNamesFromStore(ref ArrayList certificateNames, X509Store store)
		{
			ArrayList certificates = new ArrayList();

			store.Open(OpenFlags.ReadOnly);

			foreach (X509Certificate2 x509Certificate in store.Certificates)
			{
				if (x509Certificate.HasPrivateKey)
				{
					certificates.Add(x509Certificate);
					if (!String.IsNullOrWhiteSpace(x509Certificate.FriendlyName))
						certificateNames.Add(x509Certificate.FriendlyName);
					else if (!String.IsNullOrWhiteSpace(x509Certificate.SerialNumber))
						certificateNames.Add(x509Certificate.SerialNumber);
				}
			}

			//Close the store.
			store.Close();
			return certificates;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Get array of CA certificate references and the according names from the certificate store.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static ArrayList GetSSLCertificatesAndNames(ref ArrayList certificateNames)
		{
			ArrayList certificates = new ArrayList();

			X509Store store = new X509Store(StoreName.CertificateAuthority);
			store.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection certsCollection = store.Certificates.Find(X509FindType.FindByKeyUsage, (X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment), false);
			store = new X509Store(StoreName.AddressBook);
			store.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection certsCollection2 = store.Certificates.Find(X509FindType.FindByKeyUsage, (X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment), false);
			certsCollection.AddRange(certsCollection2);

			foreach (X509Certificate2 x509Certificate in certsCollection)
			{
				certificates.Add(x509Certificate);
				certificateNames.Add(Parse(x509Certificate.Subject, "CN")?.FirstOrDefault() ?? x509Certificate.FriendlyName);
			}

			//Close the store.
			store.Close();
			return certificates;
		}


		/// <summary>
		/// Recursively searches the supplied AD string for all groups.
		/// </summary>
		/// <param name="data">The string returned from AD to parse for a group.</param>
		/// <param name="delimiter">The string to use as the seperator for the data. ex. ","</param>
		/// <returns>null if no groups were found -OR- data is null or empty.</returns>
		public static List<string> Parse(string data, string delimiter)
		{
			if (data == null) return null;
			if (!delimiter.EndsWith("=")) delimiter = delimiter + "=";
			if (!data.Contains(delimiter)) return null;
			//base case
			var result = new List<string>();
			int start = data.IndexOf(delimiter) + delimiter.Length;
			int length = data.IndexOf(',', start) - start;
			if (length == 0) return null; //the group is empty
			if (length > 0)
			{
				result.Add(data.Substring(start, length));
				//only need to recurse when the comma was found, because there could be more groups
				var rec = Parse(data.Substring(start + length), delimiter);
				if (rec != null) result.AddRange(rec); //can't pass null into AddRange() :(
			}
			else //no comma found after current group so just use the whole remaining string
			{
				result.Add(data.Substring(start));
			}
			return result;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Get certificate from both stores.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static X509Certificate2 GetCertificateFromStore(byte[] publicKeyHash)
		{
			X509Certificate2 sebCertificate = null;

			// First search the Personal certificate store for the current user
			X509Store store = new X509Store(StoreLocation.CurrentUser);
			sebCertificate = GetCertificateFromPassedStore(publicKeyHash, store);

			// If the certificate wasn't found there, search the store for Trusted People
			if (sebCertificate == null)
			{
				store = new X509Store(StoreName.TrustedPeople);
				sebCertificate = GetCertificateFromPassedStore(publicKeyHash, store);
			}

			// If the certificate wasn't found there, search the Personal store for the local machine
			if (sebCertificate == null)
			{
				store = new X509Store(StoreLocation.LocalMachine);
				sebCertificate = GetCertificateFromPassedStore(publicKeyHash, store);
			}

			return sebCertificate;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Helper method: Search passed store for certificate with passed public key hash.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static X509Certificate2 GetCertificateFromPassedStore(byte[] publicKeyHash, X509Store store)
		{
			X509Certificate2 sebCertificate = null;

			store.Open(OpenFlags.ReadOnly);

			foreach (X509Certificate2 x509Certificate in store.Certificates)
			{
				byte[] publicKeyRawData = x509Certificate.PublicKey.EncodedKeyValue.RawData;
				SHA1 sha = new SHA1CryptoServiceProvider();
				byte[] certificateHash = sha.ComputeHash(publicKeyRawData);

				//certificateName = x509Certificate.Subject;
				if (certificateHash.SequenceEqual(publicKeyHash))
				{
					sebCertificate = x509Certificate;
					break;
				}
			}

			//Close the store.
			store.Close();

			return sebCertificate;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Store certificate into the Windows Certificate Store.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static void StoreCertificateIntoStore(byte[] certificateData)
		{
			X509Store store = null;

			// Save the certificate into the Personal store
			try
			{
				store = new X509Store(StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadWrite);
			}
			catch (Exception storeOpenException)
			{
				Logger.AddError("The X509 store in Windows Certificate Store could not be opened: ", null, storeOpenException, storeOpenException.Message);
			}

			if (store != null)
			{
				StoreCertificateIntoPassedStore(certificateData, store);
			}

			// In addition try to save the certificate into the Trusted People store for the Local Machine
			// This will only work if SEB is run in an administrator account
			try
			{
				store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadWrite);
			}
			catch (Exception storeOpenException)
			{
				Logger.AddError("The X509 Trusted People store in Windows Certificate Store for Local Machine could not be opened: ", null, storeOpenException, storeOpenException.Message);
				return;
			}

			StoreCertificateIntoPassedStore(certificateData, store);

		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Store certificate into the passed, already opened store.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static void StoreCertificateIntoPassedStore(byte[] certificateData, X509Store store)
		{
			X509Certificate2 x509 = new X509Certificate2();


			try
			{

				x509.Import(certificateData, SEBClientInfo.DEFAULT_KEY, X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet);
			}
			catch (Exception certImportException)
			{
				Logger.AddError("The identity data could not be imported into the X509 certificate store.", null, certImportException, certImportException.Message);
				store.Close();
				return;
			}

			try
			{
				store.Add(x509);
			}
			catch (Exception certAddingException)
			{
				Logger.AddError("The identity could not be added to the Windows Certificate Store", null, certAddingException, certAddingException.Message);
				store.Close();
				return;
			}
			Logger.AddInformation("The identity was successfully added to the Windows Certificate Store");
			store.Close();
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		///  Get the public key hash for the certificate from the store.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] GetPublicKeyHashFromCertificate(X509Certificate2 certificateRef)
		{
			//string certificateHash;

			//Create new X509 store from the local certificate store.
			//X509Store store = new X509Store(StoreLocation.CurrentUser);
			//store.Open(OpenFlags.ReadOnly);

			byte[] publicKeyRawData = certificateRef.PublicKey.EncodedKeyValue.RawData;
			SHA1 sha = new SHA1CryptoServiceProvider();
			byte[] certificateHash = sha.ComputeHash(publicKeyRawData);

			//Close the store.
			//store.Close();

			return certificateHash;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Encrypt with certificate/public key and RSA algorithm
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] EncryptDataWithCertificate(byte[] plainInputData, X509Certificate2 sebCertificate)
		{
			try
			{
				// Encrypt config data

				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				RSACryptoServiceProvider publicKey = sebCertificate.PublicKey.Key as RSACryptoServiceProvider;


				// Blocksize is for example 2048/8 = 256 
				int blockSize = (publicKey.KeySize / 8) - 32;

				// buffer to hold byte sequence of the encrypted information
				byte[] encryptedBuffer = new byte[blockSize];

				// buffer for the plain source data
				byte[] plainBuffer = new byte[blockSize];

				// initialize array so it holds at least the amount needed to decrypt.
				//byte[] decryptedData = new byte[encryptedData.Length];
				MemoryStream encryptedStream = new MemoryStream();

				// Calculate number of full data blocks that will have to be decrypted
				int blockCount = plainInputData.Length / blockSize;

				for (int i = 0; i < blockCount; i++)
				{
					// copy byte sequence from encrypted source data to the buffer
					Buffer.BlockCopy(plainInputData, i * blockSize, plainBuffer, 0, blockSize);
					// decrypt the block in the buffer
					encryptedBuffer = publicKey.Encrypt(plainBuffer, false);
					// write decrypted result back to the destination array
					encryptedStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
				}
				int remainingBytes = plainInputData.Length - (blockCount * blockSize);
				if (remainingBytes > 0)
				{
					plainBuffer = new byte[remainingBytes];
					// copy remaining bytes from encrypted source data to the buffer
					Buffer.BlockCopy(plainInputData, blockCount * blockSize, plainBuffer, 0, remainingBytes);
					// decrypt the block in the buffer
					encryptedBuffer = publicKey.Encrypt(plainBuffer, false);
					// write decrypted result back to the destination array
					//decryptedBuffer.CopyTo(decryptedData, blockCount * blockSize);
					encryptedStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
				}
				byte[] encryptedData = encryptedStream.ToArray();

				return encryptedData;
			}
			catch (CryptographicException)
			{
				//return cex.Message;
				return null;
			}
			catch (Exception)
			{
				//return ex.Message;
				return null;
			}
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Decrypt with X509 certificate/private key and RSA algorithm
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] DecryptDataWithCertificate(byte[] encryptedData, X509Certificate2 sebCertificate)
		{
			try
			{
				// Decrypt config data

				RSACryptoServiceProvider privateKey = sebCertificate.PrivateKey as RSACryptoServiceProvider;
				//byte[] decryptedData = privateKey.Decrypt(encryptedDataBytes, false);

				// Blocksize is for example 2048/8 = 256 
				int blockSize = privateKey.KeySize / 8;

				// buffer to hold byte sequence of the encrypted source data
				byte[] encryptedBuffer = new byte[blockSize];

				// buffer for the decrypted information
				byte[] decryptedBuffer = new byte[blockSize];

				// initialize array so it holds at least the amount needed to decrypt.
				//byte[] decryptedData = new byte[encryptedData.Length];
				MemoryStream decryptedStream = new MemoryStream();

				// Calculate number of full data blocks that will have to be decrypted
				int blockCount = encryptedData.Length / blockSize;

				for (int i = 0; i < blockCount; i++)
				{
					// copy byte sequence from encrypted source data to the buffer
					Buffer.BlockCopy(encryptedData, i * blockSize, encryptedBuffer, 0, blockSize);
					// decrypt the block in the buffer
					decryptedBuffer = privateKey.Decrypt(encryptedBuffer, false);
					// write decrypted result back to the destination array
					//decryptedBuffer.CopyTo(decryptedData, i*blockSize);
					decryptedStream.Write(decryptedBuffer, 0, decryptedBuffer.Length);
				}
				int remainingBytes = encryptedData.Length - (blockCount * blockSize);
				if (remainingBytes > 0)
				{
					encryptedBuffer = new byte[remainingBytes];
					// copy remaining bytes from encrypted source data to the buffer
					Buffer.BlockCopy(encryptedData, blockCount * blockSize, encryptedBuffer, 0, remainingBytes);
					// decrypt the block in the buffer
					decryptedBuffer = privateKey.Decrypt(encryptedBuffer, false);
					// write decrypted result back to the destination array
					//decryptedBuffer.CopyTo(decryptedData, blockCount * blockSize);
					decryptedStream.Write(decryptedBuffer, 0, decryptedBuffer.Length);
				}
				byte[] decryptedData = decryptedStream.ToArray();

				return decryptedData;
			}
			catch (CryptographicException cex)
			{
				Logger.AddError("Decrypting SEB config data encrypted with an identity failed with cryptographic exception:", null, cex, cex.Message);
				MessageBox.Show(SEBUIStrings.errorDecryptingSettings, SEBUIStrings.certificateDecryptingError + cex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			catch (Exception ex)
			{
				Logger.AddError("Decrypting SEB config data encrypted with an identity failed with exception:", null, ex, ex.Message);
				MessageBox.Show(SEBUIStrings.errorDecryptingSettings, SEBUIStrings.certificateDecryptingError + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Encrypt with password, key, salt using AES (Open SSL Encrypt).
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] EncryptDataWithPassword(byte[] plainData, string password)
		{
			try
			{
				// encrypt bytes
				byte[] encryptedData = AESThenHMAC.SimpleEncryptWithPassword(plainData, password, RNCRYPTOR_HEADER);

				return encryptedData;
			}
			catch (CryptographicException)
			{
				//return cex.Message;
				return null;
			}
			catch (Exception)
			{
				//return ex.Message;
				return null;
			}

		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Decrypt with password, key, salt using AES (Open SSL Decrypt)..
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] DecryptDataWithPassword(byte[] encryptedBytesWithSalt, string passphrase)
		{

			try
			{
				byte[] decryptedData = AESThenHMAC.SimpleDecryptWithPassword(encryptedBytesWithSalt, passphrase, nonSecretPayloadLength: 2);

				return decryptedData;
			}
			catch (CryptographicException)
			{
				//return cex.Message;
				return null;
			}
			catch (Exception)
			{
				//return ex.Message;
				return null;
			}
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Compute a SHA256 hash base16 string.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static string ComputePasswordHash(string input)
		{
			HashAlgorithm algorithm = new SHA256Managed();
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);

			byte[] hashedBytes = algorithm.ComputeHash(inputBytes);

			string pswdHash = BitConverter.ToString(hashedBytes).ToLower();

			return pswdHash.Replace("-", "");
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Compute a Browser Exam Key SHA256 hash base16 string.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static string ComputeBrowserExamKey()
		{
			var browserExamKey = default(string);
			var configurationKey = ComputeConfigurationKey();
			var executable = Assembly.GetExecutingAssembly();
			var certificate = executable.Modules.First().GetSignerCertificate();
			var salt = (byte[]) SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt];
			var signature = certificate?.GetCertHashString();
			var version = FileVersionInfo.GetVersionInfo(executable.Location).FileVersion;

			Logger.AddInformation("Initializing browser exam key...");

			if (configurationKey == default)
			{
				configurationKey = "";
				Logger.AddWarning("The current configuration does not contain a value for the configuration key!");
			}

			if (salt == default || salt.Length == 0)
			{
				salt = new byte[0];
				Logger.AddWarning("The current configuration does not contain a salt value for the browser exam key!");
			}

			if (TryCalculateBrowserExamKey(configurationKey, BitConverter.ToString(salt).ToLower().Replace("-", string.Empty), out browserExamKey))
			{
				Logger.AddInformation("Successfully calculated BEK using integrity module.");
			}
			else
			{
				Logger.AddWarning("Failed to calculate BEK using integrity module! Falling back to simplified calculation...");

				using (var algorithm = new HMACSHA256(salt))
				{
					var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(signature + version + configurationKey));
					var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

					browserExamKey = key;
				}
			}

			return browserExamKey;
		}

		private static bool TryCalculateBrowserExamKey(string configurationKey, string salt, out string browserExamKey)
		{
			browserExamKey = default;

			try
			{
				browserExamKey = CalculateBrowserExamKey(configurationKey, salt);
			}
			catch (DllNotFoundException)
			{
				Logger.AddWarning("Integrity module is not available!");
			}
			catch (Exception e)
			{
				Logger.AddError("Unexpected error while attempting to calculate browser exam key!", default, e);
			}

			return browserExamKey != default;
		}

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.BStr)]
		private static extern string CalculateBrowserExamKey(string configurationKey, string salt);

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Compute a Configuration Key SHA256 hash base16 string.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static string ComputeConfigurationKey()
		{
			using (var algorithm = new SHA256Managed())
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				Serialize(SEBSettings.settingsCurrentOriginal, writer);

				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);

				var hash = algorithm.ComputeHash(stream);
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				return key;
			}
		}

		private static void Serialize(IDictionary<string, object> dictionary, StreamWriter stream)
		{
			var orderedByKey = dictionary.OrderBy(d => d.Key, StringComparer.InvariantCulture).ToList();

			stream.Write('{');

			foreach (var kvp in orderedByKey)
			{
				var process = true;

				process &= !kvp.Key.Equals(SEBSettings.KeyOriginatorVersion, StringComparison.OrdinalIgnoreCase);
				process &= !(kvp.Value is IDictionary<string, object> d) || d.Any();

				if (process)
				{
					stream.Write('"');
					stream.Write(kvp.Key);
					stream.Write('"');
					stream.Write(':');
					Serialize(kvp.Value, stream);

					if (kvp.Key != orderedByKey.Last().Key)
					{
						stream.Write(',');
					}
				}
			}

			stream.Write('}');
		}

		private static void Serialize(IList<object> list, StreamWriter stream)
		{
			stream.Write('[');

			foreach (var item in list)
			{
				Serialize(item, stream);

				if (item != list.Last())
				{
					stream.Write(',');
				}
			}

			stream.Write(']');
		}

		private static void Serialize(object value, StreamWriter stream)
		{
			switch (value)
			{
				case IDictionary<string, object> dictionary:
					Serialize(dictionary, stream);
					break;
				case IList<object> list:
					Serialize(list, stream);
					break;
				case byte[] data:
					stream.Write('"');
					stream.Write(Convert.ToBase64String(data));
					stream.Write('"');
					break;
				case DateTime date:
					stream.Write(date.ToString("o"));
					break;
				case bool boolean:
					stream.Write(boolean.ToString().ToLower());
					break;
				case int integer:
					stream.Write(integer.ToString(NumberFormatInfo.InvariantInfo));
					break;
				case double number:
					stream.Write(number.ToString(NumberFormatInfo.InvariantInfo));
					break;
				case string text:
					stream.Write('"');
					stream.Write(text);
					stream.Write('"');
					break;
				case null:
					stream.Write('"');
					stream.Write('"');
					break;
			}
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Generate a Browser Exam Key Salt as byte data.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] GenerateBrowserExamKeySalt()
		{
			byte[] saltBytes = AESThenHMAC.NewKey();
			//string saltString = BitConverter.ToString(saltBytes);
			//return saltString.Replace("-", "");
			return saltBytes;
		}
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Class for encrypting and decrypting with AES and HMAC, 
	/// compatible with RNCryptor on Mac OS X and iOS.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	/// This work (Modern Encryption of a String C#, by James Tuley), 
	/// identified by James Tuley, is free of known copyright restrictions.
	/// https://gist.github.com/4336842
	/// http://creativecommons.org/publicdomain/mark/1.0/ 
	/// ----------------------------------------------------------------------------------------
	public static class AESThenHMAC
	{
		private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

		//Preconfigured Encryption Parameters
		public static readonly int BlockBitSize = 128;
		public static readonly int KeyBitSize = 256;

		//Preconfigured Password Key Derivation Parameters
		public static readonly int SaltBitSize = 64;
		public static readonly int Iterations = 10000;
		public static readonly int MinPasswordLength = 3;

		/// <summary>
		/// Helper that generates a random key on each call.
		/// </summary>
		/// <returns></returns>
		public static byte[] NewKey()
		{
			var key = new byte[KeyBitSize / 8];
			Random.GetBytes(key);
			return key;
		}

		/// <summary>
		/// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
		/// </summary>
		/// <param name="secretMessage">The secret message.</param>
		/// <param name="cryptKey">The crypt key.</param>
		/// <param name="authKey">The auth key.</param>
		/// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
		/// <returns>
		/// Encrypted Message
		/// </returns>
		/// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
		/// <remarks>
		/// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
		/// </remarks>
		public static string SimpleEncrypt(string secretMessage, byte[] cryptKey, byte[] authKey,
						   byte[] nonSecretPayload = null)
		{
			if (string.IsNullOrEmpty(secretMessage))
				throw new ArgumentException("Secret Message Required!", "secretMessage");

			var plainText = Encoding.UTF8.GetBytes(secretMessage);
			var cipherText = SimpleEncrypt(plainText, cryptKey, authKey, nonSecretPayload);
			return Convert.ToBase64String(cipherText);
		}

		/// <summary>
		/// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message.</param>
		/// <param name="cryptKey">The crypt key.</param>
		/// <param name="authKey">The auth key.</param>
		/// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
		/// <returns>
		/// Decrypted Message
		/// </returns>
		/// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
		public static string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey,
						   int nonSecretPayloadLength = 0)
		{
			if (string.IsNullOrWhiteSpace(encryptedMessage))
				throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

			var cipherText = Convert.FromBase64String(encryptedMessage);
			var plainText = SimpleDecrypt(cipherText, cryptKey, authKey, nonSecretPayloadLength);
			return Encoding.UTF8.GetString(plainText);
		}

		/// <summary>
		/// Simple Encryption (AES) then Authentication (HMAC) of a UTF8 message
		/// using Keys derived from a Password (PBKDF2).
		/// </summary>
		/// <param name="secretMessage">The secret message.</param>
		/// <param name="password">The password.</param>
		/// <param name="nonSecretPayload">The non secret payload.</param>
		/// <returns>
		/// Encrypted Message
		/// </returns>
		/// <exception cref="System.ArgumentException">password</exception>
		/// <remarks>
		/// Significantly less secure than using random binary keys.
		/// Adds additional non secret payload for key generation parameters.
		/// </remarks>
		public static string SimpleEncryptWithPassword(string secretMessage, string password,
								 byte[] nonSecretPayload = null)
		{
			if (string.IsNullOrEmpty(secretMessage))
				throw new ArgumentException("Secret Message Required!", "secretMessage");

			var plainText = Encoding.UTF8.GetBytes(secretMessage);
			var cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
			return Convert.ToBase64String(cipherText);
		}

		/// <summary>
		/// Simple Authentication (HMAC) and then Descryption (AES) of a UTF8 Message
		/// using keys derived from a password (PBKDF2). 
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message.</param>
		/// <param name="password">The password.</param>
		/// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
		/// <returns>
		/// Decrypted Message
		/// </returns>
		/// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
		/// <remarks>
		/// Significantly less secure than using random binary keys.
		/// </remarks>
		public static string SimpleDecryptWithPassword(string encryptedMessage, string password,
								 int nonSecretPayloadLength = 0)
		{
			if (string.IsNullOrWhiteSpace(encryptedMessage))
				throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

			var cipherText = Convert.FromBase64String(encryptedMessage);
			var plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
			return Encoding.UTF8.GetString(plainText);
		}

		public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
		{
			//User Error Checks
			if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "cryptKey");

			if (authKey == null || authKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "authKey");

			if (secretMessage == null || secretMessage.Length < 1)
				throw new ArgumentException("Secret Message Required!", "secretMessage");

			//non-secret payload optional
			nonSecretPayload = nonSecretPayload ?? new byte[] { };

			byte[] cipherText;
			byte[] iv;

			using (var aes = new AesManaged
			{
				KeySize = KeyBitSize,
				BlockSize = BlockBitSize,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7
			})
			{

				//Use random IV
				aes.GenerateIV();
				iv = aes.IV;

				using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
				using (var cipherStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
					using (var binaryWriter = new BinaryWriter(cryptoStream))
					{
						//Encrypt Data
						binaryWriter.Write(secretMessage);
					}

					cipherText = cipherStream.ToArray();
				}

			}

			//Assemble encrypted message and add authentication
			using (var hmac = new HMACSHA256(authKey))
			using (var encryptedStream = new MemoryStream())
			{
				using (var binaryWriter = new BinaryWriter(encryptedStream))
				{
					//Prepend non-secret payload if any
					binaryWriter.Write(nonSecretPayload);
					//Prepend IV
					binaryWriter.Write(iv);
					//Write Ciphertext
					binaryWriter.Write(cipherText);
					binaryWriter.Flush();

					//Authenticate all data
					var tag = hmac.ComputeHash(encryptedStream.ToArray());
					//Postpend tag
					binaryWriter.Write(tag);
				}
				return encryptedStream.ToArray();
			}

		}

		public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
		{

			//Basic Usage Error Checks
			if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), "cryptKey");

			if (authKey == null || authKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), "authKey");

			if (encryptedMessage == null || encryptedMessage.Length == 0)
				throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

			using (var hmac = new HMACSHA256(authKey))
			{
				var sentTag = new byte[hmac.HashSize / 8];
				//Calculate Tag
				var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
				var ivLength = (BlockBitSize / 8);

				//if message length is to small just return null
				if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
					return null;

				//Grab Sent Tag
				Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

				//Compare Tag with constant time comparison
				var compare = 0;
				for (var i = 0; i < sentTag.Length; i++)
					compare |= sentTag[i] ^ calcTag[i];

				//if message doesn't authenticate return null
				if (compare != 0)
					return null;

				using (var aes = new AesManaged
				{
					KeySize = KeyBitSize,
					BlockSize = BlockBitSize,
					Mode = CipherMode.CBC,
					Padding = PaddingMode.PKCS7
				})
				{

					//Grab IV from message
					var iv = new byte[ivLength];
					Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

					using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
					using (var plainTextStream = new MemoryStream())
					{
						using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
						using (var binaryWriter = new BinaryWriter(decrypterStream))
						{
							//Decrypt Cipher Text from Message
							binaryWriter.Write(
							  encryptedMessage,
							  nonSecretPayloadLength + iv.Length,
							  encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
							);
						}
						//Return Plain Text
						return plainTextStream.ToArray();
					}
				}
			}
		}

		public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
		{
			nonSecretPayload = nonSecretPayload ?? new byte[] { };

			//User Error Checks
			//if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
			//    throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

			if (secretMessage == null || secretMessage.Length == 0)
				throw new ArgumentException("Secret Message Required!", "secretMessage");

			var payload = new byte[((SaltBitSize / 8) * 2) + nonSecretPayload.Length];

			Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
			int payloadIndex = nonSecretPayload.Length;

			byte[] cryptKey;
			byte[] authKey;
			//Use Random Salt to prevent pre-generated weak password attacks.
			using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
			{
				var salt = generator.Salt;

				//Generate Keys
				cryptKey = generator.GetBytes(KeyBitSize / 8);

				//Create Non Secret Payload
				Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
				payloadIndex += salt.Length;
			}

			//Deriving separate key, might be less efficient than using HKDF, 
			//but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
			using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
			{
				var salt = generator.Salt;

				//Generate Keys
				authKey = generator.GetBytes(KeyBitSize / 8);

				//Create Rest of Non Secret Payload
				Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
			}

			return SimpleEncrypt(secretMessage, cryptKey, authKey, payload);
		}

		public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
		{
			if (encryptedMessage == null || encryptedMessage.Length == 0)
			{
				throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
			}

			var cryptSalt = new byte[SaltBitSize / 8];
			var authSalt = new byte[SaltBitSize / 8];

			//Grab Salt from Non-Secret Payload
			Array.Copy(encryptedMessage, nonSecretPayloadLength, cryptSalt, 0, cryptSalt.Length);
			Array.Copy(encryptedMessage, nonSecretPayloadLength + cryptSalt.Length, authSalt, 0, authSalt.Length);

			byte[] cryptKey;
			byte[] authKey;

			//Generate crypt key
			using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, Iterations))
			{
				cryptKey = generator.GetBytes(KeyBitSize / 8);
			}
			//Generate auth key
			using (var generator = new Rfc2898DeriveBytes(password, authSalt, Iterations))
			{
				authKey = generator.GetBytes(KeyBitSize / 8);
			}

			return SimpleDecrypt(encryptedMessage, cryptKey, authKey, cryptSalt.Length + authSalt.Length + nonSecretPayloadLength);
		}
	}
}