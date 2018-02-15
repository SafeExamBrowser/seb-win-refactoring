/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class ProcessFactory : IProcessFactory
	{
		private IDesktop desktop;
		private ILogger logger;

		public ProcessFactory(IDesktop desktop, ILogger logger)
		{
			this.desktop = desktop;
			this.logger = logger;
		}

		public IProcess StartNew(string path, params string[] args)
		{
			var processInfo = new PROCESS_INFORMATION();
			var pAttr = new SECURITY_ATTRIBUTES();
			var tAttr = new SECURITY_ATTRIBUTES();
			var startupInfo = new STARTUPINFO();

			startupInfo.cb = Marshal.SizeOf(startupInfo);
			startupInfo.lpDesktop = desktop.CurrentName;

			var success = Kernel32.CreateProcess(null, path, ref pAttr, ref tAttr, true, Constant.NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref startupInfo, out processInfo);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			return new Process(processInfo.dwProcessId);
		}
	}
}
