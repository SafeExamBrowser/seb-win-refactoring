/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Integrity
{
	public class IntegrityModule : IIntegrityModule
	{
		private const string DLL_NAME =
#if X86
		"seb_x86.dll";
#else
		"seb_x64.dll";
#endif

		private static readonly byte[] SESSION_DATA_IV =
		{
			0x12, 0x07, 0x14, 0x02, 0x03, 0x10, 0x14, 0x18,
			0x11, 0x01, 0x04, 0x15, 0x06, 0x16, 0x05, 0x12
		};
		private static readonly byte[] SESSION_DATA_KEY =
		{
			0x01, 0x04, 0x07, 0x08, 0x09, 0x10, 0x13, 0x06,
			0x11, 0x14, 0x15, 0x16, 0x05, 0x03, 0x13, 0x06,
			0x01, 0x04, 0x02, 0x03, 0x14, 0x15, 0x07, 0x08,
			0x11, 0x12, 0x16, 0x05, 0x09, 0x10, 0x12, 0x02
		};
		private static readonly string SESSION_DATA_SEPARATOR = "<@|--separator--|@>";

		private readonly AppConfig appConfig;
		private readonly ILogger logger;

		public IntegrityModule(AppConfig appConfig, ILogger logger)
		{
			this.appConfig = appConfig;
			this.logger = logger;
		}

		public void CacheSession(string configurationKey, string startUrl)
		{
			if (TryReadSessionCache(out var sessions) && TryWriteSessionCache(sessions.Append((configurationKey, startUrl))))
			{
				logger.Debug("Successfully cached session.");
			}
			else
			{
				logger.Error("Failed to cache session!");
			}
		}

		public void ClearSession(string configurationKey, string startUrl)
		{
			if (TryReadSessionCache(out var sessions) && TryWriteSessionCache(sessions.Where(s => s.configurationKey != configurationKey && s.startUrl != startUrl)))
			{
				logger.Debug("Successfully cleared session.");
			}
			else
			{
				logger.Error("Failed to clear session!");
			}
		}

		public bool TryCalculateAppSignatureKey(string connectionToken, string salt, out string appSignatureKey)
		{
			appSignatureKey = default;

			try
			{
				appSignatureKey = CalculateAppSignatureKey(connectionToken, salt);
			}
			catch (DllNotFoundException)
			{
				logger.Warn("Integrity module is not available!");
			}
			catch (Exception e)
			{
				logger.Error("Unexpected error while attempting to calculate app signature key!", e);
			}

			return appSignatureKey != default;
		}

		public bool TryCalculateBrowserExamKey(string configurationKey, string salt, out string browserExamKey)
		{
			browserExamKey = default;

			try
			{
				browserExamKey = CalculateBrowserExamKey(configurationKey, salt);
			}
			catch (DllNotFoundException)
			{
				logger.Warn("Integrity module is not available!");
			}
			catch (Exception e)
			{
				logger.Error("Unexpected error while attempting to calculate browser exam key!", e);
			}

			return browserExamKey != default;
		}

		public bool TryVerifyCodeSignature(out bool isValid)
		{
			var success = false;

			isValid = default;

			try
			{
				isValid = VerifyCodeSignature();
				success = true;
			}
			catch (DllNotFoundException)
			{
				logger.Warn("Integrity module is not available!");
			}
			catch (Exception e)
			{
				logger.Error("Unexpected error while attempting to verify code signature!", e);
			}

			return success;
		}

		public bool TryVerifySessionIntegrity(string configurationKey, string startUrl, out bool isValid)
		{
			var success = false;

			isValid = false;

			if (TryReadSessionCache(out var sessions))
			{
				isValid = sessions.All(s => s.configurationKey != configurationKey && s.startUrl != startUrl);
				success = true;
				logger.Debug($"Successfully verified session integrity, session is {(isValid ? "valid." : "compromised!")}");
			}
			else
			{
				logger.Error("Failed to verify session integrity!");
			}

			return success;
		}

		private bool TryReadSessionCache(out IList<(string configurationKey, string startUrl)> sessions)
		{
			var success = false;

			sessions = new List<(string configurationKey, string startUrl)>();

			try
			{
				if (File.Exists(appConfig.SessionCacheFilePath))
				{
					using (var file = new FileStream(appConfig.SessionCacheFilePath, FileMode.Open))
					using (var aes = Aes.Create())
					using (var stream = new CryptoStream(file, aes.CreateDecryptor(SESSION_DATA_KEY, SESSION_DATA_IV), CryptoStreamMode.Read))
					using (var reader = new StreamReader(stream))
					{
						var line = reader.ReadLine();

						if (line != default)
						{
							var session = line.Split(new string[] { SESSION_DATA_SEPARATOR }, StringSplitOptions.None);
							var configurationKey = session[0];
							var startUrl = session[1];

							sessions.Add((configurationKey, startUrl));
						}
					}
				}

				success = true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to read session cache!", e);
			}

			return success;
		}

		private bool TryWriteSessionCache(IEnumerable<(string configurationKey, string startUrl)> sessions)
		{
			var success = false;

			try
			{
				if (sessions.Any())
				{
					using (var file = new FileStream(appConfig.SessionCacheFilePath, FileMode.Create))
					using (var aes = Aes.Create())
					{
						aes.Key = SESSION_DATA_KEY;
						aes.IV = SESSION_DATA_IV;

						using (var stream = new CryptoStream(file, aes.CreateEncryptor(), CryptoStreamMode.Write))
						using (var writer = new StreamWriter(stream))
						{
							foreach (var (configurationKey, startUrl) in sessions)
							{
								writer.WriteLine($"{configurationKey}{SESSION_DATA_SEPARATOR}{startUrl}");
							}
						}
					}
				}
				else
				{
					File.Delete(appConfig.SessionCacheFilePath);
				}

				success = true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to write session cache!", e);
			}

			return success;
		}

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.BStr)]
		private static extern string CalculateAppSignatureKey(string connectionToken, string salt);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.BStr)]
		private static extern string CalculateBrowserExamKey(string configurationKey, string salt);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool VerifyCodeSignature();
	}
}
