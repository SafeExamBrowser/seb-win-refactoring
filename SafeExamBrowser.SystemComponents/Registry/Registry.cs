/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Timers;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.SystemComponents.Contracts.Registry.Events;

namespace SafeExamBrowser.SystemComponents.Registry
{
	public class Registry : IRegistry
	{
		private readonly ILogger logger;
		private readonly ConcurrentBag<(string key, string name, object value)> values;

		private Timer timer;

		public event RegistryValueChangedEventHandler ValueChanged;

		public Registry(ILogger logger)
		{
			this.logger = logger;
			this.values = new ConcurrentBag<(string key, string name, object value)>();
		}

		public void StartMonitoring(string key, string name)
		{
			const int ONE_SECOND = 1000;

			if (timer?.Enabled != true)
			{
				timer = new Timer(ONE_SECOND);
				timer.AutoReset = true;
				timer.Elapsed += Timer_Elapsed;
				timer.Start();
			}

			if (TryRead(key, name, out var value))
			{
				values.Add((key, name, value));
				logger.Debug($"Started monitoring value '{name}' from registry key '{key}'. Initial value: '{value}'.");
			}
			else
			{
				logger.Error($"Failed to start monitoring value '{name}' from registry key '{key}'!");
			}
		}

		public void StopMonitoring()
		{
			while (!values.IsEmpty)
			{
				values.TryTake(out _);
			}

			if (timer != null)
			{
				timer.Stop();
				logger.Debug("Stopped monitoring the registry.");
			}
		}

		public bool TryRead(string key, string name, out object value)
		{
			var success = true;

			value = default;

			try
			{
				value = Microsoft.Win32.Registry.GetValue(key, name, default);
			}
			catch (Exception e)
			{
				success = false;
				logger.Error($"Failed to read value '{name}' from registry key '{key}'!", e);
			}

			return success;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			foreach (var item in values)
			{
				if (TryRead(item.key, item.name, out var value))
				{
					if (item.value != value)
					{
						logger.Debug($"Value '{item.name}' from registry key '{item.key}' has changed from '{item.value}' to '{value}'!");
						ValueChanged?.Invoke(item.value, value);
					}
				}
				else
				{
					logger.Error($"Failed to monitor value '{item.name}' from registry key '{item.key}'!");
				}
			}
		}
	}
}
