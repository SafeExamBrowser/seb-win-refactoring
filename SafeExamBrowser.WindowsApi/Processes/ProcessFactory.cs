/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi.Processes
{
	public class ProcessFactory : IProcessFactory
	{
		private readonly IModuleLogger logger;

		public IDesktop StartupDesktop { private get; set; }

		public ProcessFactory(IModuleLogger logger)
		{
			this.logger = logger;
		}

		public IList<IProcess> GetAllRunning()
		{
			var processes = new List<IProcess>();
			var running = System.Diagnostics.Process.GetProcesses();
			var names = LoadAllProcessNames();

			foreach (var process in running)
			{
				if (names.Any(n => n.processId == process.Id))
				{
					var (_, name, originalName, path, signature) = names.First(n => n.processId == process.Id);

					processes.Add(new Process(process, name, originalName, LoggerFor(process, name), path, signature));
				}
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
				raw = StartNormal(path, args);
			}

			var (name, originalName, _, signature) = LoadProcessNamesFor(raw);
			var process = new Process(raw, name, originalName, LoggerFor(raw, name), path, signature);

			logger.Info($"Successfully started process '{path}' with ID = {process.Id}.");

			return process;
		}

		public bool TryGetById(int id, out IProcess process)
		{
			process = default;

			try
			{
				var raw = System.Diagnostics.Process.GetProcessById(id);
				var (name, originalName, path, signature) = LoadProcessNamesFor(raw);

				process = new Process(raw, name, originalName, LoggerFor(raw, name), path, signature);
			}
			catch (Exception e)
			{
				logger.Error($"Failed to get process with ID = {id}!", e);
			}

			return process != default(IProcess);
		}

		private IEnumerable<(int processId, string name, string originalName, string path, string signature)> LoadAllProcessNames()
		{
			var names = new List<(int, string, string, string, string)>();

			try
			{
				using (var searcher = new ManagementObjectSearcher($"SELECT ProcessId, Name, ExecutablePath FROM Win32_Process"))
				using (var results = searcher.Get())
				{
					var processData = results.Cast<ManagementObject>().ToList();

					foreach (var process in processData)
					{
						using (process)
						{
							var name = Convert.ToString(process["Name"]);
							var originalName = default(string);
							var path = Convert.ToString(process["ExecutablePath"]);
							var processId = Convert.ToInt32(process["ProcessId"]);
							var signature = default(string);

							if (File.Exists(path))
							{
								TryLoadOriginalName(path, out originalName);
								TryLoadSignature(path, out signature);
							}

							names.Add((processId, name, originalName, path, signature));
						}

					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to load process names!", e);
			}

			return names;
		}

		private (string name, string originalName, string path, string signature) LoadProcessNamesFor(System.Diagnostics.Process process)
		{
			var name = process.ProcessName;
			var originalName = default(string);
			var path = default(string);
			var signature = default(string);

			try
			{
				using (var searcher = new ManagementObjectSearcher($"SELECT Name, ExecutablePath FROM Win32_Process WHERE ProcessId = {process.Id}"))
				using (var results = searcher.Get())
				using (var processData = results.Cast<ManagementObject>().First())
				{
					name = Convert.ToString(processData["Name"]);
					path = Convert.ToString(processData["ExecutablePath"]);

					if (File.Exists(path))
					{
						TryLoadOriginalName(path, out originalName);
						TryLoadSignature(path, out signature);
					}
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to load process names for {process.ProcessName}!", e);
			}

			return (name, originalName, path, signature);
		}

		private ILogger LoggerFor(System.Diagnostics.Process process, string name)
		{
			return logger.CloneFor($"{nameof(Process)} '{name}' ({process.Id})");
		}

		private System.Diagnostics.Process StartNormal(string path, params string[] args)
		{
			var process = new System.Diagnostics.Process();

			process.StartInfo.Arguments = string.Join(" ", args);
			process.StartInfo.FileName = path;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			process.Start();

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

		private bool TryLoadOriginalName(string path, out string originalName)
		{
			originalName = default;

			try
			{
				originalName = FileVersionInfo.GetVersionInfo(path).OriginalFilename;
			}
			catch
			{
			}

			return originalName != default;
		}

		private bool TryLoadSignature(string path, out string signature)
		{
			signature = default;

			try
			{
				using (var certificate = X509Certificate.CreateFromSignedFile(path))
				{
					signature = certificate.GetCertHashString();
				}
			}
			catch
			{
			}

			return signature != default;
		}
	}
}
