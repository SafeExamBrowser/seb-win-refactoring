/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace SafeExamBrowser.WindowsApi
{
	/// <summary>
	/// Provides access to the native Windows API exposed by <c>msctf.dll</c>.
	/// </summary>
	internal static class Msctf
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct TF_INPUTPROCESSORPROFILE
		{
			public uint dwProfileType;
			public ushort langid;
			public Guid clsid;
			public Guid guidProfile;
			public Guid catid;
			public IntPtr hklSubstitute;
			public uint dwCaps;
			public IntPtr hkl;
			public uint dwFlags;
		}

		[ComImport]
		[Guid("1F02B6C5-7842-4EE6-8A0B-9A24183A95CA")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ITfInputProcessorProfiles
		{
			// The members must be declared in the exact order of the v-table, all of them are required even if unused!
			void Register([In] ref Guid rclsid);
			void Unregister([In] ref Guid rclsid);
			void AddLanguageProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, [MarshalAs(UnmanagedType.LPWStr)] string pchDesc, uint cchDesc, [MarshalAs(UnmanagedType.LPWStr)] string pchIconFile, uint cchFile, uint uIconIndex);
			void RemoveLanguageProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile);
			void EnumInputProcessorInfo(out IntPtr ppEnum);
			void GetDefaultLanguageProfile(ushort langid, [In] ref Guid catid, out Guid pclsid, out Guid pguidProfile);
			void SetDefaultLanguageProfile(ushort langid, [In] ref Guid rclsid, [In] ref Guid guidProfiles);
			void ActivateLanguageProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfiles);
			void GetActiveLanguageProfile([In] ref Guid rclsid, out ushort plangid, out Guid pguidProfile);
			void GetLanguageProfileDescription([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, [MarshalAs(UnmanagedType.BStr)] out string pbstrProfile);
			void GetCurrentLanguage(out ushort plangid);
			void ChangeCurrentLanguage(ushort langid);
			void GetLanguageList(out IntPtr ppLangId, out uint pulCount);
			void EnumLanguageProfiles(ushort langid, out IntPtr ppEnum);
			void EnableLanguageProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, [MarshalAs(UnmanagedType.Bool)] bool fEnable);
			void IsEnabledLanguageProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, [MarshalAs(UnmanagedType.Bool)] out bool pfEnable);
			void EnableLanguageProfileByDefault([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, [MarshalAs(UnmanagedType.Bool)] bool fEnable);
			void SubstituteKeyboardLayout([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, IntPtr hKL);
		}

		[ComImport]
		[Guid("71C6E74C-0F28-11D8-A82A-00065B84435C")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ITfInputProcessorProfileMgr
		{
			void ActivateProfile(uint dwProfileType, ushort langid, [In] ref Guid clsid, [In] ref Guid guidProfile, IntPtr hkl, uint dwFlags);
			void DeactivateProfile(uint dwProfileType, ushort langid, [In] ref Guid clsid, [In] ref Guid guidProfile, IntPtr hkl, uint dwFlags);
			void GetProfile(uint dwProfileType, ushort langid, [In] ref Guid clsid, [In] ref Guid guidProfile, IntPtr hkl, out TF_INPUTPROCESSORPROFILE pProfile);
			void EnumProfiles(ushort langid, out IEnumTfInputProcessorProfiles ppEnum);
			void ReleaseInputProcessor([In] ref Guid rclsid, uint dwFlags);
			void RegisterProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, [MarshalAs(UnmanagedType.LPWStr)] string pchDesc, uint cchDesc, [MarshalAs(UnmanagedType.LPWStr)] string pchIconFile, uint cchFile, uint uIconIndex, IntPtr hklsubstitute, uint dwPreferredLayout, [MarshalAs(UnmanagedType.Bool)] bool bEnabledByDefault, uint dwFlags);
			void UnregisterProfile([In] ref Guid rclsid, ushort langid, [In] ref Guid guidProfile, uint dwFlags);
			[PreserveSig]
			int GetActiveProfile([In] ref Guid catid, out TF_INPUTPROCESSORPROFILE pProfile);
		}

		[ComImport]
		[Guid("71C6E74D-0F28-11D8-A82A-00065B84435C")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IEnumTfInputProcessorProfiles
		{
			void Clone(out IEnumTfInputProcessorProfiles ppEnum);
			[PreserveSig]
			int Next(uint ulCount, out TF_INPUTPROCESSORPROFILE pProfile, out uint pcFetch);
			void Reset();
			void Skip(uint ulCount);
		}

		[ComImport]
		[Guid("71C6E74E-0F28-11D8-A82A-00065B84435C")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ITfInputProcessorProfileActivationSink
		{
			[PreserveSig]
			int OnActivated(uint dwProfileType, ushort langid, [In] ref Guid clsid, [In] ref Guid catid, [In] ref Guid guidProfile, IntPtr hkl, uint dwFlags);
		}

		[ComImport]
		[Guid("4EA48A35-60AE-446F-8FD6-E6A8D82459F7")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ITfSource
		{
			void AdviseSink([In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] object punk, out uint pdwCookie);
			void UnadviseSink(uint dwCookie);
		}

		[ComImport]
		[Guid("AA80E801-2021-11D2-93E0-0060B067B86E")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ITfThreadMgr
		{
			// Only the first members of the v-table are declared, as omitting trailing members is safe (as opposed to omitting members which precede a used one).
			void Activate(out uint ptid);
			void Deactivate();
		}
	}
}
