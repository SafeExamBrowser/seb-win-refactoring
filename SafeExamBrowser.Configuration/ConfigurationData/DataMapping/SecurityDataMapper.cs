/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class SecurityDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Security.AdminPasswordHash:
					MapAdminPasswordHash(settings, value);
					break;
				case Keys.Security.AllowReconfiguration:
					MapAllowReconfiguration(settings, value);
					break;
				case Keys.Security.AllowStickyKeys:
					MapAllowStickyKeys(settings, value);
					break;
				case Keys.Security.AllowTermination:
					MapAllowTermination(settings, value);
					break;
				case Keys.Security.AllowVirtualMachine:
					MapVirtualMachinePolicy(settings, value);
					break;
				case Keys.Security.AllowWindowCapture:
					MapAllowWindowCapture(settings, value);
					break;
				case Keys.Security.ClipboardPolicy:
					MapClipboardPolicy(settings, value);
					break;
				case Keys.Security.DisableSessionChangeLockScreen:
					MapDisableSessionChangeLockScreen(settings, value);
					break;
				case Keys.Security.QuitPasswordHash:
					MapQuitPasswordHash(settings, value);
					break;
				case Keys.Security.ReconfigurationUrl:
					MapReconfigurationUrl(settings, value);
					break;
				case Keys.Security.VerifyCursorConfiguration:
					MapVerifyCursorConfiguration(settings, value);
					break;
				case Keys.Security.VerifySessionIntegrity:
					MapVerifySessionIntegrity(settings, value);
					break;
				case Keys.Security.VersionRestrictions:
					MapVersionRestrictions(settings, value);
					break;
			}
		}

		internal override void MapGlobal(IDictionary<string, object> rawData, AppSettings settings)
		{
			MapApplicationLogAccess(rawData, settings);
			MapKioskMode(rawData, settings);
		}

		private void MapAdminPasswordHash(AppSettings settings, object value)
		{
			if (value is string hash)
			{
				settings.Security.AdminPasswordHash = hash;
			}
		}

		private void MapAllowReconfiguration(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Security.AllowReconfiguration = allow;
			}
		}

		private void MapAllowStickyKeys(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Security.AllowStickyKeys = allow;
			}
		}

		private void MapAllowTermination(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Security.AllowTermination = allow;
			}
		}

		private void MapAllowWindowCapture(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Security.AllowWindowCapture = allow;
			}
		}

		private void MapApplicationLogAccess(IDictionary<string, object> rawData, AppSettings settings)
		{
			var hasValue = rawData.TryGetValue(Keys.Security.AllowApplicationLog, out var value);

			if (hasValue && value is bool allow)
			{
				settings.Security.AllowApplicationLogAccess = allow;
			}

			if (settings.Security.AllowApplicationLogAccess)
			{
				settings.UserInterface.ActionCenter.ShowApplicationLog = true;
			}
			else
			{
				settings.UserInterface.ActionCenter.ShowApplicationLog = false;
				settings.UserInterface.Taskbar.ShowApplicationLog = false;
			}
		}

		private void MapKioskMode(IDictionary<string, object> rawData, AppSettings settings)
		{
			var hasCreateNewDesktop = rawData.TryGetValue(Keys.Security.KioskModeCreateNewDesktop, out var createNewDesktop);
			var hasDisableExplorerShell = rawData.TryGetValue(Keys.Security.KioskModeDisableExplorerShell, out var disableExplorerShell);

			if (hasDisableExplorerShell && disableExplorerShell as bool? == true)
			{
				settings.Security.KioskMode = KioskMode.DisableExplorerShell;
			}

			if (hasCreateNewDesktop && createNewDesktop as bool? == true)
			{
				settings.Security.KioskMode = KioskMode.CreateNewDesktop;
			}

			if (hasCreateNewDesktop && hasDisableExplorerShell && createNewDesktop as bool? == false && disableExplorerShell as bool? == false)
			{
				settings.Security.KioskMode = KioskMode.None;
			}
		}

		private void MapQuitPasswordHash(AppSettings settings, object value)
		{
			if (value is string hash)
			{
				settings.Security.QuitPasswordHash = hash;
			}
		}

		private void MapClipboardPolicy(AppSettings settings, object value)
		{
			const int ALLOW = 0;
			const int BLOCK = 1;

			if (value is int policy)
			{
				settings.Security.ClipboardPolicy = policy == ALLOW ? ClipboardPolicy.Allow : (policy == BLOCK ? ClipboardPolicy.Block : ClipboardPolicy.Isolated);
			}
		}

		private void MapDisableSessionChangeLockScreen(AppSettings settings, object value)
		{
			if (value is bool disable)
			{
				settings.Security.DisableSessionChangeLockScreen = disable;
			}
		}

		private void MapVirtualMachinePolicy(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Security.VirtualMachinePolicy = allow ? VirtualMachinePolicy.Allow : VirtualMachinePolicy.Deny;
			}
		}

		private void MapReconfigurationUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Security.ReconfigurationUrl = url;
			}
		}

		private void MapVerifyCursorConfiguration(AppSettings settings, object value)
		{
			if (value is bool verify)
			{
				settings.Security.VerifyCursorConfiguration = verify;
			}
		}

		private void MapVerifySessionIntegrity(AppSettings settings, object value)
		{
			if (value is bool verify)
			{
				settings.Security.VerifySessionIntegrity = verify;
			}
		}

		private void MapVersionRestrictions(AppSettings settings, object value)
		{
			if (value is IList<object> restrictions)
			{
				foreach (var restriction in restrictions.Cast<string>())
				{
					var parts = restriction.Split('.');
					var os = parts.Length > 0 ? parts[0] : default;

					if (os?.Equals("win", StringComparison.OrdinalIgnoreCase) == true)
					{
						var major = parts.Length > 1 ? int.Parse(parts[1]) : default;
						var minor = parts.Length > 2 ? int.Parse(parts[2]) : default;
						var patch = parts.Length > 3 && int.TryParse(parts[3], out _) ? int.Parse(parts[3]) : default(int?);
						var build = parts.Length > 4 && int.TryParse(parts[4], out _) ? int.Parse(parts[4]) : default(int?);

						settings.Security.VersionRestrictions.Add(new VersionRestriction
						{
							Major = major,
							Minor = minor,
							Patch = patch,
							Build = build,
							IsMinimumRestriction = restriction.Contains("min"),
							RequiresAllianceEdition = restriction.Contains("AE")
						});
					}
				}
			}
		}
	}
}
