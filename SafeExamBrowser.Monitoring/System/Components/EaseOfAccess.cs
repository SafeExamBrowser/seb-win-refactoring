/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System.Events;
using SafeExamBrowser.SystemComponents.Contracts.Registry;

namespace SafeExamBrowser.Monitoring.System.Components
{
	internal class EaseOfAccess
	{
		private readonly ILogger logger;
		private readonly IRegistry registry;

		internal event SentinelEventHandler EaseOfAccessChanged;

		internal EaseOfAccess(ILogger logger, IRegistry registry)
		{
			this.logger = logger;
			this.registry = registry;
		}

		internal void StartMonitoring()
		{
			registry.ValueChanged += Registry_ValueChanged;
			registry.StartMonitoring(RegistryValue.MachineHive.EaseOfAccess_Key, RegistryValue.MachineHive.EaseOfAccess_Name);

			logger.Info("Started monitoring ease of access.");
		}

		internal void StopMonitoring()
		{
			registry.ValueChanged -= Registry_ValueChanged;
			registry.StopMonitoring(RegistryValue.MachineHive.EaseOfAccess_Key, RegistryValue.MachineHive.EaseOfAccess_Name);

			logger.Info("Stopped monitoring ease of access.");
		}

		internal bool Verify()
		{
			logger.Info($"Starting ease of access verification...");

			var success = registry.TryRead(RegistryValue.MachineHive.EaseOfAccess_Key, RegistryValue.MachineHive.EaseOfAccess_Name, out var value);

			if (success)
			{
				if (value is string s && string.IsNullOrWhiteSpace(s))
				{
					logger.Info("Ease of access configuration successfully verified.");
				}
				else
				{
					logger.Warn($"Ease of access configuration is compromised: '{value}'!");
					success = false;
				}
			}
			else
			{
				success = true;
				logger.Info("Ease of access configuration successfully verified (value does not exist).");
			}

			return success;
		}

		private void Registry_ValueChanged(string key, string name, object oldValue, object newValue)
		{
			if (key == RegistryValue.MachineHive.EaseOfAccess_Key)
			{
				HandleEaseOfAccessChange(key, name, oldValue, newValue);
			}
		}

		private void HandleEaseOfAccessChange(string key, string name, object oldValue, object newValue)
		{
			var args = new SentinelEventArgs();

			logger.Warn($@"The ease of access registry value '{key}\{name}' has changed from '{oldValue}' to '{newValue}'!");

			Task.Run(() => EaseOfAccessChanged?.Invoke(args)).ContinueWith((_) =>
			{
				if (args.Allow)
				{
					registry.StopMonitoring(key, name);
				}
			});
		}
	}
}
