/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Suspension
{
	internal class ThreadMonitor : IDisposable
	{
		private readonly List<IntPtr> handles;
		private readonly int processId;
		private readonly INativeMethods nativeMethods;

		internal ThreadMonitor(INativeMethods nativeMethods)
		{
			this.nativeMethods = nativeMethods;
			this.handles = new List<IntPtr>();
			this.processId = Process.GetCurrentProcess().Id;
		}

		public void Dispose()
		{
			foreach (var handle in handles)
			{
				nativeMethods.CloseThread(handle);
			}

			handles.Clear();
		}

		internal void Refresh()
		{
			Dispose();

			using (var process = Process.GetProcessById(processId))
			{
				foreach (var thread in process.Threads.Cast<ProcessThread>())
				{
					var handle = nativeMethods.OpenThread(thread.Id);

					if (handle != IntPtr.Zero)
					{
						handles.Add(handle);
					}
				}
			}
		}

		internal bool IsAnySuspended()
		{
			var suspended = false;

			foreach (var handle in handles)
			{
				suspended = nativeMethods.IsThreadSuspended(handle);

				if (suspended)
				{
					break;
				}
			}

			return suspended;
		}
	}
}
