/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class ProcessFactory : IProcessFactory
	{
		private IModuleLogger logger;

		public IDesktop StartupDesktop { private get; set; }

		public ProcessFactory(IModuleLogger logger)
		{
			this.logger = logger;
		}

		public IList<IProcess> GetAllRunning()
		{
			var processes = new List<IProcess>();
			var running = System.Diagnostics.Process.GetProcesses();
			var originalNames = LoadOriginalNames();

			foreach (var process in running)
			{
				var originalName = originalNames.FirstOrDefault(n => n.processId == process.Id).originalName;

				processes.Add(new Process(process, originalName, LoggerFor(process)));
			}

			return processes;
		}

		public IProcess StartNew(string path, params string[] args)
		{
			var raw = default(System.Diagnostics.Process);

			logger.Info($"Attempting to start process '{path}'...");

			if (StartupDesktop != default(IDesktop))
			{
				raw = StartOnDesktop(path, args);
			}
			else
			{
				raw = System.Diagnostics.Process.Start(path, string.Join(" ", args));
			}

			var process = new Process(raw, LoggerFor(raw));

			logger.Info($"Successfully started process '{path}' with ID = {process.Id}.");

			return process;
		}

		private System.Diagnostics.Process StartOnDesktop(string path, params string[] args)
		{
			var commandLine = $"{'"' + path + '"'} {string.Join(" ", args)}";
			var processInfo = new PROCESS_INFORMATION();
			var startupInfo = new STARTUPINFO();

			startupInfo.cb = Marshal.SizeOf(startupInfo);
			startupInfo.lpDesktop = StartupDesktop?.Name;

			var success = Kernel32.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, true, Constant.NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref startupInfo, ref processInfo);

			if (success)
			{
				return System.Diagnostics.Process.GetProcessById(processInfo.dwProcessId);
			}

			var errorCode = Marshal.GetLastWin32Error();

			logger.Error($"Failed to start process '{path}' on desktop '{StartupDesktop}'! Error code: {errorCode}.");

			throw new Win32Exception(errorCode);
		}

		public bool TryGetById(int id, out IProcess process)
		{
			var raw = System.Diagnostics.Process.GetProcesses().FirstOrDefault(p => p.Id == id);

			process = default(IProcess);

			if (raw != default(System.Diagnostics.Process))
			{
				process = new Process(raw, LoggerFor(raw));
			}

			return process != default(IProcess);
		}

		private IEnumerable<(int processId, string originalName)> LoadOriginalNames()
		{
			var names = new List<(int, string)>();

			try
			{
				using (var searcher = new ManagementObjectSearcher($"SELECT ProcessId, ExecutablePath FROM Win32_Process"))
				using (var results = searcher.Get())
				{
					var processData = results.Cast<ManagementObject>().ToList();

					foreach (var process in processData)
					{
						using (process)
						{
							var processId = Convert.ToInt32(process["ProcessId"]);
							var executablePath = Convert.ToString(process["ExecutablePath"]);

							if (File.Exists(executablePath))
							{
								var executableInfo = FileVersionInfo.GetVersionInfo(executablePath);
								var originalName = Path.GetFileNameWithoutExtension(executableInfo.OriginalFilename);

								names.Add((processId, originalName));
							}
							else
							{
								names.Add((processId, default(string)));
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to retrieve original names for processes!", e);
			}

			return names;
		}

		private ILogger LoggerFor(System.Diagnostics.Process process)
		{
			return logger.CloneFor($"{nameof(Process)} '{process.ProcessName}' ({process.Id})");
		}
	}
}
