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

		public bool TryGetNames(string key, out IEnumerable<string> names)
		{
			names = default;

			if (!TryOpenKey(key, out var keyObj))
				return false;

			bool success = true;
			using (keyObj)
			{
				try
				{
					names = keyObj.GetValueNames();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to get registry value names '{key}'!", e);
					success = false;
					// persist keyObj dispose operation by finishing using() {}
				}

			}

			return success;
		}

		public bool TryGetSubKeys(string key, out IEnumerable<string> subKeys)
		{
			subKeys = default;
			
			if (!TryOpenKey(key, out var keyObj))
				return false;

			bool success = true;
			using (keyObj)
			{
				try
				{
					subKeys = keyObj.GetSubKeyNames();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to get registry value names '{key}'!", e);
					success = false;
					// persist keyObj dispose operation by finishing using() {}
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

		/// <summary>
		/// Parses a keyName and returns the basekey for it.
		/// It will also store the subkey name in the out parameter.
		/// If the keyName is not valid, we will return false.
		/// Does not raise Exceptions.
		/// Supports shortcuts.
		/// </summary>
		// yoinked (and partially modified to follow SEB conventions) private Win32 function: https://stackoverflow.com/a/58547945
		private bool GetBaseKeyFromKeyName(string keyName, out RegistryKey hiveKey, out string subKeyName)
		{
			hiveKey = default;
			subKeyName = default;

			string basekeyName;
			int i = keyName.IndexOf('\\');
			if (i != -1)
			{
				basekeyName = keyName.Substring(0, i).ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			}
			else
			{
				basekeyName = keyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			}

			// add shortcuts as well to be implicit
			switch (basekeyName)
			{
				case "HKEY_CURRENT_USER":
				case "HKCU":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
					break;
				case "HKEY_LOCAL_MACHINE":
				case "HKLM":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
					break;
				case "HKEY_CLASSES_ROOT":
				case "HKCR":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
					break;
				case "HKEY_USERS":
				case "HKU":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
					break;
				case "HKEY_PERFORMANCE_DATA":
				case "HKPD":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.PerformanceData, RegistryView.Registry64);
					break;
				case "HKEY_CURRENT_CONFIG":
				case "HKCC":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64);
					break;
				case "HKEY_DYN_DATA":
				case "HKDD":
					hiveKey = RegistryKey.OpenBaseKey(RegistryHive.DynData, RegistryView.Registry64);
					break;
				default:
					// output is already set to null at the start
					return false;
			}

			if (i == -1 || i == keyName.Length)
			{
				subKeyName = string.Empty;
			}
			else
			{
				subKeyName = keyName.Substring(i + 1, keyName.Length - i - 1);
			}

			return true;
		}

		/// <summary>
		/// Tries to open a key and outputs a RegistryKey object. Does not raise Exceptions, but returns false/true.
		/// </summary>
		private bool TryOpenKey(string key, out RegistryKey keyObj)
		{
			keyObj = default;

			try
			{
				if (!GetBaseKeyFromKeyName(key, out var hiveObj, out var subHiveKey))
					return false;

				keyObj = hiveObj.OpenSubKey(subHiveKey);
				if (keyObj == null)
				{
					keyObj = default;
					return false;
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to open registry key '{key}'!", e);
				return false;
			}

			return true;
		}
	}
}
