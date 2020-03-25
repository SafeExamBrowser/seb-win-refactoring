using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using DictObj = System.Collections.Generic.Dictionary<string, object>;

//
//  SEBConfigFileManager.cs
//  SafeExamBrowser
//
//  Copyright (c) 2010-2020 Daniel R. Schneider, 
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
//  are Copyright (c) 2010-2020 Daniel R. Schneider, 
//  ETH Zurich, Educational Development and Technology (LET), 
//  based on the original idea of Safe Exam Browser
//  by Stefan Schneider, University of Giessen. All Rights Reserved.
//
//  Contributor(s): ______________________________________.
//

namespace SebWindowsConfig.Utilities
{
	public class SEBConfigFileManager
	{
		public static SebPasswordDialogForm sebPasswordDialogForm;

		// Prefixes
		private const int PREFIX_LENGTH = 4;
		private const int MULTIPART_LENGTH = 8;
		private const int CUSTOMHEADER_LENGTH = 4;
		private const string PUBLIC_KEY_HASH_MODE = "pkhs";
		private const string PUBLIC_SYMMETRIC_KEY_MODE = "phsk";
		private const string PASSWORD_MODE = "pswd";
		private const string PLAIN_DATA_MODE = "plnd";
		private const string PASSWORD_CONFIGURING_CLIENT_MODE = "pwcc";
		private const string UNENCRYPTED_MODE = "<?xm";
		private const string MULTIPART_MODE = "mphd";
		private const string CUSTOM_HEADER_MODE = "cmhd";

		// Public key hash identifier length
		private const int PUBLIC_KEY_HASH_LENGTH = 20;

			
		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Decrypt and deserialize SEB settings
		/// When forEditing = true, then the decrypting password the user entered and/or 
		/// certificate reference found in the .seb file is returned 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static DictObj DecryptSEBSettings(byte[] sebData, bool forEditing, ref string sebFilePassword, ref bool passwordIsHash, ref X509Certificate2 sebFileCertificateRef, bool suppressFileFormatError = false)
		{
			// Ungzip the .seb (according to specification >= v14) source data
			byte[] unzippedSebData = GZipByte.Decompress(sebData);

			// if unzipped data is not null, then unzipping worked, we use unzipped data
			// if unzipped data is null, then the source data may be an uncompressed .seb file, we proceed with it
			if (unzippedSebData != null) sebData = unzippedSebData;

			string prefixString;

			// save the data including the first 4 bytes for the case that it's acutally an unencrypted XML plist
			byte[] sebDataUnencrypted = sebData.Clone() as byte[];

			// Get 4-char prefix
			prefixString = GetPrefixStringFromData(ref sebData);

			//// Check prefix identifying encryption modes

			/// Check for new Multipart and Custom headers

			// Multipart Config File: The first part containts the regular SEB key/value settings
			// following parts can contain additional resources. An updated SEB version will be
			// able to read and process those parts sequentially as a stream.
			// Therefore potentially large additional resources won't have to be loaded into memory at once
			if (prefixString.CompareTo(MULTIPART_MODE) == 0)
			{
				// Skip the Multipart Config File header
				byte[] multipartConfigLengthData = GetPrefixDataFromData(ref sebData, MULTIPART_LENGTH);
				long multipartConfigLength = BitConverter.ToInt64(multipartConfigLengthData, 0);
				Logger.AddInformation("Multipart Config File, first part (settings) length: " + multipartConfigLength);

				try
				{
					Logger.AddInformation("Cropping config file, as this SEB version cannot process additional parts of multipart config files.");

					byte[] dataFirstPart = new byte[sebData.Length - multipartConfigLength];
					Buffer.BlockCopy(sebData, 0, dataFirstPart, 0, dataFirstPart.Length);
					sebData = dataFirstPart;
				}
				catch (Exception ex)
				{
					Logger.AddError("Error while cropping config file", null, ex, ex.Message);
				}
			}

			// Custom Header: Containts a 32 bit value for the length of the header
			// followed by the custom header information. After the header, regular
			// SEB config file data follows
			if (prefixString.CompareTo(CUSTOM_HEADER_MODE) == 0)
			{
				// Skip the Custom Header
				byte[] customHeaderLengthData = GetPrefixDataFromData(ref sebData, CUSTOMHEADER_LENGTH);
				int customHeaderLength = BitConverter.ToInt32(customHeaderLengthData, 0);
				Logger.AddInformation("Custom Config File Header length: " + customHeaderLength);
				try
				{
					Logger.AddInformation("Removing custom header from config file data. This SEB version cannot process this header type and will ignore it.");

					byte[] customHeaderData = GetPrefixDataFromData(ref sebData, customHeaderLength);

					Logger.AddInformation("Custom header data: " + customHeaderData);
				}
				catch (Exception ex)
				{
					Logger.AddError("Error while removing custom header from config file data", null, ex, ex.Message);
				}
			}

			// Prefix = pksh ("Public-Symmetric Key Hash") ?

			if (prefixString.CompareTo(PUBLIC_SYMMETRIC_KEY_MODE) == 0)
			{

				// Decrypt with cryptographic identity/private and symmetric key
				sebData = DecryptDataWithPublicKeyHashPrefix(sebData, true, forEditing, ref sebFileCertificateRef);
				if (sebData == null)
				{
					return null;
				}

				// Get 4-char prefix again
				// and remaining data without prefix, which is either plain or still encoded with password
				prefixString = GetPrefixStringFromData(ref sebData);
			}

			// Prefix = pkhs ("Public Key Hash") ?

			if (prefixString.CompareTo(PUBLIC_KEY_HASH_MODE) == 0)
			{

				// Decrypt with cryptographic identity/private key
				sebData = DecryptDataWithPublicKeyHashPrefix(sebData, false, forEditing, ref sebFileCertificateRef);
				if (sebData == null)
				{
					return null;
				}

				// Get 4-char prefix again
				// and remaining data without prefix, which is either plain or still encoded with password
				prefixString = GetPrefixStringFromData(ref sebData);
			}

			// Prefix = pswd ("Password") ?

			if (prefixString.CompareTo(PASSWORD_MODE) == 0)
			{

				// Decrypt with password
				// if the user enters the right one
				byte[] sebDataDecrypted = null;
				string password;
				// Allow up to 5 attempts for entering decoding password
				string enterPasswordString = SEBUIStrings.enterPassword;
				int i = 5;
				do
				{
					i--;
					// Prompt for password
					password = SebPasswordDialogForm.ShowPasswordDialogForm(SEBUIStrings.loadingSettings, enterPasswordString);
					if (password == null) return null;
					//error = nil;
					sebDataDecrypted = SEBProtectionController.DecryptDataWithPassword(sebData, password);
					enterPasswordString = SEBUIStrings.enterPasswordAgain;
					// in case we get an error we allow the user to try it again
				} while ((sebDataDecrypted == null) && i > 0);
				if (sebDataDecrypted == null)
				{
					//wrong password entered in 5th try: stop reading .seb file
					MessageBox.Show(SEBUIStrings.decryptingSettingsFailedReason, SEBUIStrings.decryptingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return null;
				}
				sebData = sebDataDecrypted;
				// If these settings are being decrypted for editing, we return the decryption password
				if (forEditing) sebFilePassword = password;
			}
			else
			{

				// Prefix = pwcc ("Password Configuring Client") ?

				if (prefixString.CompareTo(PASSWORD_CONFIGURING_CLIENT_MODE) == 0)
				{

					// Decrypt with password and configure local client settings
					// and quit afterwards, returning if reading the .seb file was successfull
					DictObj sebSettings = DecryptDataWithPasswordForConfiguringClient(sebData, forEditing, ref sebFilePassword, ref passwordIsHash);
					return sebSettings;

				}
				else
				{

					// Prefix = plnd ("Plain Data") ?

					if (prefixString.CompareTo(PLAIN_DATA_MODE) != 0)
					{
						// No valid 4-char prefix was found in the .seb file
						// Check if .seb file is unencrypted
						if (prefixString.CompareTo(UNENCRYPTED_MODE) == 0)
						{
							// .seb file seems to be an unencrypted XML plist
							// get the original data including the first 4 bytes
							sebData = sebDataUnencrypted;
						}
						else
						{
							// No valid prefix and no unencrypted file with valid header
							// cancel reading .seb file
							if (!suppressFileFormatError)
							{
								MessageBox.Show(SEBUIStrings.settingsNotUsableReason, SEBUIStrings.settingsNotUsable, MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
							return null;
						}
					}
				}
			}
			// If we don't deal with an unencrypted seb file
			// ungzip the .seb (according to specification >= v14) decrypted serialized XML plist data
			if (prefixString.CompareTo(UNENCRYPTED_MODE) != 0)
			{
				sebData = GZipByte.Decompress(sebData);
			}

			// Get preferences dictionary from decrypted data
			DictObj sebPreferencesDict = GetPreferencesDictFromConfigData(sebData, forEditing);
			DictObj sebPreferencesDictOriginal = GetPreferencesDictFromConfigData(sebData, false);
			// If we didn't get a preferences dict back, we abort reading settings
			if (sebPreferencesDict == null) return null;

			// We need to set the right value for the key sebConfigPurpose to know later where to store the new settings
			sebPreferencesDict[SEBSettings.KeySebConfigPurpose] = (int)SEBSettings.sebConfigPurposes.sebConfigPurposeStartingExam;
			sebPreferencesDictOriginal[SEBSettings.KeySebConfigPurpose] = (int)SEBSettings.sebConfigPurposes.sebConfigPurposeStartingExam;

			SEBSettings.settingsCurrentOriginal = sebPreferencesDictOriginal;

			// Reading preferences was successful!
			return sebPreferencesDict;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method which decrypts the byte array using an empty password, 
		/// or the administrator password currently set in SEB 
		/// or asks for the password used for encrypting this SEB file
		/// for configuring the client 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private static DictObj DecryptDataWithPasswordForConfiguringClient(byte[] sebData, bool forEditing, ref string sebFilePassword, ref bool passwordIsHash)
		{
			passwordIsHash = false;
			string password;
			// First try to decrypt with the current admin password
			// get admin password hash
			string hashedAdminPassword = (string)SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, SEBSettings.KeyHashedAdminPassword);
			if (hashedAdminPassword == null)
			{
				hashedAdminPassword = "";
			}
			// We use always uppercase letters in the base16 hashed admin password used for encrypting
			hashedAdminPassword = hashedAdminPassword.ToUpper();
			DictObj sebPreferencesDict = null;
			DictObj sebPreferencesDictOriginal = null;
			byte[] decryptedSebData = SEBProtectionController.DecryptDataWithPassword(sebData, hashedAdminPassword);
			if (decryptedSebData == null)
			{
				// If decryption with admin password didn't work, try it with an empty password
				decryptedSebData = SEBProtectionController.DecryptDataWithPassword(sebData, "");
				if (decryptedSebData == null)
				{
					// If decryption with empty and admin password didn't work, ask for the password the .seb file was encrypted with
					// Allow up to 5 attempts for entering decoding password
					int i = 5;
					password = null;
					string enterPasswordString = SEBUIStrings.enterEncryptionPassword;
					do
					{
						i--;
						// Prompt for password
						password = SebPasswordDialogForm.ShowPasswordDialogForm(SEBUIStrings.reconfiguringLocalSettings, enterPasswordString);
						// If cancel was pressed, abort
						if (password == null) return null;
						string hashedPassword = SEBProtectionController.ComputePasswordHash(password);
						// we try to decrypt with the hashed password
						decryptedSebData = SEBProtectionController.DecryptDataWithPassword(sebData, hashedPassword);
						// in case we get an error we allow the user to try it again
						enterPasswordString = SEBUIStrings.enterEncryptionPasswordAgain;
					} while (decryptedSebData == null && i > 0);
					if (decryptedSebData == null)
					{
						//wrong password entered in 5th try: stop reading .seb file
						MessageBox.Show(SEBUIStrings.reconfiguringLocalSettingsFailedWrongPassword, SEBUIStrings.reconfiguringLocalSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
						return null;
					}
					else
					{
						// Decrypting with entered password worked: We save it for returning it later
						if (forEditing) sebFilePassword = password;
					}
				}
			}
			else
			{
				//decrypting with hashedAdminPassword worked: we save it for returning as decryption password 
				sebFilePassword = hashedAdminPassword;
				// identify that password as hash
				passwordIsHash = true;
			}
			/// Decryption worked
			
			// Ungzip the .seb (according to specification >= v14) decrypted serialized XML plist data
			decryptedSebData = GZipByte.Decompress(decryptedSebData);

			// Check if the openend reconfiguring seb file has the same admin password inside like the current one

			try
			{
				sebPreferencesDict = (DictObj)Plist.readPlist(decryptedSebData);
				sebPreferencesDictOriginal = (DictObj)Plist.readPlist(decryptedSebData);
			}
			catch (Exception readPlistException)
			{
				// Error when deserializing the decrypted configuration data
				// We abort reading the new settings here
				MessageBox.Show(SEBUIStrings.loadingSettingsFailedReason, SEBUIStrings.loadingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Console.WriteLine(readPlistException.Message);
				return null;
			}
			// Get the admin password set in these settings
			string sebFileHashedAdminPassword = (string)SEBSettings.valueForDictionaryKey(sebPreferencesDict, SEBSettings.KeyHashedAdminPassword);
			if (sebFileHashedAdminPassword == null)
			{
				sebFileHashedAdminPassword = "";
			}
			// Has the SEB config file the same admin password inside as the current settings have?
			if (String.Compare(hashedAdminPassword, sebFileHashedAdminPassword, StringComparison.OrdinalIgnoreCase) != 0)
			{
				//No: The admin password inside the .seb file wasn't the same as the current one
				if (forEditing)
				{
					// If the file is openend for editing (and not to reconfigure SEB)
					// we have to ask the user for the admin password inside the file
					if (!askForPasswordAndCompareToHashedPassword(sebFileHashedAdminPassword, forEditing))
					{
						// If the user didn't enter the right password we abort
						return null;
					}
				}
				else
				{
					// The file was actually opened for reconfiguring the SEB client:
					// we have to ask for the current admin password and
					// allow reconfiguring only if the user enters the right one
					// We don't check this for the case the current admin password was used to encrypt the new settings
					// In this case there can be a new admin pw defined in the new settings and users don't need to enter the old one
					if (passwordIsHash == false && hashedAdminPassword.Length > 0)
					{
						// Allow up to 5 attempts for entering current admin password
						int i = 5;
						password = null;
						string hashedPassword;
						string enterPasswordString = SEBUIStrings.enterCurrentAdminPwdForReconfiguring;
						bool passwordsMatch;
						do
						{
							i--;
							// Prompt for password
							password = SebPasswordDialogForm.ShowPasswordDialogForm(SEBUIStrings.reconfiguringLocalSettings, enterPasswordString);
							// If cancel was pressed, abort
							if (password == null) return null;
							if (password.Length == 0)
							{
								hashedPassword = "";
							}
							else
							{
								hashedPassword = SEBProtectionController.ComputePasswordHash(password);
							}
							passwordsMatch = (String.Compare(hashedPassword, hashedAdminPassword, StringComparison.OrdinalIgnoreCase) == 0);
							// in case we get an error we allow the user to try it again
							enterPasswordString = SEBUIStrings.enterCurrentAdminPwdForReconfiguringAgain;
						} while (!passwordsMatch && i > 0);
						if (!passwordsMatch)
						{
							//wrong password entered in 5th try: stop reading .seb file
							MessageBox.Show(SEBUIStrings.reconfiguringLocalSettingsFailedWrongCurrentAdminPwd, SEBUIStrings.reconfiguringLocalSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
							return null;
						}
					}
				}
			}

			// We need to set the right value for the key sebConfigPurpose to know later where to store the new settings
			sebPreferencesDict[SEBSettings.KeySebConfigPurpose] = (int)SEBSettings.sebConfigPurposes.sebConfigPurposeConfiguringClient;
			sebPreferencesDictOriginal[SEBSettings.KeySebConfigPurpose] = (int)SEBSettings.sebConfigPurposes.sebConfigPurposeConfiguringClient;

			SEBSettings.settingsCurrentOriginal = sebPreferencesDictOriginal;

			// Reading preferences was successful!
			return sebPreferencesDict;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method: Get preferences dictionary from decrypted data.
		/// In editing mode, users have to enter the right SEB administrator password 
		/// before they can access the settings contents
		/// and returns the decrypted bytes 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private static DictObj GetPreferencesDictFromConfigData(byte[] sebData, bool forEditing)
		{
			DictObj sebPreferencesDict = null;
			try
			{
				// Get preferences dictionary from decrypted data
				sebPreferencesDict = (DictObj)Plist.readPlist(sebData);
			}
			catch (Exception readPlistException)
			{
				MessageBox.Show(SEBUIStrings.loadingSettingsFailedReason, SEBUIStrings.loadingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Console.WriteLine(readPlistException.Message);
				return null;
			}
			// In editing mode, the user has to enter the right SEB administrator password used in those settings before he can access their contents
			if (forEditing)
			{
				// Get the admin password set in these settings
				string sebFileHashedAdminPassword = (string)SEBSettings.valueForDictionaryKey(sebPreferencesDict, SEBSettings.KeyHashedAdminPassword);
				// If there was no or empty admin password set in these settings, the user can access them anyways
				if (!String.IsNullOrEmpty(sebFileHashedAdminPassword))
				{
					// Get the current hashed admin password
					string hashedAdminPassword = (string)SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, SEBSettings.KeyHashedAdminPassword);
					if (hashedAdminPassword == null)
					{
						hashedAdminPassword = "";
					}
					// If the current hashed admin password is same as the hashed admin password from the settings file
					// then the user is allowed to access the settings
					if (String.Compare(hashedAdminPassword, sebFileHashedAdminPassword, StringComparison.OrdinalIgnoreCase) != 0)
					{
						// otherwise we have to ask for the SEB administrator password used in those settings and
						// allow opening settings only if the user enters the right one

						if (!askForPasswordAndCompareToHashedPassword(sebFileHashedAdminPassword, forEditing))
						{
							return null;
						}
					}
				}
			}
			// Reading preferences was successful!
			return sebPreferencesDict;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Ask user to enter password and compare it to the passed (hashed) password string 
		/// Returns true if correct password was entered 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private static bool askForPasswordAndCompareToHashedPassword(string sebFileHashedAdminPassword, bool forEditing)
		{
			// Check if there wasn't a hashed password (= empty password)
			if (sebFileHashedAdminPassword.Length == 0) return true;
			// We have to ask for the SEB administrator password used in the settings 
			// and allow opening settings only if the user enters the right one
			// Allow up to 5 attempts for entering  admin password
			int i = 5;
			string password = null;
			string hashedPassword;
			string enterPasswordString = SEBUIStrings.enterAdminPasswordRequired;
			bool passwordsMatch;
			do
			{
				i--;
				// Prompt for password
				password = SebPasswordDialogForm.ShowPasswordDialogForm(SEBUIStrings.loadingSettings + (String.IsNullOrEmpty(SEBClientInfo.LoadingSettingsFileName) ? "" : ": " + SEBClientInfo.LoadingSettingsFileName), enterPasswordString);
				// If cancel was pressed, abort
				if (password == null) return false;
				if (password.Length == 0)
				{
					hashedPassword = "";
				}
				else
				{
					hashedPassword = SEBProtectionController.ComputePasswordHash(password);
				}
				passwordsMatch = (String.Compare(hashedPassword, sebFileHashedAdminPassword, StringComparison.OrdinalIgnoreCase) == 0);
				// in case we get an error we allow the user to try it again
				enterPasswordString = SEBUIStrings.enterAdminPasswordRequiredAgain;
			} while ((password == null || !passwordsMatch) && i > 0);
			if (!passwordsMatch)
			{
				//wrong password entered in 5th try: stop reading .seb file
				MessageBox.Show(SEBUIStrings.loadingSettingsFailedWrongAdminPwd, SEBUIStrings.loadingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			// Right password entered
			return passwordsMatch;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method which fetches the public key hash from a byte array, 
		/// retrieves the according cryptographic identity from the certificate store
		/// and returns the decrypted bytes 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private static byte[] DecryptDataWithPublicKeyHashPrefix(byte[] sebData, bool usingSymmetricKey, bool forEditing, ref X509Certificate2 sebFileCertificateRef)
		{
			// Get 20 bytes public key hash prefix
			// and remaining data with the prefix stripped
			byte[] publicKeyHash = GetPrefixDataFromData(ref sebData, PUBLIC_KEY_HASH_LENGTH);

			X509Certificate2 certificateRef = SEBProtectionController.GetCertificateFromStore(publicKeyHash);
			if (certificateRef == null)
			{
				MessageBox.Show(SEBUIStrings.certificateNotFoundInStore, SEBUIStrings.errorDecryptingSettings, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			// If these settings are being decrypted for editing, we will return the decryption certificate reference
			// in the variable which was passed as reference when calling this method
			if (forEditing) sebFileCertificateRef = certificateRef;

			// Are we using the new identity certificate decryption with a symmetric key?
			if (usingSymmetricKey)
			{
				// Get length of the encrypted symmetric key
				Int32 encryptedSymmetricKeyLength = BitConverter.ToInt32(GetPrefixDataFromData(ref sebData, sizeof(Int32)), 0);
				// Get encrypted symmetric key
				byte[] encryptedSymmetricKey = GetPrefixDataFromData(ref sebData, encryptedSymmetricKeyLength);
				// Decrypt symmetric key
				byte[] symmetricKey = SEBProtectionController.DecryptDataWithCertificate(encryptedSymmetricKey, certificateRef);
				if (symmetricKey == null)
				{
					return null;
				}
				string symmetricKeyString = Convert.ToBase64String(symmetricKey);
				// Decrypt config file data using the symmetric key as password
				sebData = SEBProtectionController.DecryptDataWithPassword(sebData, symmetricKeyString);
			}
			else
			{
				sebData = SEBProtectionController.DecryptDataWithCertificate(sebData, certificateRef);
			}

			return sebData;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method for returning a prefix string (of PREFIX_LENGTH, currently 4 chars)
		/// from a data byte array which is returned without the stripped prefix
		/// </summary>
		/// ----------------------------------------------------------------------------------------

		public static string GetPrefixStringFromData(ref byte[] data)
		{
			string decryptedDataString = Encoding.UTF8.GetString(GetPrefixDataFromData(ref data, PREFIX_LENGTH));
			return decryptedDataString;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Helper method for stripping (and returning) a prefix byte array of prefixLength
		/// from a data byte array which is returned without the stripped prefix
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static byte[] GetPrefixDataFromData(ref byte[] data, int prefixLength)
		{
			// Get prefix with indicated length
			byte[] prefixData = new byte[prefixLength];
			Buffer.BlockCopy(data, 0, prefixData, 0, prefixLength);

			// Get data without the stripped prefix
			byte[] dataStrippedKey = new byte[data.Length - prefixLength];
			Buffer.BlockCopy(data, prefixLength, dataStrippedKey, 0, data.Length - prefixLength);
			data = dataStrippedKey;

			return prefixData;
		}


		///// ----------------------------------------------------------------------------------------
		///// <summary>
		///// Show SEB Password Dialog Form.
		///// </summary>
		///// ----------------------------------------------------------------------------------------
		//public static string ShowPasswordDialogForm(string title, string passwordRequestText)
		//{
		//    // Set the title of the dialog window
		//    sebPasswordDialogForm.Text = title;
		//    // Set the text of the dialog
		//    sebPasswordDialogForm.LabelText = passwordRequestText;
		//    sebPasswordDialogForm.txtSEBPassword.Focus();
		//    // If we are running in SebWindowsClient we need to activate it before showing the password dialog
		//    if (SEBClientInfo.SebWindowsClientForm != null) SebWindowsClientForm.SEBToForeground(); //SEBClientInfo.SebWindowsClientForm.Activate();
		//    // Show password dialog as a modal dialog and determine if DialogResult = OK.
		//    if (sebPasswordDialogForm.ShowDialog() == DialogResult.OK)
		//    {
		//        // Read the contents of testDialog's TextBox.
		//        string password = sebPasswordDialogForm.txtSEBPassword.Text;
		//        sebPasswordDialogForm.txtSEBPassword.Text = "";
		//        //sebPasswordDialogForm.txtSEBPassword.Focus();
		//        return password;
		//    }
		//    else
		//    {
		//        return null;
		//    }
		//}

		/// Generate Encrypted .seb Settings Data

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Read SEB settings from UserDefaults and encrypt them using provided security credentials
		/// </summary>
		/// ----------------------------------------------------------------------------------------

		public static byte[] EncryptSEBSettingsWithCredentials(string settingsPassword, bool passwordIsHash, X509Certificate2 certificateRef, bool useAsymmetricOnlyEncryption, SEBSettings.sebConfigPurposes configPurpose, bool forEditing)
		{
			// Get current settings dictionary and clean it from empty arrays and dictionaries
			//DictObj cleanedCurrentSettings = SEBSettings.CleanSettingsDictionary();

			SEBSettings.settingsCurrentOriginal = SEBSettings.settingsCurrent;

			// Serialize preferences dictionary to an XML string
			string sebXML = Plist.writeXml(SEBSettings.settingsCurrent);
			string cleanedSebXML = sebXML.Replace("<array />", "<array></array>");
			cleanedSebXML = cleanedSebXML.Replace("<dict />", "<dict></dict>");
			cleanedSebXML = cleanedSebXML.Replace("<data />", "<data></data>");

			byte[] encryptedSebData = Encoding.UTF8.GetBytes(cleanedSebXML);

			string encryptingPassword = null;

			// Check for special case: .seb configures client, empty password
			if (String.IsNullOrEmpty(settingsPassword) && configPurpose == SEBSettings.sebConfigPurposes.sebConfigPurposeConfiguringClient)
			{
				encryptingPassword = "";
			}
			else
			{
				// in all other cases:
				// Check if no password entered and no identity selected
				if (String.IsNullOrEmpty(settingsPassword) && certificateRef == null)
				{
					if (MessageBox.Show(SEBUIStrings.noEncryptionChosenSaveUnencrypted, SEBUIStrings.noEncryptionChosen, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						// OK: save .seb config data unencrypted
						return encryptedSebData;
					}
					else
					{
						return null;
					}
				}
			}
			// gzip the serialized XML data
			encryptedSebData = GZipByte.Compress(encryptedSebData);

			// Check if password for encryption is provided and use it then
			if (!String.IsNullOrEmpty(settingsPassword))
			{
				encryptingPassword = settingsPassword;
			}
			// So if password is empty (special case) or provided
			if (!(encryptingPassword == null))
			{
				// encrypt with password
				encryptedSebData = EncryptDataUsingPassword(encryptedSebData, encryptingPassword, passwordIsHash, configPurpose);
			}
			else
			{
				// Create byte array large enough to hold prefix and data
				byte[] encryptedData = new byte[encryptedSebData.Length + PREFIX_LENGTH];

				// if no encryption with password: Add a 4-char prefix identifying plain data
				string prefixString = PLAIN_DATA_MODE;
				Buffer.BlockCopy(Encoding.UTF8.GetBytes(prefixString), 0, encryptedData, 0, PREFIX_LENGTH);
				// append plain data
				Buffer.BlockCopy(encryptedSebData, 0, encryptedData, PREFIX_LENGTH, encryptedSebData.Length);
				encryptedSebData = (byte[])encryptedData.Clone();
			}
			// Check if cryptographic identity for encryption is selected
			if (certificateRef != null)
			{
				// Encrypt preferences using a cryptographic identity
				encryptedSebData = EncryptDataUsingIdentity(encryptedSebData, certificateRef, useAsymmetricOnlyEncryption);
			}

			// gzip the encrypted data
			encryptedSebData = GZipByte.Compress(encryptedSebData);

			return encryptedSebData;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Encrypt preferences using a certificate
		/// </summary>
		/// ----------------------------------------------------------------------------------------

		public static byte[] EncryptDataUsingIdentity(byte[] data, X509Certificate2 certificateRef, bool useAsymmetricOnlyEncryption)
		{
			// Get public key hash from selected identity's certificate
			string prefixString;
			byte[] publicKeyHash = SEBProtectionController.GetPublicKeyHashFromCertificate(certificateRef);
			byte[] encryptedData;
			byte[] encryptedKeyLengthBytes = new byte[0];
			byte[] encryptedKey = new byte[0];
			byte[] encryptedSEBConfigData;

			if (!useAsymmetricOnlyEncryption)
			{
				prefixString = PUBLIC_SYMMETRIC_KEY_MODE;

				// For new asymmetric/symmetric encryption create a random symmetric key
				byte[] symmetricKey = AESThenHMAC.NewKey();
				string symmetricKeyString = Convert.ToBase64String(symmetricKey);

				// Encrypt the symmetric key using the identity certificate
				encryptedKey = SEBProtectionController.EncryptDataWithCertificate(symmetricKey, certificateRef);

				// Get length of the encrypted key
				encryptedKeyLengthBytes = BitConverter.GetBytes(encryptedKey.Length);

				//encrypt data using symmetric key
				encryptedData = SEBProtectionController.EncryptDataWithPassword(data, symmetricKeyString);
			}
			else
			{
				prefixString = PUBLIC_KEY_HASH_MODE;

				//encrypt data using public key
				encryptedData = SEBProtectionController.EncryptDataWithCertificate(data, certificateRef);
			}

			// Create byte array large enough to hold prefix, public key hash, length of and encrypted symmetric key plus encrypted data
			encryptedSEBConfigData = new byte[PREFIX_LENGTH + publicKeyHash.Length + encryptedKeyLengthBytes.Length + encryptedKey.Length + encryptedData.Length];
			int destinationOffset = 0;

			// Copy prefix indicating data has been encrypted with a public key identified by hash into out data
			Buffer.BlockCopy(Encoding.UTF8.GetBytes(prefixString), 0, encryptedSEBConfigData, destinationOffset, PREFIX_LENGTH);
			destinationOffset += PREFIX_LENGTH;

			// Copy public key hash to out data
			Buffer.BlockCopy(publicKeyHash, 0, encryptedSEBConfigData, destinationOffset, publicKeyHash.Length);
			destinationOffset += publicKeyHash.Length;

			// Copy length of encrypted symmetric key to out data
			Buffer.BlockCopy(encryptedKeyLengthBytes, 0, encryptedSEBConfigData, destinationOffset, encryptedKeyLengthBytes.Length);
			destinationOffset += encryptedKeyLengthBytes.Length;

			// Copy encrypted symmetric key to out data
			Buffer.BlockCopy(encryptedKey, 0, encryptedSEBConfigData, destinationOffset, encryptedKey.Length);
			destinationOffset += encryptedKey.Length;

			// Copy encrypted data to out data
			Buffer.BlockCopy(encryptedData, 0, encryptedSEBConfigData, destinationOffset, encryptedData.Length);

			return encryptedSEBConfigData;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Encrypt preferences using a password
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		// Encrypt preferences using a password
		public static byte[] EncryptDataUsingPassword(byte[] data, string password, bool passwordIsHash, SEBSettings.sebConfigPurposes configPurpose)
		{
			string prefixString;
			// Check if .seb file should start exam or configure client
			if (configPurpose == SEBSettings.sebConfigPurposes.sebConfigPurposeStartingExam)
			{
				// prefix string for starting exam: normal password will be prompted
				prefixString = PASSWORD_MODE;
			}
			else
			{
				// prefix string for configuring client: configuring password will either be hashed admin pw on client
				// or if no admin pw on client set: empty pw
				prefixString = PASSWORD_CONFIGURING_CLIENT_MODE;
				if (!String.IsNullOrEmpty(password) && !passwordIsHash)
				{
					//empty password means no admin pw on clients and should not be hashed
					//or we got already a hashed admin pw as settings pw, then we don't hash again
					password = SEBProtectionController.ComputePasswordHash(password);
				}
			}
			byte[] encryptedData = SEBProtectionController.EncryptDataWithPassword(data, password);
			// Create byte array large enough to hold prefix and data
			byte[] encryptedSebData = new byte[encryptedData.Length + PREFIX_LENGTH];
			Buffer.BlockCopy(Encoding.UTF8.GetBytes(prefixString), 0, encryptedSebData, 0, PREFIX_LENGTH);
			Buffer.BlockCopy(encryptedData, 0, encryptedSebData, PREFIX_LENGTH, encryptedData.Length);

			return encryptedSebData;
		}

	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Compressing and decompressing byte arrays using gzip
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public static class GZipByte
	{
		public static byte[] Compress(byte[] input)
		{
			using (MemoryStream output = new MemoryStream())
			{
				using (GZipStream zip = new GZipStream(output, CompressionMode.Compress))
				{
					zip.Write(input, 0, input.Length);
				}
				return output.ToArray();
			}
		}

		public static byte[] Decompress(byte[] input)
		{
			try
			{
				using (GZipStream stream = new GZipStream(new MemoryStream(input),
							  CompressionMode.Decompress))
				{
					const int size = 4096;
					byte[] buffer = new byte[size];
					using (MemoryStream output = new MemoryStream())
					{
						int count = 0;
						do
						{
							count = stream.Read(buffer, 0, size);
							if (count > 0)
							{
								output.Write(buffer, 0, count);
							}
						}
						while (count > 0);
						return output.ToArray();
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Show SEB Password Dialog Form.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		//public static string ShowPasswordDialogForm(string title, string passwordRequestText)
		//{
		//    Thread sf= new Thread(new ThreadStart(SebPasswordDialogForm.ShowPasswordDialogForm);
		//    sf.Start();

		//}

	}
}

