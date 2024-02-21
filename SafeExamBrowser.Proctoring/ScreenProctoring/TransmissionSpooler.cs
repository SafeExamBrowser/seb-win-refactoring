/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class TransmissionSpooler
	{
		const int BAD = 10;
		const int GOOD = 0;

		private readonly Cache cache;
		private readonly ILogger logger;
		private readonly ConcurrentQueue<(MetaData metaData, ScreenShot screenShot)> queue;
		private readonly Random random;
		private readonly ServiceProxy service;
		private readonly Timer timer;

		private Queue<(MetaData metaData, DateTime schedule, ScreenShot screenShot)> buffer;
		private int health;
		private bool recovering;
		private DateTime resume;
		private Thread thread;
		private CancellationTokenSource token;

		internal TransmissionSpooler(AppConfig appConfig, IModuleLogger logger, ServiceProxy service)
		{
			this.buffer = new Queue<(MetaData, DateTime, ScreenShot)>();
			this.cache = new Cache(appConfig, logger.CloneFor(nameof(Cache)));
			this.logger = logger;
			this.queue = new ConcurrentQueue<(MetaData, ScreenShot)>();
			this.random = new Random();
			this.service = service;
			this.timer = new Timer();
		}

		internal void Add(MetaData metaData, ScreenShot screenShot)
		{
			queue.Enqueue((metaData, screenShot));
		}

		internal void Start()
		{
			const int FIFTEEN_SECONDS = 15000;

			logger.Debug("Starting...");

			health = GOOD;
			recovering = false;
			resume = default;
			token = new CancellationTokenSource();

			thread = new Thread(Execute);
			thread.IsBackground = true;
			thread.Start();

			timer.AutoReset = false;
			timer.Interval = FIFTEEN_SECONDS;
		}

		internal void Stop()
		{
			const int TEN_SECONDS = 10000;

			if (thread != default)
			{
				logger.Debug("Stopping...");

				timer.Stop();
				timer.Elapsed -= Timer_Elapsed;

				try
				{
					token.Cancel();
				}
				catch (Exception e)
				{
					logger.Error("Failed to initiate execution cancellation!", e);
				}

				try
				{
					if (!thread.Join(TEN_SECONDS))
					{
						thread.Abort();
						logger.Warn($"Aborted execution since stopping gracefully within {TEN_SECONDS / 1000} seconds failed!");
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to stop!", e);
				}

				recovering = false;
				resume = default;
				thread = default;
				token = default;
			}
		}

		private void Execute()
		{
			logger.Debug("Ready.");

			while (!token.IsCancellationRequested)
			{
				if (health == BAD)
				{
					ExecuteCaching();
				}
				else if (recovering)
				{
					ExecuteRecovery();
				}
				else if (health == GOOD)
				{
					ExecuteNormally();
				}
				else
				{
					ExecuteDeferred();
				}

				Thread.Sleep(50);
			}

			logger.Debug("Stopped.");
		}

		private void ExecuteCaching()
		{
			const int THREE_MINUTES = 180;

			if (!recovering)
			{
				recovering = true;
				resume = DateTime.Now.AddSeconds(random.Next(0, THREE_MINUTES));

				timer.Elapsed += Timer_Elapsed;
				timer.Start();

				logger.Warn($"Activating local caching and suspending transmission due to bad service health (value: {health}, resume: {resume:HH:mm:ss}).");
			}

			CacheFromBuffer();
			CacheFromQueue();
		}

		private void ExecuteDeferred()
		{
			BufferFromCache();
			BufferFromQueue();
			TryTransmitFromBuffer();
		}

		private void ExecuteNormally()
		{
			TryTransmitFromBuffer();
			TryTransmitFromCache();
			TryTransmitFromQueue();
		}

		private void ExecuteRecovery()
		{
			recovering = DateTime.Now < resume;

			if (recovering)
			{
				CacheFromQueue();
			}
			else
			{
				timer.Stop();
				timer.Elapsed -= Timer_Elapsed;

				logger.Info($"Deactivating local caching and resuming transmission due to improved service health (value: {health}).");
			}
		}

		private void Buffer(MetaData metaData, DateTime schedule, ScreenShot screenShot)
		{
			buffer.Enqueue((metaData, schedule, screenShot));
			buffer = new Queue<(MetaData, DateTime, ScreenShot)>(buffer.OrderBy((b) => b.schedule));
		}

		private void BufferFromCache()
		{
			if (cache.TryDequeue(out var metaData, out var screenShot))
			{
				Buffer(metaData, CalculateSchedule(metaData), screenShot);
			}
		}

		private void BufferFromQueue()
		{
			if (TryDequeue(out var metaData, out var screenShot))
			{
				Buffer(metaData, CalculateSchedule(metaData), screenShot);
			}
		}

		private void CacheFromBuffer()
		{
			if (TryPeekFromBuffer(out var metaData, out _, out var screenShot))
			{
				var success = cache.TryEnqueue(metaData, screenShot);

				if (success)
				{
					buffer.Dequeue();
					screenShot.Dispose();
				}
			}
		}

		private void CacheFromQueue()
		{
			if (TryDequeue(out var metaData, out var screenShot))
			{
				var success = cache.TryEnqueue(metaData, screenShot);

				if (success)
				{
					screenShot.Dispose();
				}
				else
				{
					Buffer(metaData, DateTime.Now, screenShot);
				}
			}
		}

		private DateTime CalculateSchedule(MetaData metaData)
		{
			var timeout = (health + 1) * metaData.Elapsed.TotalMilliseconds;
			var schedule = DateTime.Now.AddMilliseconds(timeout);

			return schedule;
		}

		private bool TryDequeue(out MetaData metaData, out ScreenShot screenShot)
		{
			metaData = default;
			screenShot = default;

			if (queue.TryDequeue(out var item))
			{
				metaData = item.metaData;
				screenShot = item.screenShot;
			}

			return metaData != default && screenShot != default;
		}

		private bool TryPeekFromBuffer(out MetaData metaData, out DateTime schedule, out ScreenShot screenShot)
		{
			metaData = default;
			schedule = default;
			screenShot = default;

			if (buffer.Any())
			{
				(metaData, schedule, screenShot) = buffer.Peek();
			}

			return metaData != default && screenShot != default;
		}

		private bool TryTransmitFromBuffer()
		{
			var success = false;

			if (TryPeekFromBuffer(out var metaData, out var schedule, out var screenShot) && schedule <= DateTime.Now)
			{
				success = TryTransmit(metaData, screenShot);

				if (success)
				{
					buffer.Dequeue();
					screenShot.Dispose();
				}
			}

			return success;
		}

		private bool TryTransmitFromCache()
		{
			var success = true;

			if (cache.TryDequeue(out var metaData, out var screenShot))
			{
				success = TryTransmit(metaData, screenShot);

				if (success)
				{
					screenShot.Dispose();
				}
				else
				{
					Buffer(metaData, DateTime.Now, screenShot);
				}
			}

			return success;
		}

		private bool TryTransmitFromQueue()
		{
			var success = false;

			if (TryDequeue(out var metaData, out var screenShot))
			{
				success = TryTransmit(metaData, screenShot);

				if (success)
				{
					screenShot.Dispose();
				}
				else
				{
					Buffer(metaData, DateTime.Now, screenShot);
				}
			}

			return success;
		}

		private bool TryTransmit(MetaData metaData, ScreenShot screenShot)
		{
			var success = false;

			if (service.IsConnected)
			{
				var response = service.Send(metaData, screenShot);

				if (response.Success)
				{
					health = UpdateHealth(response.Value);
					success = true;
				}
			}
			else
			{
				logger.Warn("Cannot send screen shot as service is disconnected!");
			}

			return success;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (service.IsConnected)
			{
				var response = service.GetHealth();

				if (response.Success)
				{
					health = UpdateHealth(response.Value);
				}
			}
			else
			{
				logger.Warn("Cannot query health as service is disconnected!");
			}

			timer.Start();
		}

		private int UpdateHealth(int value)
		{
			var previous = health;
			var current = value > BAD ? BAD : (value < GOOD ? GOOD : value);

			if (previous != current)
			{
				logger.Info($"Service health {(previous < current ? "deteriorated" : "improved")} from {previous} to {current}.");
			}

			return current;
		}
	}
}
