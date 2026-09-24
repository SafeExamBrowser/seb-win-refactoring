/*
 * Copyright (c) 2026 ETH Zürich, IT Services
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

		private object originalDebuggerValue;
		private bool originalDebuggerExisted;
		private bool neutralized;

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
					logger.Warn($"Ease of access configuration is compromised: '{RegistryValue.MachineHive.EaseOfAccess_Key}\\{RegistryValue.MachineHive.EaseOfAccess_Name}' = '{value}'!");
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

		internal bool Neutralize()
		{
			var key = RegistryValue.MachineHive.EaseOfAccess_Key;
			var name = RegistryValue.MachineHive.EaseOfAccess_Name;

			originalDebuggerExisted = registry.TryRead(key, name, out originalDebuggerValue);

			if (!originalDebuggerExisted || (originalDebuggerValue is string s && string.IsNullOrWhiteSpace(s)))
			{
				logger.Info("Ease of access configuration does not require neutralization.");

				return true;
			}

			if (registry.TryDelete(key, name))
			{
				neutralized = true;
				logger.Info($"Neutralized ease of access debugger '{originalDebuggerValue}' for the current session.");

				return true;
			}

			logger.Error($"Failed to neutralize ease of access debugger '{originalDebuggerValue}'. Writing '{key}\\{name}' requires administrative rights or the SEB Service.");

			return false;
		}

		internal bool Restore()
		{
			if (!neutralized)
			{
				return true;
			}

			var key = RegistryValue.MachineHive.EaseOfAccess_Key;
			var name = RegistryValue.MachineHive.EaseOfAccess_Name;
			var success = originalDebuggerExisted ? registry.TryWrite(key, name, originalDebuggerValue) : registry.TryDelete(key, name);

			if (success)
			{
				logger.Info(originalDebuggerExisted
					? $"Restored ease of access debugger to '{originalDebuggerValue}'."
					: "Restored ease of access configuration (debugger value remains absent).");
				neutralized = false;
			}
			else
			{
				logger.Error($"Failed to restore ease of access debugger '{originalDebuggerValue}'!");
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
