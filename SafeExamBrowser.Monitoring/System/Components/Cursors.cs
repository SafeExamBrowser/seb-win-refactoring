/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System.Events;
using SafeExamBrowser.SystemComponents.Contracts.Registry;

namespace SafeExamBrowser.Monitoring.System.Components
{
	internal class Cursors
	{
		private static readonly string SYSTEM_PATH = $@"{Environment.ExpandEnvironmentVariables("%SystemRoot%")}\Cursors\";
		private static readonly string USER_PATH = $@"{Environment.ExpandEnvironmentVariables("%LocalAppData%")}\Microsoft\Windows\Cursors\";

		private readonly ILogger logger;
		private readonly IRegistry registry;

		internal event SentinelEventHandler CursorChanged;

		internal Cursors(ILogger logger, IRegistry registry)
		{
			this.logger = logger;
			this.registry = registry;
		}

		internal void StartMonitoring()
		{
			registry.ValueChanged += Registry_ValueChanged;

			if (registry.TryGetNames(RegistryValue.UserHive.Cursors_Key, out var names))
			{
				foreach (var name in names)
				{
					registry.StartMonitoring(RegistryValue.UserHive.Cursors_Key, name);
				}

				logger.Info("Started monitoring cursors.");
			}
			else
			{
				logger.Warn("Failed to start monitoring cursor registry values!");
			}
		}

		internal void StopMonitoring()
		{
			registry.ValueChanged -= Registry_ValueChanged;

			if (registry.TryGetNames(RegistryValue.UserHive.Cursors_Key, out var names))
			{
				foreach (var name in names)
				{
					registry.StopMonitoring(RegistryValue.UserHive.Cursors_Key, name);
				}

				logger.Info("Stopped monitoring cursors.");
			}
			else
			{
				logger.Warn("Failed to stop monitoring cursor registry values!");
			}
		}

		internal bool Verify()
		{
			logger.Info($"Starting cursor verification...");

			var success = registry.TryGetNames(RegistryValue.UserHive.Cursors_Key, out var cursors);

			if (success)
			{
				foreach (var cursor in cursors.Where(c => !string.IsNullOrWhiteSpace(c)))
				{
					success &= VerifyCursor(cursor);
				}

				if (success)
				{
					logger.Info("Cursor configuration successfully verified.");
				}
				else
				{
					logger.Warn("Cursor configuration is compromised!");
				}
			}
			else
			{
				logger.Error("Failed to verify cursor configuration!");
			}

			return success;
		}

		private void Registry_ValueChanged(string key, string name, object oldValue, object newValue)
		{
			if (key == RegistryValue.UserHive.Cursors_Key)
			{
				HandleCursorChange(key, name, oldValue, newValue);
			}
		}

		private void HandleCursorChange(string key, string name, object oldValue, object newValue)
		{
			var args = new SentinelEventArgs();

			logger.Warn($@"The cursor registry value '{key}\{name}' has changed from '{oldValue}' to '{newValue}'!");

			Task.Run(() => CursorChanged?.Invoke(args)).ContinueWith((_) =>
			{
				if (args.Allow)
				{
					registry.StopMonitoring(key, name);
				}
			});
		}

		private bool VerifyCursor(string cursor)
		{
			var success = true;

			success &= registry.TryRead(RegistryValue.UserHive.Cursors_Key, cursor, out var value);
			success &= !(value is string) || (value is string path && (string.IsNullOrWhiteSpace(path) || IsValidCursorPath(path)));

			if (!success)
			{
				if (value != default)
				{
					logger.Warn($"Configuration of cursor '{cursor}' is compromised: '{value}'!");
				}
				else
				{
					logger.Warn($"Failed to verify configuration of cursor '{cursor}'!");
				}
			}

			return success;
		}

		private bool IsValidCursorPath(string path)
		{
			return path.StartsWith(USER_PATH, StringComparison.OrdinalIgnoreCase) || path.StartsWith(SYSTEM_PATH, StringComparison.OrdinalIgnoreCase);
		}
	}
}
