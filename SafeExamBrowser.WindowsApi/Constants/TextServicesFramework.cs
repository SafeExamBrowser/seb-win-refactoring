/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See https://learn.microsoft.com/en-us/windows/win32/api/_tsf/.
	/// </remarks>
	internal static class TextServicesFramework
	{
		internal const int S_OK = 0;
		internal const uint TF_PROFILETYPE_INPUTPROCESSOR = 0x1;
		internal const uint TF_PROFILETYPE_KEYBOARDLAYOUT = 0x2;
		internal const uint TF_IPP_FLAG_ACTIVE = 0x1;
		internal const uint TF_IPP_FLAG_ENABLED = 0x2;
		internal const uint TF_IPP_FLAG_SUBSTITUTEDBYINPUTPROCESSOR = 0x4;
		internal const uint TF_IPPMF_DONTCARECURRENTINPUTLANGUAGE = 0x4;
		internal const uint TF_IPPMF_FORPROCESS = 0x10000000;
		internal const uint TF_IPSINK_FLAG_ACTIVE = 0x1;

		internal static readonly Guid GUID_TFCAT_TIP_KEYBOARD = new Guid("34745C63-B2F0-4784-8B67-5E12C8701A31");
	}
}
