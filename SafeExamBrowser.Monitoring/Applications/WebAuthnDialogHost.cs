/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Applications
{
	/// <summary>
	/// Identifies the operating system process which hosts the native WebAuthn user interface of Windows. This is the "Windows Security" dialog
	/// shown whenever a website requests a passkey, regardless of the chosen method (Windows Hello, FIDO2 security key, phone via QR code).
	/// Without it, signing in with a passkey (e.g. on the Microsoft login page) is impossible, as the browser engine delegates the whole
	/// interaction to this separate window.
	/// </summary>
	/// <remarks>
	/// A process is only trusted if it is the genuine system executable. The name alone is not sufficient (any user may copy and rename an
	/// executable), which is why the full path is verified as well: the <c>System32</c> directory can only be modified by privileged users.
	/// </remarks>
	internal static class WebAuthnDialogHost
	{
		private const string EXECUTABLE_NAME = "CredentialUIBroker.exe";
		private const string SYSTEM_DIRECTORY_NAME = "System32";
		private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

		/// <summary>
		/// Determines whether the given process is the genuine Windows process hosting the native WebAuthn user interface.
		/// </summary>
		internal static bool IsHostedBy(IProcess process)
		{
			if (process == default)
			{
				return false;
			}

			// The path provided by WMI is empty for processes of other users and services if SEB is not run as administrator.
			var path = string.IsNullOrWhiteSpace(process.Path) ? TryGetImagePath(process.Id) : process.Path;

			return IsGenuine(process.Name, path, Environment.GetFolderPath(Environment.SpecialFolder.Windows));
		}

		/// <summary>
		/// Determines whether the given executable name and path match the genuine Windows process hosting the native WebAuthn user interface,
		/// which must be located in the <c>System32</c> directory of the given Windows directory.
		/// </summary>
		internal static bool IsGenuine(string name, string path, string windowsDirectory)
		{
			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(windowsDirectory))
			{
				return false;
			}

			if (!string.Equals(name, EXECUTABLE_NAME, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			try
			{
				var expected = Path.GetFullPath(Path.Combine(windowsDirectory, SYSTEM_DIRECTORY_NAME, EXECUTABLE_NAME));
				var actual = Path.GetFullPath(path);

				return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception e) when (e is ArgumentException || e is NotSupportedException || e is PathTooLongException)
			{
				return false;
			}
		}

		private static string TryGetImagePath(int processId)
		{
			var handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);

			if (handle == IntPtr.Zero)
			{
				return default;
			}

			try
			{
				var path = new StringBuilder(1024);
				var size = path.Capacity;

				return QueryFullProcessImageName(handle, 0, path, ref size) ? path.ToString() : default;
			}
			finally
			{
				CloseHandle(handle);
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint desiredAccess, bool inheritHandle, int processId);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool QueryFullProcessImageName(IntPtr process, uint flags, StringBuilder path, ref int size);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle);
	}
}
