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
using System.Security.Cryptography;
using System.Timers;
using Microsoft.Win32;
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

		public bool TryGetNames(string keyName, out IEnumerable<string> names)
		{
			names = default;

			if (!TryOpenKey(keyName, out var key))
			{
				return false;
			}

			var success = true;
			using (key)
			{
				try
				{
					names = key.GetValueNames();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to get registry value names '{keyName}'!", e);
					success = false;
				}

			}

			return success;
		}

		public bool TryGetSubKeys(string keyName, out IEnumerable<string> subKeys)
		{
			subKeys = default;

			if (!TryOpenKey(keyName, out var key))
			{
				return false;
			}

			var success = true;
			using (key)
			{
				try
				{
					subKeys = key.GetSubKeyNames();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to get registry value names '{keyName}'!", e);
					success = false;
				}
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

		private bool TryGetBaseKeyFromKeyName(string keyName, out RegistryKey baseKey, out string subKeyName)
		{
			baseKey = default;
			subKeyName = default;

			string basekeyName;
			var baseKeyLength = keyName.IndexOf('\\');
			if (baseKeyLength != -1)
			{
				basekeyName = keyName.Substring(0, baseKeyLength).ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			}
			else
			{
				basekeyName = keyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			}

			switch (basekeyName)
			{
				case "HKEY_CURRENT_USER":
				case "HKCU":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
					break;
				case "HKEY_LOCAL_MACHINE":
				case "HKLM":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
					break;
				case "HKEY_CLASSES_ROOT":
				case "HKCR":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
					break;
				case "HKEY_USERS":
				case "HKU":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
					break;
				case "HKEY_PERFORMANCE_DATA":
				case "HKPD":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.PerformanceData, RegistryView.Registry64);
					break;
				case "HKEY_CURRENT_CONFIG":
				case "HKCC":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64);
					break;
				case "HKEY_DYN_DATA":
				case "HKDD":
					baseKey = RegistryKey.OpenBaseKey(RegistryHive.DynData, RegistryView.Registry64);
					break;
				default:
					return false;
			}

			if (baseKeyLength == -1 || baseKeyLength == keyName.Length)
			{
				subKeyName = string.Empty;
			}
			else
			{
				subKeyName = keyName.Substring(baseKeyLength + 1, keyName.Length - baseKeyLength - 1);
			}

			return true;
		}

		private bool TryOpenKey(string keyName, out RegistryKey key)
		{
			key = default;

			try
			{
				logger.Info($"default(RegistryKey) == null: {key == null}");
				if (TryGetBaseKeyFromKeyName(keyName, out var baseKey, out var subKey))
				{
					key = baseKey.OpenSubKey(subKey);
				}

			}
			catch (Exception e)
			{
				logger.Error($"Failed to open registry key '{keyName}'!", e);
				return false;
			}

			return key != default;
		}
	}
}
