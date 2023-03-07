/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using System.Timers;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.WindowsApi
{
	public class DesktopMonitor : IDesktopMonitor
	{
		private readonly ILogger logger;
		private readonly Timer timer;

		private IDesktop desktop;

		public DesktopMonitor(ILogger logger)
		{
			this.logger = logger;
			this.timer = new Timer(1000);
		}

		public void Start(IDesktop desktop)
		{
			this.desktop = desktop;

			timer.AutoReset = false;
			timer.Elapsed += Timer_Elapsed;
			timer.Start();

			logger.Info($"Started monitoring desktop {desktop}.");
		}

		public void Stop()
		{
			timer.Stop();
			timer.Elapsed -= Timer_Elapsed;

			logger.Info($"Stopped monitoring desktop {desktop}.");
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var handle = User32.OpenInputDesktop(0, false, (uint) AccessMask.DESKTOP_NONE);
			var name = string.Empty;
			var nameLength = 0;

			if (handle != IntPtr.Zero)
			{
				User32.GetUserObjectInformation(handle, Constant.UOI_NAME, IntPtr.Zero, 0, ref nameLength);

				var namePointer = Marshal.AllocHGlobal(nameLength);
				var success = User32.GetUserObjectInformation(handle, Constant.UOI_NAME, namePointer, nameLength, ref nameLength);

				if (success)
				{
					name = Marshal.PtrToStringAnsi(namePointer);
					Marshal.FreeHGlobal(namePointer);

					if (name?.Equals(desktop.Name, StringComparison.OrdinalIgnoreCase) != true)
					{
						logger.Warn($"Detected desktop switch to '{name}' [{handle}], trying to reactivate {desktop}...");
						desktop.Activate();
					}
				}
				else
				{
					logger.Warn("Failed to get name of currently active desktop!");
				}

				User32.CloseDesktop(handle);
			}
			else
			{
				logger.Warn("Failed to get currently active desktop!");
			}

			timer.Start();
		}
	}
}
