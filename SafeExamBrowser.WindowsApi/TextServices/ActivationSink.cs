/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SafeExamBrowser.WindowsApi.Constants;
using static SafeExamBrowser.WindowsApi.Msctf;

namespace SafeExamBrowser.WindowsApi.TextServices
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	internal class ActivationSink : ITfInputProcessorProfileActivationSink
	{
		private readonly Action<InputProfile> callback;

		internal ActivationSink(Action<InputProfile> callback)
		{
			this.callback = callback;
		}

		public int OnActivated(uint dwProfileType, ushort langid, ref Guid clsid, ref Guid catid, ref Guid guidProfile, IntPtr hkl, uint dwFlags)
		{
			if ((dwFlags & TextServicesFramework.TF_IPSINK_FLAG_ACTIVE) == TextServicesFramework.TF_IPSINK_FLAG_ACTIVE)
			{
				var isTextService = dwProfileType == TextServicesFramework.TF_PROFILETYPE_INPUTPROCESSOR;
				var profile = new InputProfile
				{
					ClassId = clsid,
					Hkl = hkl,
					IsActive = true,
					IsEnabled = true,
					IsKeyboard = !isTextService || catid == TextServicesFramework.GUID_TFCAT_TIP_KEYBOARD,
					LanguageId = langid,
					ProfileId = guidProfile,
					ProfileType = dwProfileType
				};

				callback(profile);
			}

			return TextServicesFramework.S_OK;
		}
	}
}
