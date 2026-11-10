/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Events;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Suspension
{
	public class SuspensionMonitor : ISuspensionMonitor
	{
		private const int SUSPENSION_MINIMUM = 3;
		private const double RATIO_MAXIMUM = 0.1;

		private static readonly TimeSpan REFRESH_INTERVAL = TimeSpan.FromSeconds(1);
		private static readonly TimeSpan SUSPENSION_LIMIT = TimeSpan.FromSeconds(1.5);
		private static readonly TimeSpan TWO_SECONDS = TimeSpan.FromSeconds(2);

		private readonly ILogger logger;
		private readonly MonotonicTime monotonicTime;
		private readonly INativeMethods nativeMethods;
		private readonly ManualResetEventSlim stop;

		private Thread thread;

		public event SuspensionDetectedEventHandler SuspensionDetected;

		public SuspensionMonitor(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.monotonicTime = new MonotonicTime(nativeMethods);
			this.nativeMethods = nativeMethods;
			this.stop = new ManualResetEventSlim();
		}

		public void Start()
		{
			stop.Reset();

			thread = new Thread(Run);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Highest;
			thread.Start();

			logger.Info("Started monitoring.");
		}

		public void Stop()
		{
			if (thread != default)
			{
				stop.Set();

				try
				{
					if (!thread.Join(TWO_SECONDS))
					{
						thread.Abort();
					}

					thread = default;
					logger.Info("Stopped monitoring.");
				}
				catch (Exception e)
				{
					logger.Error("Failed to stop monitoring!", e);
				}
			}
		}

		private void Run()
		{
			var last = monotonicTime.Now;
			var lastRefresh = last - REFRESH_INTERVAL;
			var resume = TimeSpan.Zero;
			var samples = new SampleWindow(TWO_SECONDS);

			using (var threads = new ThreadMonitor(nativeMethods))
			using (var timeout = new RandomTimeout(nativeMethods))
			{
				while (!stop.IsSet)
				{
					var expected = timeout.Wait();
					var now = monotonicTime.Now;
					var actual = now - last - expected;

					last = now;

					if (now - lastRefresh >= REFRESH_INTERVAL)
					{
						threads.Refresh();
						lastRefresh = now;
					}

					samples.Add(now, threads.IsAnySuspended());

					if (now >= resume)
					{
						Detect(actual, ref resume, samples);
					}
				}
			}
		}

		private void Detect(TimeSpan actual, ref TimeSpan resume, SampleWindow samples)
		{
			var detected = false;

			if (actual > SUSPENSION_LIMIT)
			{
				detected = true;
				logger.Warn($"The process was suspended for {actual:hh\\:mm\\:ss\\.fff} since {DateTime.Now - actual:HH:mm:ss}!");
			}

			if (samples.Suspended >= SUSPENSION_MINIMUM && samples.Ratio > RATIO_MAXIMUM)
			{
				detected = true;
				logger.Warn($"Threads were suspended in {samples.Suspended} of {samples.Count} samples during the last {samples.Duration.TotalSeconds} seconds ({samples.Ratio * 100:0.#}%)!");
			}

			if (detected)
			{
				resume = monotonicTime.Now + samples.Duration + SUSPENSION_LIMIT;
				samples.Clear();

				Task.Run(() => SuspensionDetected?.Invoke());
			}
		}
	}
}
