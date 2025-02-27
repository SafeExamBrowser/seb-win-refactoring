/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.WindowsApi.Desktops
{
	public class DesktopFactory : IDesktopFactory
	{
		private readonly ILogger logger;
		private readonly Random random;

		public DesktopFactory(ILogger logger)
		{
			this.logger = logger;
			this.random = new Random();
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

		public IDesktop CreateRandom()
		{
			logger.Debug($"Attempting to create random desktop...");

			var name = GenerateRandomDesktopName();
			var handle = User32.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, (uint) AccessMask.GENERIC_ALL, IntPtr.Zero);

			if (handle == IntPtr.Zero)
			{
				logger.Error($"Failed to create random desktop '{name}'!");

				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			var obfuscatedHandle = new IntPtr(random.Next(100, 10000));
			var obfuscatedName = GenerateRandomDesktopName();
			var desktop = new ObfuscatedDesktop(handle, name, obfuscatedHandle, obfuscatedName);

			logger.Debug($"Successfully created random desktop {desktop}.");

			return desktop;
		}

		public IDesktop GetCurrent()
		{
			var threadId = Kernel32.GetCurrentThreadId();
			var handle = User32.GetThreadDesktop(threadId);
			var name = string.Empty;
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

		private string GenerateRandomDesktopName()
		{
			var length = random.Next(5, 20);
			var name = new char[length];

			for (var letter = 0; letter < length; letter++)
			{
				name[letter] = (char) (random.Next(2) == 0 && letter != 0 ? random.Next('a', 'z' + 1) : random.Next('A', 'Z' + 1));
			}

			return new string(name);
		}
	}
}
