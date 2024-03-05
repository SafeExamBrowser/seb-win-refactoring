/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Timers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class TransmissionSpooler
	{
		private const int BAD = 10;
		private const int GOOD = 0;

		private readonly Buffer buffer;
		private readonly Cache cache;
		private readonly ILogger logger;
		private readonly ConcurrentQueue<(MetaData metaData, ScreenShot screenShot)> queue;
		private readonly Random random;
		private readonly ServiceProxy service;
		private readonly Timer timer;

		private int health;
		private bool networkIssue;
		private bool recovering;
		private DateTime resume;
		private Thread thread;
		private CancellationTokenSource token;

		internal TransmissionSpooler(AppConfig appConfig, IModuleLogger logger, ServiceProxy service)
		{
			this.buffer = new Buffer(logger.CloneFor(nameof(Buffer)));
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

		internal void ExecuteRemainingWork(Action<RemainingWorkUpdatedEventArgs> updateStatus)
		{
			var previous = buffer.Count + cache.Count;
			var progress = 0;
			var total = previous;

			while (HasRemainingWork() && service.IsConnected && (!networkIssue || recovering))
			{
				var remaining = buffer.Count + cache.Count;

				if (total < remaining)
				{
					total = remaining;
				}
				else if (previous < remaining)
				{
					total += remaining - previous;
				}

				previous = remaining;
				progress = total - remaining;

				updateStatus(new RemainingWorkUpdatedEventArgs
				{
					IsWaiting = recovering,
					Next = buffer.TryPeek(out _, out var schedule, out _) ? schedule : default(DateTime?),
					Progress = progress,
					Resume = resume,
					Total = total
				});

				Thread.Sleep(100);
			}

			if (networkIssue)
			{
				updateStatus(new RemainingWorkUpdatedEventArgs { HasFailed = true, CachePath = cache.Directory });
			}
			else
			{
				updateStatus(new RemainingWorkUpdatedEventArgs { IsFinished = true });
			}
		}

		internal bool HasRemainingWork()
		{
			return buffer.Any() || cache.Any();
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
			TransmitFromBuffer();
		}

		private void ExecuteNormally()
		{
			TransmitFromBuffer();
			TransmitFromCache();
			TransmitFromQueue();
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

		private void BufferFromCache()
		{
			if (cache.TryDequeue(out var metaData, out var screenShot))
			{
				buffer.Enqueue(metaData, CalculateSchedule(metaData), screenShot);
			}
		}

		private void BufferFromQueue()
		{
			if (TryDequeue(out var metaData, out var screenShot))
			{
				buffer.Enqueue(metaData, CalculateSchedule(metaData), screenShot);
			}
		}

		private void CacheFromBuffer()
		{
			if (buffer.TryPeek(out var metaData, out _, out var screenShot) && cache.TryEnqueue(metaData, screenShot))
			{
				buffer.Dequeue();
				screenShot.Dispose();
			}
		}

		private void CacheFromQueue()
		{
			if (TryDequeue(out var metaData, out var screenShot))
			{
				if (cache.TryEnqueue(metaData, screenShot))
				{
					screenShot.Dispose();
				}
				else
				{
					buffer.Enqueue(metaData, DateTime.Now, screenShot);
				}
			}
		}

		private DateTime CalculateSchedule(MetaData metaData)
		{
			var timeout = (health + 1) * metaData.Elapsed.TotalMilliseconds;
			var schedule = DateTime.Now.AddMilliseconds(timeout);

			return schedule;
		}

		private void TransmitFromBuffer()
		{
			var hasItem = buffer.TryPeek(out var metaData, out var schedule, out var screenShot);
			var ready = schedule <= DateTime.Now;

			if (hasItem && ready && TryTransmit(metaData, screenShot))
			{
				buffer.Dequeue();
				screenShot.Dispose();
			}
		}

		private void TransmitFromCache()
		{
			if (cache.TryDequeue(out var metaData, out var screenShot))
			{
				if (TryTransmit(metaData, screenShot))
				{
					screenShot.Dispose();
				}
				else
				{
					buffer.Enqueue(metaData, DateTime.Now, screenShot);
				}
			}
		}

		private void TransmitFromQueue()
		{
			if (TryDequeue(out var metaData, out var screenShot))
			{
				if (TryTransmit(metaData, screenShot))
				{
					screenShot.Dispose();
				}
				else
				{
					buffer.Enqueue(metaData, DateTime.Now, screenShot);
				}
			}
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

		private bool TryTransmit(MetaData metaData, ScreenShot screenShot)
		{
			var success = false;

			if (service.IsConnected)
			{
				var response = service.Send(metaData, screenShot);

				networkIssue = !response.Success;
				success = response.Success;

				if (response.Success)
				{
					health = UpdateHealth(response.Value);
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

				networkIssue = !response.Success;

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
