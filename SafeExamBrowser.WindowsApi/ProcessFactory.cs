/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class ProcessFactory : IProcessFactory
	{
		private ILogger logger;

		public IDesktop StartupDesktop { private get; set; }

		public ProcessFactory(ILogger logger)
		{
			this.logger = logger;
		}

		public IProcess StartNew(string path, params string[] args)
		{
			var commandLine = $"{'"' + path + '"'} {String.Join(" ", args)}";
			var processInfo = new PROCESS_INFORMATION();
			var startupInfo = new STARTUPINFO();

			startupInfo.cb = Marshal.SizeOf(startupInfo);
			startupInfo.lpDesktop = StartupDesktop?.Name;

			logger.Info($"Attempting to start process '{path}'...");

			var success = Kernel32.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, Constant.NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref startupInfo, ref processInfo);

			if (success)
			{
				logger.Info($"Successfully started process '{Path.GetFileName(path)}' with ID {processInfo.dwProcessId}.");
			}
			else
			{
				logger.Error($"Failed to start process '{Path.GetFileName(path)}'!");

				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			return new Process(processInfo.dwProcessId);
		}
	}
}
