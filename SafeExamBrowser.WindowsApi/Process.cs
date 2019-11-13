/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	internal class Process : IProcess
	{
		private bool eventInitialized, originalNameInitialized;
		private ILogger logger;
		private string originalName;
		private System.Diagnostics.Process process;

		private event ProcessTerminatedEventHandler TerminatedEvent;

		public int Id
		{
			get { return process.Id; }
		}

		public bool HasTerminated
		{
			get { return IsTerminated(); }
		}

		public string Name { get; }

		public string OriginalName
		{
			get { return originalNameInitialized ? originalName : InitializeOriginalName(); }
		}

		public event ProcessTerminatedEventHandler Terminated
		{
			add { TerminatedEvent += value; InitializeEvent(); }
			remove { TerminatedEvent -= value; }
		}

		internal Process(System.Diagnostics.Process process, ILogger logger)
		{
			this.Name = process.ProcessName;
			this.process = process;
			this.logger = logger;
		}

		internal Process(System.Diagnostics.Process process, string originalName, ILogger logger) : this(process, logger)
		{
			this.originalName = originalName;
			this.originalNameInitialized = true;
		}

		public bool TryActivate()
		{
			try
			{
				var success = User32.BringWindowToTop(process.MainWindowHandle);
				var placement = new WINDOWPLACEMENT();

				placement.length = Marshal.SizeOf(placement);
				User32.GetWindowPlacement(process.MainWindowHandle, ref placement);

				if (placement.showCmd == (int) ShowWindowCommand.ShowMinimized)
				{
					success &= User32.ShowWindow(process.MainWindowHandle, (int) ShowWindowCommand.Restore);
				}

				return success;
			}
			catch (Exception e)
			{
				logger.Error("Failed to activate process!", e);
			}

			return false;
		}

		public bool TryClose(int timeout_ms = 0)
		{
			try
			{
				logger.Debug("Attempting to close process...");
				process.Refresh();

				var success = process.CloseMainWindow();

				if (success)
				{
					logger.Debug("Successfully sent close message to main window.");
				}
				else
				{
					logger.Warn("Failed to send close message to main window!");
				}

				return success && WaitForTermination(timeout_ms);
			}
			catch (Exception e)
			{
				logger.Error("Failed to close main window!", e);
			}

			return false;
		}

		public bool TryGetWindowTitle(out string title)
		{
			title = default(string);

			try
			{
				process.Refresh();
				title = process.MainWindowTitle;

				return true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to retrieve title of main window!", e);
			}

			return false;
		}

		public bool TryKill(int timeout_ms = 0)
		{
			try
			{
				logger.Debug("Attempting to kill process...");

				process.Refresh();
				process.Kill();

				return WaitForTermination(timeout_ms);
			}
			catch (Exception e)
			{
				logger.Error("Failed to kill process!", e);
			}

			return false;
		}

		public override string ToString()
		{
			return $"'{Name}' ({Id})";
		}

		private bool IsTerminated()
		{
			try
			{
				process.Refresh();

				return process.HasExited;
			}
			catch (Exception e)
			{
				logger.Error("Failed to check whether process is terminated!", e);
			}

			return false;
		}

		private void InitializeEvent()
		{
			if (!eventInitialized)
			{
				eventInitialized = true;
				process.Exited += Process_Exited;
				process.EnableRaisingEvents = true;
				logger.Debug("Initialized termination event.");
			}
		}

		private string InitializeOriginalName()
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {process.Id}"))
				using (var results = searcher.Get())
				using (var processData = results.Cast<ManagementObject>().First())
				{
					var executablePath = Convert.ToString(processData["ExecutablePath"]);

					if (File.Exists(executablePath))
					{
						var executableInfo = FileVersionInfo.GetVersionInfo(executablePath);
						var originalName = Path.GetFileNameWithoutExtension(executableInfo.OriginalFilename);

						this.originalName = originalName;
					}
					else
					{
						logger.Warn("Could not find original name!");
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to initialize original name!", e);
			}
			finally
			{
				originalNameInitialized = true;
			}

			return originalName;
		}

		private bool WaitForTermination(int timeout_ms)
		{
			var terminated = process.WaitForExit(timeout_ms);

			if (terminated)
			{
				logger.Debug($"Process has terminated within {timeout_ms}ms.");
			}
			else
			{
				logger.Warn($"Process failed to terminate within {timeout_ms}ms!");
			}

			return terminated;
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			TerminatedEvent?.Invoke(process.ExitCode);
			logger.Debug("Process has terminated.");
		}
	}
}
