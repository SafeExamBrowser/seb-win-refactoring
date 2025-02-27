/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.Settings.Proctoring;
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

		internal TransmissionSpooler(AppConfig appConfig, IModuleLogger logger, ServiceProxy service, ScreenProctoringSettings settings)
		{
			this.buffer = new Buffer(logger.CloneFor(nameof(Buffer)));
			this.cache = new Cache(appConfig, logger.CloneFor(nameof(Cache)), settings);
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

		internal void ExecuteRemainingWork(Action<RemainingWorkUpdatedEventArgs> handler)
		{
			var previous = buffer.Count + cache.Count;
			var progress = 0;
			var start = DateTime.Now;
			var total = previous;

			while (HasRemainingWork() && service.IsConnected)
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

				var args = UpdateStatus(handler, progress, start, total);

				if (args.CancellationRequested)
				{
					logger.Warn($"The execution of the remaining work has been cancelled and {remaining} item(s) will not be transmitted!");

					break;
				}

				Thread.Sleep(100);
			}

			UpdateStatus(handler);
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
			timer.Elapsed += Timer_Elapsed;
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

			buffer.Clear();
			cache.Clear();
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
				logger.Info($"Recovery terminated, deactivating local caching and resuming transmission.");
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
					buffer.Enqueue(metaData, CalculateSchedule(metaData), screenShot);
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

			if (hasItem && ready)
			{
				buffer.Dequeue();

				if (TryTransmit(metaData, screenShot))
				{
					screenShot.Dispose();
				}
				else
				{
					buffer.Enqueue(metaData, CalculateSchedule(metaData), screenShot);
				}
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
					buffer.Enqueue(metaData, CalculateSchedule(metaData), screenShot);
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
					buffer.Enqueue(metaData, CalculateSchedule(metaData), screenShot);
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
				var value = response.Success ? response.Value : BAD;

				health = UpdateHealth(value);
				networkIssue = !response.Success;
				success = response.Success;
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
				var value = response.Success ? response.Value : BAD;

				health = UpdateHealth(value);
				networkIssue = !response.Success;
			}
			else
			{
				logger.Warn("Cannot query health as service is disconnected!");
			}

			if (!timer.Enabled)
			{
				timer.Start();
			}
		}

		private int UpdateHealth(int value)
		{
			const int THREE_MINUTES = 180;

			var previous = health;
			var current = value > BAD ? BAD : (value < GOOD ? GOOD : value);

			if (previous != current)
			{
				logger.Info($"Service health {(previous < current ? "deteriorated" : "improved")} from {previous} to {current}.");

				if (current == BAD)
				{
					recovering = false;
					resume = DateTime.Now.AddSeconds(random.Next(0, THREE_MINUTES));

					if (!timer.Enabled)
					{
						timer.Start();
					}

					logger.Warn($"Activating local caching and suspending transmission due to bad service health (resume: {resume:HH:mm:ss}).");
				}
				else if (previous == BAD)
				{
					recovering = true;
					resume = DateTime.Now < resume ? resume : DateTime.Now.AddSeconds(random.Next(0, THREE_MINUTES));

					logger.Info($"Starting recovery while maintaining local caching (resume: {resume:HH:mm:ss}).");
				}
			}

			return current;
		}

		private RemainingWorkUpdatedEventArgs UpdateStatus(Action<RemainingWorkUpdatedEventArgs> handler, int progress, DateTime start, int total)
		{
			var args = new RemainingWorkUpdatedEventArgs
			{
				AllowCancellation = start.Add(new TimeSpan(0, 1, 15)) < DateTime.Now,
				IsWaiting = health == BAD || networkIssue || recovering || DateTime.Now < resume,
				Next = buffer.TryPeek(out _, out var schedule, out _) ? schedule : default(DateTime?),
				Progress = progress,
				Resume = DateTime.Now < resume ? resume : default(DateTime?),
				Total = total
			};

			handler.Invoke(args);

			return args;
		}

		private void UpdateStatus(Action<RemainingWorkUpdatedEventArgs> handler)
		{
			if (HasRemainingWork())
			{
				handler.Invoke(new RemainingWorkUpdatedEventArgs { HasFailed = true, CachePath = cache.Directory });
			}
			else
			{
				handler.Invoke(new RemainingWorkUpdatedEventArgs { IsFinished = true });
			}
		}
	}
}
