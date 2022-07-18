/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Integrity
{
	public class IntegrityModule : IIntegrityModule
	{
		const string DLL_NAME =
#if X86
		"seb_x86.dll";
#else
		"seb_x64.dll";
#endif

		private readonly ILogger logger;

		public IntegrityModule(ILogger logger)
		{
			this.logger = logger;
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
				logger.Warn("Integrity module is not present!");
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
				logger.Warn("Integrity module is not present!");
			}
			catch (Exception e)
			{
				logger.Error("Unexpected error while attempting to verify code signature!", e);
			}

			return success;
		}

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.BStr)]
		private static extern string CalculateBrowserExamKey(string configurationKey, string salt);

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool VerifyCodeSignature();
	}
}
