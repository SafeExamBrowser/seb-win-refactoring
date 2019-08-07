using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
//  Copyright (c) 2010-2019 Daniel R. Schneider, 
//  ETH Zurich, Educational Development and Technology (LET),
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
//  are Copyright (c) 2010-2019 Daniel R. Schneider, 
//  ETH Zurich, Educational Development and Technology (LET), 
//  based on the original idea of Safe Exam Browser
//  by Stefan Schneider, University of Giessen. All Rights Reserved.
//
//  Contributor(s): ______________________________________.
//

namespace SebWindowsConfig.Utilities
{
	public class SEBProtectionController
	{
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
		private static byte[] RNCRYPTOR_HEADER = new byte[] { 0x02, 0x01 };

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
			// Serialize preferences dictionary to an XML string
			string sebXML = Plist.writeXml(SEBSettings.settingsCurrent);

			//Add the Hash of the Executable and of the XulRunnerFiles to the message

			sebXML = String.Format("{0}{1}", sebXML, ComputeSEBComponentsHash());

			byte[] message = Encoding.UTF8.GetBytes(sebXML);
			byte[] salt = (byte[])SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, SEBSettings.KeyExamKeySalt);
			var hash = new HMACSHA256(salt);
			byte[] browserExamKey = hash.ComputeHash(message);
			string browserExamKeyString = BitConverter.ToString(browserExamKey);
			return browserExamKeyString.Replace("-", "").ToLower();
		}

		private static string ComputeSEBComponentsHash()
		{
			string SEBDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			var fileNames = new List<string>
			{
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_SEB),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_SEBCONFIGTOOL),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_FLECK),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_ICONLIB),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_IONICZIP),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_METRO),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_NAUDIO),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_NEWTONSOFTJSON),
				Path.Combine(SEBDirectory, SEBClientInfo.FILENAME_DLL_SERVICECONTRACTS),
				Path.Combine(SEBDirectory, SEBClientInfo.SEB_SERVICE_DIRECTORY, SEBClientInfo.FILENAME_SEBSERVICE),
				Path.Combine(SEBDirectory, SEBClientInfo.SEB_SERVICE_DIRECTORY, SEBClientInfo.FILENAME_DLL_SERVICECONTRACTS),
			};

			var SEBBrowserDirectory = Path.Combine(SEBDirectory, SEBClientInfo.SEB_BROWSER_DIRECTORY);
			if (Directory.Exists(SEBBrowserDirectory))
			{
				List<string> browserFiles = new List<string>(Directory.GetFiles(SEBBrowserDirectory, "*.*", SearchOption.AllDirectories));
				browserFiles = browserFiles.Where(x => !x.EndsWith(".gitignore") && !x.EndsWith(".DS_Store")).ToList();
				browserFiles.Sort(StringComparer.InvariantCulture);

				int fileCounter = 0;
				int pathBrowserDirectoryLength = SEBBrowserDirectory.Length + 1;
				var spareFileNames = new List<string>();
				foreach (var browserFile in browserFiles)
				{
					string browserFileWithoutBrowserPath = browserFile.Remove(0, pathBrowserDirectoryLength);
					string originalBrowserFile = allSEBBrowserFiles[fileCounter];
					if (!browserFileWithoutBrowserPath.Equals(allSEBBrowserFiles[fileCounter]))
					{
						spareFileNames.Add(browserFile);
					}
					else
					{
						if (fileCounter < allSEBBrowserFiles.Count)
						{
							fileCounter++;
						}
					}
				}

				if (spareFileNames.Count > 0)
				{
					Logger.AddInformation(SEBUIStrings.spareBrowserFileNamesFoundText + Environment.NewLine + string.Join(Environment.NewLine, spareFileNames.ToArray()));
					ShowSpareFilesErrorMessage(spareFileNames);
				}

				fileNames.AddRange(browserFiles);
			}
 
			Logger.AddInformation("All SEB files: " + Environment.NewLine + string.Join(Environment.NewLine, fileNames.ToArray()));
			return ComputeHashForFiles(fileNames);
		}

		private static void ShowSpareFilesErrorMessage(List<string> spareFileNames)
		{
			MessageBox.Show(SEBUIStrings.spareBrowserFileNamesFound, SEBUIStrings.spareBrowserFileNamesFoundText + Environment.NewLine + string.Join(Environment.NewLine, spareFileNames.ToArray()), MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static string ComputeHashForFiles(IEnumerable<string> fileNames)
		{
			var bigHash = new StringBuilder();
			foreach (string fileName in fileNames)
			{
				bigHash.Append(ComputeFileHash(fileName));
			}
			var sha = new SHA256Managed();
			byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(bigHash.ToString()));
			return BitConverter.ToString(hash).Replace("-", String.Empty);
		}

		private static string ComputeFileHash(string file)
		{
			if (!File.Exists(file)) return null;

			using (FileStream stream = File.OpenRead(file))
			{
				var sha = new SHA256Managed();
				byte[] hash = sha.ComputeHash(stream);
				return BitConverter.ToString(hash).Replace("-", String.Empty);
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

		private static List<string> allSEBBrowserFiles = new List<string> {
			"xul_seb\\chrome.manifest",
			"xul_seb\\chrome\\branding.jar",
			"xul_seb\\chrome\\branding.manifest",
			"xul_seb\\chrome\\content\\pdfjs\\build\\pdf.js",
			"xul_seb\\chrome\\content\\pdfjs\\build\\pdf.js.map",
			"xul_seb\\chrome\\content\\pdfjs\\build\\pdf.worker.js",
			"xul_seb\\chrome\\content\\pdfjs\\build\\pdf.worker.js.map",
			"xul_seb\\chrome\\content\\pdfjs\\LICENSE",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78ms-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78ms-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\78-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\83pv-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\90msp-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\90msp-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\90ms-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\90ms-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\90pv-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\90pv-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Add-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Add-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Add-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Add-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-0.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-1.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-3.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-4.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-5.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-6.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-CNS1-UCS2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-0.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-1.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-3.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-4.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-5.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-GB1-UCS2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-0.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-1.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-3.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-4.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-5.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-6.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Japan1-UCS2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Korea1-0.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Korea1-1.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Korea1-2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Adobe-Korea1-UCS2.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\B5pc-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\B5pc-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\CNS1-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\CNS1-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\CNS2-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\CNS2-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\CNS-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\CNS-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\ETen-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\ETen-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\ETenms-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\ETenms-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\ETHK-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\ETHK-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Ext-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Ext-RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Ext-RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Ext-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GB-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GB-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GB-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBK2K-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBK2K-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBK-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBK-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBKp-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBKp-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBpc-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBpc-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBT-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBT-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBT-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBTpc-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBTpc-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GBT-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\GB-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Hankaku.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Hiragana.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKdla-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKdla-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKdlb-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKdlb-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKgccs-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKgccs-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKm314-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKm314-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKm471-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKm471-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKscs-B5-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\HKscs-B5-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Katakana.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSC-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSC-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSC-Johab-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSC-Johab-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSCms-UHC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSCms-UHC-HW-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSCms-UHC-HW-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSCms-UHC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSCpc-EUC-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSCpc-EUC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\KSC-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\LICENSE",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\NWP-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\NWP-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\RKSJ-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\RKSJ-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\Roman.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UCS2-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UCS2-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UTF16-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UTF16-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UTF8-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniCNS-UTF8-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UCS2-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UCS2-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UTF16-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UTF16-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UTF8-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniGB-UTF8-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS2004-UTF16-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS2004-UTF16-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS2004-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS2004-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS2004-UTF8-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS2004-UTF8-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISPro-UCS2-HW-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISPro-UCS2-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISPro-UTF8-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UCS2-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UCS2-HW-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UCS2-HW-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UCS2-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UTF16-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UTF16-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UTF8-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJIS-UTF8-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISX02132004-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISX02132004-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISX0213-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniJISX0213-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UCS2-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UCS2-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UTF16-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UTF16-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UTF32-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UTF32-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UTF8-H.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\UniKS-UTF8-V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\V.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\cmaps\\WP-Symbol.bcmap",
			"xul_seb\\chrome\\content\\pdfjs\\web\\compressed.tracemonkey-pldi-09.pdf",
			"xul_seb\\chrome\\content\\pdfjs\\web\\debugger.js",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-check.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-comment.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-help.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-insert.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-key.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-newparagraph.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-noicon.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-note.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\annotation-paragraph.svg",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-next.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-next@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-next-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-next-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-previous.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-previous@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-previous-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\findbarButton-previous-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\grab.cur",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\grabbing.cur",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\loading-icon.gif",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\loading-small.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\loading-small@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-documentProperties.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-documentProperties@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-firstPage.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-firstPage@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-handTool.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-handTool@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-lastPage.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-lastPage@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-rotateCcw.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-rotateCcw@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-rotateCw.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-rotateCw@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-selectTool.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\secondaryToolbarButton-selectTool@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\shadow.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\texture.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-bookmark.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-bookmark@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-download.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-download@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-menuArrows.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-menuArrows@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-openFile.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-openFile@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageDown.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageDown@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageDown-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageDown-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageUp.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageUp@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageUp-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-pageUp-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-presentationMode.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-presentationMode@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-print.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-print@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-search.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-search@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-secondaryToolbarToggle.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-secondaryToolbarToggle@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-secondaryToolbarToggle-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-secondaryToolbarToggle-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-sidebarToggle.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-sidebarToggle@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-sidebarToggle-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-sidebarToggle-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewAttachments.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewAttachments@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewOutline.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewOutline@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewOutline-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewOutline-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewThumbnail.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-viewThumbnail@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-zoomIn.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-zoomIn@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-zoomOut.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\toolbarButton-zoomOut@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\treeitem-collapsed.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\treeitem-collapsed@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\treeitem-collapsed-rtl.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\treeitem-collapsed-rtl@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\treeitem-expanded.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\images\\treeitem-expanded@2x.png",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ach\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\af\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ak\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\an\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ar\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\as\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ast\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\az\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\be\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\bg\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\bn-BD\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\bn-IN\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\br\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\bs\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ca\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\cs\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\csb\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\cy\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\da\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\de\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\el\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\en-GB\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\en-US\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\en-ZA\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\eo\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\es-AR\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\es-CL\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\es-ES\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\es-MX\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\et\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\eu\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\fa\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ff\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\fi\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\fr\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\fy-NL\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ga-IE\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\gd\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\gl\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\gu-IN\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\he\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\hi-IN\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\hr\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\hu\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\hy-AM\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\id\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\is\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\it\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ja\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ka\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\kk\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\km\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\kn\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ko\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ku\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\lg\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\lij\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\locale.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\lt\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\lv\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\mai\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\mk\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ml\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\mn\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\mr\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ms\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\my\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\nb-NO\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\nl\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\nn-NO\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\nso\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\oc\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\or\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\pa-IN\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\pl\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\pt-BR\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\pt-PT\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\rm\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ro\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ru\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\rw\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sah\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\si\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sk\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sl\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\son\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sq\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sr\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sv-SE\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\sw\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ta\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ta-LK\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\te\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\th\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\tl\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\tn\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\tr\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\uk\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\ur\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\vi\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\wo\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\xh\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\zh-CN\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\zh-TW\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\locale\\zu\\viewer.properties",
			"xul_seb\\chrome\\content\\pdfjs\\web\\viewer.css",
			"xul_seb\\chrome\\content\\pdfjs\\web\\viewer.html",
			"xul_seb\\chrome\\content\\pdfjs\\web\\viewer.js",
			"xul_seb\\chrome\\content\\pdfjs\\web\\viewer.js.map",
			"xul_seb\\chrome\\content\\seb\\css\\aboutNetError_alert.svg",
			"xul_seb\\chrome\\content\\seb\\css\\aboutNetError_info.svg",
			"xul_seb\\chrome\\content\\seb\\css\\neterror.css",
			"xul_seb\\chrome\\content\\seb\\css\\seb.css",
			"xul_seb\\chrome\\content\\seb\\err.xul",
			"xul_seb\\chrome\\content\\seb\\error.html",
			"xul_seb\\chrome\\content\\seb\\error.xhtml",
			"xul_seb\\chrome\\content\\seb\\hidden.xul",
			"xul_seb\\chrome\\content\\seb\\images\\back.png",
			"xul_seb\\chrome\\content\\seb\\images\\forward.png",
			"xul_seb\\chrome\\content\\seb\\images\\loading.gif",
			"xul_seb\\chrome\\content\\seb\\images\\quit.png",
			"xul_seb\\chrome\\content\\seb\\images\\reload.png",
			"xul_seb\\chrome\\content\\seb\\images\\restart.png",
			"xul_seb\\chrome\\content\\seb\\images\\zoom.png",
			"xul_seb\\chrome\\content\\seb\\load.xul",
			"xul_seb\\chrome\\content\\seb\\lockscreen.xul",
			"xul_seb\\chrome\\content\\seb\\message_socket.html",
			"xul_seb\\chrome\\content\\seb\\reconf.xhtml",
			"xul_seb\\chrome\\content\\seb\\reconf.xul",
			"xul_seb\\chrome\\content\\seb\\seb.properties",
			"xul_seb\\chrome\\content\\seb\\seb.xul",
			"xul_seb\\chrome\\content\\seb\\sounds\\snapshot.mp3",
			"xul_seb\\chrome\\content\\seb\\sounds\\snapshot.ogg",
			"xul_seb\\chrome\\content\\seb\\sounds\\snapshot.wav",
			"xul_seb\\chrome\\locale\\seb\\de-DE\\seb.dtd",
			"xul_seb\\chrome\\locale\\seb\\de-DE\\seb.properties",
			"xul_seb\\chrome\\locale\\seb\\en-US\\seb.dtd",
			"xul_seb\\chrome\\locale\\seb\\en-US\\seb.properties",
			"xul_seb\\chrome\\locale\\seb\\fr-FR\\seb.dtd",
			"xul_seb\\chrome\\locale\\seb\\fr-FR\\seb.properties",
			"xul_seb\\chrome\\pdfjs.manifest",
			"xul_seb\\chrome\\seb.manifest",
			"xul_seb\\components\\certsService.js",
			"xul_seb\\components\\sebProtocol.js",
			"xul_seb\\components\\sebsProtocol.js",
			"xul_seb\\components\\xulApplication.js",
			"xul_seb\\config.certs.json",
			"xul_seb\\config.custom.json",
			"xul_seb\\config.default.json",
			"xul_seb\\config.dev.json",
			"xul_seb\\config.full.json",
			"xul_seb\\config.json",
			"xul_seb\\config.localhost.json",
			"xul_seb\\config.SEB22.json",
			"xul_seb\\config.server.json",
			"xul_seb\\debug_prefs.js",
			"xul_seb\\debug_reset_prefs.js",
			"xul_seb\\default.json",
			"xul_seb\\defaults\\preferences\\seb.js",
			"xul_seb\\defaults\\profile\\mimeTypes.rdf",
			"xul_seb\\dictionaries\\da-DK\\da-DK.aff",
			"xul_seb\\dictionaries\\da-DK\\da-DK.dic",
			"xul_seb\\dictionaries\\da-DK\\README_da.txt",
			"xul_seb\\dictionaries\\en-AU\\en-AU.aff",
			"xul_seb\\dictionaries\\en-AU\\en-AU.dic",
			"xul_seb\\dictionaries\\en-AU\\README_en-AU.txt",
			"xul_seb\\dictionaries\\en-GB\\en-GB.aff",
			"xul_seb\\dictionaries\\en-GB\\en-GB.dic",
			"xul_seb\\dictionaries\\en-GB\\README_en-GB.txt",
			"xul_seb\\dictionaries\\en-US\\en-US.aff",
			"xul_seb\\dictionaries\\en-US\\en-US.dic",
			"xul_seb\\dictionaries\\en-US\\README_en-US.txt",
			"xul_seb\\dictionaries\\es-ES\\es-ES.aff",
			"xul_seb\\dictionaries\\es-ES\\es-ES.dic",
			"xul_seb\\dictionaries\\es-ES\\README_es-ES.txt",
			"xul_seb\\dictionaries\\fr-FR\\fr-FR.aff",
			"xul_seb\\dictionaries\\fr-FR\\fr-FR.dic",
			"xul_seb\\dictionaries\\fr-FR\\README_fr.txt",
			"xul_seb\\dictionaries\\pt-PT\\pt-PT.aff",
			"xul_seb\\dictionaries\\pt-PT\\pt-PT.dic",
			"xul_seb\\dictionaries\\pt-PT\\README_pt-PT.txt",
			"xul_seb\\dictionaries\\sv-FI\\README_sv.txt",
			"xul_seb\\dictionaries\\sv-FI\\sv-FI.aff",
			"xul_seb\\dictionaries\\sv-FI\\sv-FI.dic",
			"xul_seb\\dictionaries\\sv-SE\\README_sv.txt",
			"xul_seb\\dictionaries\\sv-SE\\sv-SE.aff",
			"xul_seb\\dictionaries\\sv-SE\\sv-SE.dic",
			"xul_seb\\globals\\const.js",
			"xul_seb\\globals\\prototypes.js",
			"xul_seb\\modules\\empty.js",
			"xul_seb\\modules\\seb.jsm",
			"xul_seb\\modules\\SebBrowser.jsm",
			"xul_seb\\modules\\SebConfig.jsm",
			"xul_seb\\modules\\SebHost.jsm",
			"xul_seb\\modules\\SebLog.jsm",
			"xul_seb\\modules\\SebNet.jsm",
			"xul_seb\\modules\\SebScreenshot.jsm",
			"xul_seb\\modules\\SebServer.jsm",
			"xul_seb\\modules\\SebUtils.jsm",
			"xul_seb\\modules\\SebWin.jsm",
			"xul_seb\\modules\\ZoomManager.jsm",
			"xul_seb\\seb.ini",
			"xulrunner\\Accessible.tlb",
			"xulrunner\\AccessibleMarshal.dll",
			"xulrunner\\api-ms-win-core-console-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-datetime-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-debug-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-errorhandling-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-file-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-file-l1-2-0.dll",
			"xulrunner\\api-ms-win-core-file-l2-1-0.dll",
			"xulrunner\\api-ms-win-core-handle-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-heap-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-interlocked-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-libraryloader-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-localization-l1-2-0.dll",
			"xulrunner\\api-ms-win-core-memory-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-namedpipe-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-processenvironment-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-processthreads-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-processthreads-l1-1-1.dll",
			"xulrunner\\api-ms-win-core-profile-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-rtlsupport-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-string-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-synch-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-synch-l1-2-0.dll",
			"xulrunner\\api-ms-win-core-sysinfo-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-timezone-l1-1-0.dll",
			"xulrunner\\api-ms-win-core-util-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-conio-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-convert-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-environment-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-filesystem-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-heap-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-locale-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-math-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-multibyte-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-private-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-process-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-runtime-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-stdio-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-string-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-time-l1-1-0.dll",
			"xulrunner\\api-ms-win-crt-utility-l1-1-0.dll",
			"xulrunner\\application.ini",
			"xulrunner\\breakpadinjector.dll",
			"xulrunner\\browser\\blocklist.xml",
			"xulrunner\\browser\\chrome.manifest",
			"xulrunner\\browser\\crashreporter-override.ini",
			"xulrunner\\browser\\extensions\\{972ce4c6-7e08-4474-a285-3208198ce6fd}.xpi",
			"xulrunner\\browser\\features\\aushelper@mozilla.org.xpi",
			"xulrunner\\browser\\features\\e10srollout@mozilla.org.xpi",
			"xulrunner\\browser\\features\\firefox@getpocket.com.xpi",
			"xulrunner\\browser\\features\\webcompat@mozilla.org.xpi",
			"xulrunner\\browser\\omni.ja",
			"xulrunner\\browser\\VisualElements\\VisualElements_150.png",
			"xulrunner\\browser\\VisualElements\\VisualElements_70.png",
			"xulrunner\\chrome.manifest",
			"xulrunner\\crashreporter.exe",
			"xulrunner\\crashreporter.ini",
			"xulrunner\\D3DCompiler_43.dll",
			"xulrunner\\d3dcompiler_47.dll",
			"xulrunner\\defaults\\pref\\channel-prefs.js",
			"xulrunner\\dependentlibs.list",
			"xulrunner\\firefox.exe",
			"xulrunner\\firefox.VisualElementsManifest.xml",
			"xulrunner\\freebl3.chk",
			"xulrunner\\freebl3.dll",
			"xulrunner\\gmp-clearkey\\0.1\\clearkey.dll",
			"xulrunner\\gmp-clearkey\\0.1\\clearkey.info",
			"xulrunner\\IA2Marshal.dll",
			"xulrunner\\install.log",
			"xulrunner\\lgpllibs.dll",
			"xulrunner\\libEGL.dll",
			"xulrunner\\libGLESv2.dll",
			"xulrunner\\maintenanceservice.exe",
			"xulrunner\\maintenanceservice_installer.exe",
			"xulrunner\\minidump-analyzer.exe",
			"xulrunner\\mozavcodec.dll",
			"xulrunner\\mozavutil.dll",
			"xulrunner\\mozglue.dll",
			"xulrunner\\msvcp140.dll",
			"xulrunner\\nss3.dll",
			"xulrunner\\nssckbi.dll",
			"xulrunner\\nssdbm3.chk",
			"xulrunner\\nssdbm3.dll",
			"xulrunner\\omni.ja",
			"xulrunner\\platform.ini",
			"xulrunner\\plugin-container.exe",
			"xulrunner\\plugin-hang-ui.exe",
			"xulrunner\\precomplete",
			"xulrunner\\qipcap.dll",
			"xulrunner\\removed-files",
			"xulrunner\\softokn3.chk",
			"xulrunner\\softokn3.dll",
			"xulrunner\\ucrtbase.dll",
			"xulrunner\\uninstall\\helper.exe",
			"xulrunner\\uninstall\\shortcuts_log.ini",
			"xulrunner\\uninstall\\uninstall.log",
			"xulrunner\\updater.exe",
			"xulrunner\\updater.ini",
			"xulrunner\\update-settings.ini",
			"xulrunner\\vcruntime140.dll",
			"xulrunner\\voucher.bin",
			"xulrunner\\wow_helper.exe",
			"xulrunner\\xul.dll"};
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