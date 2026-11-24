/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using static SafeExamBrowser.WindowsApi.Msctf;
using TSF = SafeExamBrowser.WindowsApi.Constants.TextServicesFramework;

namespace SafeExamBrowser.WindowsApi.TextServices
{
	public class TextService : ITextService
	{
		private const string KEYBOARD_LAYOUTS_KEY = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
		private const string USER_PROFILE_KEY = @"Control Panel\International\User Profile";

		private static readonly Guid CLSID_TF_InputProcessorProfiles = new Guid("33C53A50-F456-4884-B049-85FD643ECFED");
		private static readonly Guid CLSID_TF_ThreadMgr = new Guid("529A9E6B-6587-4F23-AB9E-9C7D683E3C50");
		private static readonly Guid IID_ITfInputProcessorProfileActivationSink = new Guid("71C6E74E-0F28-11D8-A82A-00065B84435C");

		private readonly object profiles;
		private readonly object threadManager;

		private ActivationSink sink;
		private uint sinkCookie;

		private ITfInputProcessorProfileMgr Manager => (ITfInputProcessorProfileMgr) profiles;
		private ITfInputProcessorProfiles Profiles => (ITfInputProcessorProfiles) profiles;
		private ITfSource Source => (ITfSource) threadManager;
		private ITfThreadMgr ThreadManager => (ITfThreadMgr) threadManager;

		public event InputProfileActivatedEventHandler ProfileActivated;

		public TextService()
		{
			profiles = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_TF_InputProcessorProfiles));
			threadManager = Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_TF_ThreadMgr));
		}

		public void ActivateProfile(IInputProfile profile)
		{
			var classId = profile.ClassId;
			var profileId = profile.ProfileId;

			Manager.ActivateProfile(profile.ProfileType, profile.LanguageId, ref classId, ref profileId, profile.Hkl, TSF.TF_IPPMF_DONTCARECURRENTINPUTLANGUAGE);
		}

		public void Dispose()
		{
			StopMonitoring();

			// The thread manager exists once per thread and is also used by WPF, while the CLR uses a single runtime
			// callable wrapper per COM object. Marshal.FinalReleaseComObject could thus also invalidate the wrapper used
			// by WPF, which is why only the reference of this instance is released.
			Marshal.ReleaseComObject(threadManager);
			Marshal.ReleaseComObject(profiles);
		}

		public string GetDescription(IInputProfile profile)
		{
			var classId = profile.ClassId;
			var profileId = profile.ProfileId;

			Profiles.GetLanguageProfileDescription(ref classId, profile.LanguageId, ref profileId, out var description);

			return description;
		}

		public string GetKeyboardLayoutName(IntPtr hkl)
		{
			var klid = GetKeyboardLayoutId(hkl);

			using (var layout = Registry.LocalMachine.OpenSubKey($@"{KEYBOARD_LAYOUTS_KEY}\{klid}"))
			{
				if (layout?.GetValue("Layout Display Name") is string resource && TryLoadIndirectString(resource, out var displayName))
				{
					return displayName;
				}

				if (layout?.GetValue("Layout Text") is string text && !string.IsNullOrWhiteSpace(text))
				{
					return text;
				}
			}

			return klid;
		}

		public IList<IInputProfile> GetProfiles()
		{
			var result = new List<IInputProfile>();

			Manager.EnumProfiles(0, out var enumerator);

			try
			{
				while (enumerator.Next(1, out var profile, out var fetched) == TSF.S_OK && fetched == 1)
				{
					result.Add(ToInputProfile(profile));
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(enumerator);
			}

			return result;
		}

		public void StartMonitoring()
		{
			if (sink == null)
			{
				var activationSink = new ActivationSink(p => ProfileActivated?.Invoke(p));
				var iid = IID_ITfInputProcessorProfileActivationSink;

				// The activation sink must be installed via the source of the thread manager, the source of the input
				// processor profiles rejects it with CONNECT_E_CANNOTCONNECT. TSF is activated for this thread to make
				// sure that notifications are delivered, as WPF only does so lazily. Activation is reference-counted per
				// thread, i.e. it doesn't interfere with WPF as long as every Activate is matched by a Deactivate.
				ThreadManager.Activate(out _);

				try
				{
					Source.AdviseSink(ref iid, activationSink, out sinkCookie);
					sink = activationSink;
				}
				catch
				{
					ThreadManager.Deactivate();
					throw;
				}
			}
		}

		public void StopMonitoring()
		{
			if (sink != null)
			{
				try
				{
					Source.UnadviseSink(sinkCookie);
				}
				finally
				{
					ThreadManager.Deactivate();
					sink = null;
					sinkCookie = 0;
				}
			}
		}

		public bool TryGetActiveProfile(out IInputProfile profile)
		{
			var category = TSF.GUID_TFCAT_TIP_KEYBOARD;
			var result = Manager.GetActiveProfile(ref category, out var native);
			var success = result == TSF.S_OK;

			profile = success ? ToInputProfile(native) : default;

			return success;
		}

		public bool TryGetCulture(ushort languageId, out CultureInfo culture)
		{
			try
			{
				var name = GetTransientLanguageName(languageId);

				culture = name != default ? CultureInfo.GetCultureInfo(name) : CultureInfo.GetCultureInfo(languageId);

				return true;
			}
			catch (CultureNotFoundException)
			{
				culture = default;

				return false;
			}
		}

		/// <summary>
		/// Retrieves the keyboard layout identifier (KLID) for the given keyboard layout handle (HKL), i.e. the name of the
		/// registry key below <c>HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layouts</c>. This replicates the logic of
		/// <c>System.Windows.Forms.InputLanguage.LayoutId</c> of .NET 8, since the .NET Framework implementation resolves
		/// the HKL via a heuristic on <c>HKCU\Keyboard Layout\Substitutes</c> which yields the same (wrong) name for all
		/// additional layouts of an input language.
		/// </summary>
		private string GetKeyboardLayoutId(IntPtr hkl)
		{
			// The HKL is a 32-bit value even on 64-bit systems: the high word identifies the keyboard layout (the
			// "device"), the low word identifies the input language (e.g. 0x04010820 for the "Arabic (101)" layout
			// 0x0401 under the input language "Urdu (India)" 0x0820).
			var handle = unchecked((int) (long) hkl);
			var language = handle & 0xFFFF;
			var device = (handle >> 16) & 0xFFFF;

			if ((device & 0xF000) == 0xF000)
			{
				// Keyboard layouts which are not the default layout of their language (e.g. "Hindi Traditional" with KLID
				// 00010439) are identified by their "Layout Id" (with the high nibble set to 0xF) instead of their KLID.
				var layoutId = device & 0x0FFF;

				using (var layouts = Registry.LocalMachine.OpenSubKey(KEYBOARD_LAYOUTS_KEY))
				{
					foreach (var name in layouts?.GetSubKeyNames() ?? new string[0])
					{
						using (var layout = layouts.OpenSubKey(name))
						{
							if (layout?.GetValue("Layout Id") is string value && Convert.ToInt32(value, 16) == layoutId)
							{
								return name.ToUpperInvariant();
							}
						}
					}
				}
			}
			else if ((device & 0xF000) == 0xE000)
			{
				// Legacy input method editors (e.g. E0010411) are registered with their complete HKL as KLID.
				return handle.ToString("X8");
			}
			else if (device == 0)
			{
				// The default layout of a language may be identified by the language only (e.g. 0x00000409).
				device = language;
			}

			return device.ToString("X8");
		}

		private string GetTransientLanguageName(int languageId)
		{
			const int LOCALE_TRANSIENT_KEYBOARD1 = 0x2000;
			const int LOCALE_TRANSIENT_KEYBOARD2 = 0x2400;
			const int LOCALE_TRANSIENT_KEYBOARD3 = 0x2800;
			const int LOCALE_TRANSIENT_KEYBOARD4 = 0x2C00;

			var isTransient = languageId == LOCALE_TRANSIENT_KEYBOARD1
				|| languageId == LOCALE_TRANSIENT_KEYBOARD2
				|| languageId == LOCALE_TRANSIENT_KEYBOARD3
				|| languageId == LOCALE_TRANSIENT_KEYBOARD4;

			if (isTransient)
			{
				// Languages without a language identifier of their own (e.g. "jv-Java") are assigned one of the transient
				// identifiers, the assignment is stored in the user profile.
				using (var key = Registry.CurrentUser.OpenSubKey(USER_PROFILE_KEY))
				{
					foreach (var language in key?.GetValue("Languages") as string[] ?? new string[0])
					{
						using (var subKey = key.OpenSubKey(language))
						{
							if (subKey?.GetValue("TransientLangId") is int transientId && transientId == languageId)
							{
								return language;
							}
						}
					}
				}
			}

			return default;
		}

		private InputProfile ToInputProfile(TF_INPUTPROCESSORPROFILE profile)
		{
			var isTextService = profile.dwProfileType == TSF.TF_PROFILETYPE_INPUTPROCESSOR;

			return new InputProfile
			{
				ClassId = profile.clsid,
				Hkl = profile.hkl,
				IsActive = (profile.dwFlags & TSF.TF_IPP_FLAG_ACTIVE) == TSF.TF_IPP_FLAG_ACTIVE,
				IsEnabled = (profile.dwFlags & TSF.TF_IPP_FLAG_ENABLED) == TSF.TF_IPP_FLAG_ENABLED,
				IsKeyboard = !isTextService || profile.catid == TSF.GUID_TFCAT_TIP_KEYBOARD,
				IsSubstituted = (profile.dwFlags & TSF.TF_IPP_FLAG_SUBSTITUTEDBYINPUTPROCESSOR) == TSF.TF_IPP_FLAG_SUBSTITUTEDBYINPUTPROCESSOR,
				LanguageId = profile.langid,
				ProfileId = profile.guidProfile,
				ProfileType = profile.dwProfileType
			};
		}

		private bool TryLoadIndirectString(string resource, out string value)
		{
			var buffer = new StringBuilder(1024);
			var success = Shlwapi.SHLoadIndirectString(resource, buffer, buffer.Capacity, IntPtr.Zero) == TSF.S_OK && buffer.Length > 0;

			value = success ? buffer.ToString() : default;

			return success;
		}
	}
}
