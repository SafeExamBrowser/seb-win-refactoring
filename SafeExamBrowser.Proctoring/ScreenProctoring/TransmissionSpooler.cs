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

		private readonly ILogger logger;
		private readonly ConcurrentQueue<(Metadata metadata, ScreenShot screenShot)> queue;
		private readonly Random random;
		private readonly ServiceProxy service;
		private readonly Timer timer;

		private Queue<(Metadata metadata, DateTime schedule, ScreenShot screenShot)> buffer;
		private int health;
		private bool recovering;
		private DateTime resume;
		private Thread thread;
		private CancellationTokenSource token;

		internal TransmissionSpooler(ILogger logger, ServiceProxy service)
		{
			this.buffer = new Queue<(Metadata, DateTime, ScreenShot)>();
			this.logger = logger;
			this.queue = new ConcurrentQueue<(Metadata, ScreenShot)>();
			this.random = new Random();
			this.service = service;
			this.timer = new Timer();
		}

		internal void Add(Metadata metadata, ScreenShot screenShot)
		{
			queue.Enqueue((metadata, screenShot));
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
			timer.Interval = 2000;
			// TODO: Revert!
			// timer.Interval = FIFTEEN_SECONDS;
			timer.Start();
		}

		internal void Stop()
		{
			const int TEN_SECONDS = 10000;

			if (thread != default)
			{
				logger.Debug("Stopping...");

				timer.Elapsed -= Timer_Elapsed;
				timer.Stop();

				try
				{
					token.Cancel();
				}
				catch (Exception e)
				{
					logger.Error("Failed to initiate cancellation!", e);
				}

				try
				{
					var success = thread.Join(TEN_SECONDS);

					if (!success)
					{
						thread.Abort();
						logger.Warn($"Aborted since stopping gracefully within {TEN_SECONDS / 1000:N0} seconds failed!");
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to stop!", e);
				}

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
					ExecuteCacheOnly();
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

		private void ExecuteCacheOnly()
		{
			const int THREE_MINUTES = 180;

			if (!recovering)
			{
				recovering = true;
				resume = DateTime.Now.AddSeconds(random.Next(0, THREE_MINUTES));

				logger.Warn($"Activating local caching and suspending transmission due to bad service health (value: {health}, resume: {resume:HH:mm:ss}).");
			}

			CacheBuffer();
			CacheFromQueue();
		}

		private void ExecuteDeferred()
		{
			Schedule(health);

			if (TryPeekFromBuffer(out _, out var schedule, out _) && schedule <= DateTime.Now)
			{
				TryTransmitFromBuffer();
			}
		}

		private void ExecuteNormally()
		{
			TryTransmitFromBuffer();
			TryTransmitFromCache();
			TryTransmitFromQueue();
		}

		private void ExecuteRecovery()
		{
			CacheFromQueue();
			recovering = DateTime.Now < resume;

			if (!recovering)
			{
				logger.Info($"Deactivating local caching and resuming transmission due to improved service health (value: {health}).");
			}
		}

		private void Buffer(Metadata metadata, DateTime schedule, ScreenShot screenShot)
		{
			buffer.Enqueue((metadata, schedule, screenShot));
			buffer = new Queue<(Metadata, DateTime, ScreenShot)>(buffer.OrderBy((b) => b.schedule));

			// TODO: Remove!
			PrintBuffer();
		}

		private void PrintBuffer()
		{
			logger.Log("-------------------------------------------------------------------------------------------------------");
			logger.Info($"Buffer: {buffer.Count}");

			foreach (var (m, t, s) in buffer)
			{
				logger.Log($"\t\t{t} ({m.Elapsed} {s.Data.Length})");
			}

			logger.Log("-------------------------------------------------------------------------------------------------------");
		}

		private void CacheBuffer()
		{
			foreach (var (metadata, _, screenShot) in buffer)
			{
				using (screenShot)
				{
					Cache(metadata, screenShot);
				}
			}

			// TODO: Revert!
			// buffer.Clear();
		}

		private void CacheFromQueue()
		{
			if (TryDequeue(out var metadata, out var screenShot))
			{
				using (screenShot)
				{
					Cache(metadata, screenShot);
				}
			}
		}

		private void Cache(Metadata metadata, ScreenShot screenShot)
		{
			// TODO: Implement caching!
			//var directory = Dispatcher.Invoke(() => OutputPath.Text);
			//var extension = screenShot.Format.ToString().ToLower();
			//var path = Path.Combine(directory, $"{DateTime.Now:HH\\hmm\\mss\\sfff\\m\\s}.{extension}");

			//if (!Directory.Exists(directory))
			//{
			//	Directory.CreateDirectory(directory);
			//	logger.Debug($"Created local output directory '{directory}'.");
			//}

			//File.WriteAllBytes(path, screenShot.Data);
			//logger.Debug($"Screen shot saved as '{path}'.");
		}

		private void Schedule(int health)
		{
			if (TryDequeue(out var metadata, out var screenShot))
			{
				var schedule = DateTime.Now.AddMilliseconds((health + 1) * metadata.Elapsed.TotalMilliseconds);

				Buffer(metadata, schedule, screenShot);
			}
		}

		private bool TryDequeue(out Metadata metadata, out ScreenShot screenShot)
		{
			metadata = default;
			screenShot = default;

			if (queue.TryDequeue(out var item))
			{
				metadata = item.metadata;
				screenShot = item.screenShot;
			}

			return metadata != default && screenShot != default;
		}

		private bool TryPeekFromBuffer(out Metadata metadata, out DateTime schedule, out ScreenShot screenShot)
		{
			metadata = default;
			schedule = default;
			screenShot = default;

			if (buffer.Any())
			{
				(metadata, schedule, screenShot) = buffer.Peek();
			}

			return metadata != default && screenShot != default;
		}

		private bool TryTransmitFromBuffer()
		{
			var success = false;

			if (TryPeekFromBuffer(out var metadata, out _, out var screenShot))
			{
				// TODO: Exception after sending of screenshot, most likely due to concurrent disposal!!
				success = TryTransmit(metadata, screenShot);

				if (success)
				{
					buffer.Dequeue();
					screenShot.Dispose();

					// TODO: Revert!
					PrintBuffer();
				}
			}

			return success;
		}

		private bool TryTransmitFromCache()
		{
			var success = false;

			// TODO: Implement transmission from cache!
			//if (Cache.Any())
			//{
			//	
			//}
			//else
			//{
			//	success = true;
			//}

			return success;
		}

		private bool TryTransmitFromQueue()
		{
			var success = false;

			if (TryDequeue(out var metadata, out var screenShot))
			{
				success = TryTransmit(metadata, screenShot);

				if (success)
				{
					screenShot.Dispose();
				}
				else
				{
					Buffer(metadata, DateTime.Now, screenShot);
				}
			}

			return success;
		}

		private bool TryTransmit(Metadata metadata, ScreenShot screenShot)
		{
			var success = false;

			if (service.IsConnected)
			{
				success = service.Send(metadata, screenShot).Success;
			}
			else
			{
				logger.Warn("Cannot send screen shot as service is disconnected!");
			}

			return success;
		}

		private readonly Random temp = new Random();

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			// TODO: Revert!
			//if (service.IsConnected)
			//{
			//	var response = service.GetHealth();

			//	if (response.Success)
			//	{
			//		var previous = health;

			//		health = response.Value > BAD ? BAD : (response.Value < GOOD ? GOOD : response.Value);

			//		if (previous != health)
			//		{
			//			logger.Info($"Service health {(previous < health ? "deteriorated" : "improved")} from {previous} to {health}.");
			//		}
			//	}
			//}
			//else
			//{
			//	logger.Warn("Cannot query health as service is disconnected!");
			//}

			var previous = health;

			health += temp.Next(-3, 5);
			health = health < GOOD ? GOOD : (health > BAD ? BAD : health);

			if (previous != health)
			{
				logger.Info($"Service health {(previous < health ? "deteriorated" : "improved")} from {previous} to {health}.");
			}

			timer.Start();
		}
	}
}
