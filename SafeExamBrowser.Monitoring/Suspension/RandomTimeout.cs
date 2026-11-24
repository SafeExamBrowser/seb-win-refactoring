/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Suspension
{
	internal class RandomTimeout : IDisposable
	{
		private static readonly TimeSpan MEAN_INTERVAL = TimeSpan.FromMilliseconds(50);

		private readonly IntPtr timer;
		private readonly Random random;
		private readonly INativeMethods nativeMethods;

		internal RandomTimeout(INativeMethods nativeMethods)
		{
			this.nativeMethods = nativeMethods;
			this.random = new Random();
			this.timer = nativeMethods.CreateWaitableTimer();
		}

		public void Dispose()
		{
			nativeMethods.CloseTimer(timer);
		}

		internal TimeSpan Wait()
		{
			var ticks = -MEAN_INTERVAL.Ticks * Math.Log(1.0 - random.NextDouble());
			var interval = TimeSpan.FromTicks(Math.Max(1, (long) Math.Min(ticks, MEAN_INTERVAL.Ticks * 10.0)));
			var dueTime = -interval.Ticks;

			nativeMethods.SetAndWait(timer, dueTime);

			return interval;
		}
	}
}
