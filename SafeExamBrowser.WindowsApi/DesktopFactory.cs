/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Constants;

namespace SafeExamBrowser.WindowsApi
{
	public class DesktopFactory : IDesktopFactory
	{
		private ILogger logger;

		public DesktopFactory(ILogger logger)
		{
			this.logger = logger;
		}

		public IDesktop CreateNew(string name)
		{
			logger.Debug($"Attempting to create new desktop '{name}'...");

			var handle = User32.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, (uint) AccessMask.GENERIC_ALL, IntPtr.Zero);

			if (handle == IntPtr.Zero)
			{
				logger.Error($"Failed to create new desktop '{name}'!");

				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			var desktop = new Desktop(handle, name);

			logger.Debug($"Successfully created desktop {desktop}.");

			return desktop;
		}

		public IDesktop GetCurrent()
		{
			var threadId = Kernel32.GetCurrentThreadId();
			var handle = User32.GetThreadDesktop(threadId);
			var name = String.Empty;
			var nameLength = 0;

			if (handle == IntPtr.Zero)
			{
				logger.Error($"Failed to get desktop handle for thread with ID = {threadId}!");

				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			logger.Debug($"Found desktop handle for thread with ID = {threadId}. Attempting to get desktop name...");

			User32.GetUserObjectInformation(handle, Constant.UOI_NAME, IntPtr.Zero, 0, ref nameLength);

			var namePointer = Marshal.AllocHGlobal(nameLength);
			var success = User32.GetUserObjectInformation(handle, Constant.UOI_NAME, namePointer, nameLength, ref nameLength);

			if (!success)
			{
				logger.Error($"Failed to retrieve name for desktop with handle = {handle}!");

				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			name = Marshal.PtrToStringAnsi(namePointer);
			Marshal.FreeHGlobal(namePointer);

			var desktop = new Desktop(handle, name);

			logger.Debug($"Successfully determined current desktop {desktop}.");

			return desktop;
		}
	}
}
