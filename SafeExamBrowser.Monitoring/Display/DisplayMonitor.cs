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
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Monitoring.Contracts.Display.Events;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using OperatingSystem = SafeExamBrowser.SystemComponents.Contracts.OperatingSystem;

namespace SafeExamBrowser.Monitoring.Display
{
	public class DisplayMonitor : IDisplayMonitor
	{
		private IBounds originalWorkingArea;
		private readonly ILogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly ISystemInfo systemInfo;
		private string wallpaper;

		public event DisplayChangedEventHandler DisplayChanged;

		public DisplayMonitor(ILogger logger, INativeMethods nativeMethods, ISystemInfo systemInfo)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.systemInfo = systemInfo;
		}

		public void InitializePrimaryDisplay(int taskbarHeight)
		{
			InitializeWorkingArea(taskbarHeight);
			InitializeWallpaper();
		}

		public void ResetPrimaryDisplay()
		{
			ResetWorkingArea();
			ResetWallpaper();
		}

		public void StartMonitoringDisplayChanges()
		{
			SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
			logger.Info("Started monitoring display changes.");
		}

		public void StopMonitoringDisplayChanges()
		{
			SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
			logger.Info("Stopped monitoring display changes.");
		}

		public ValidationResult ValidateConfiguration(DisplaySettings settings)
		{
			var result = new ValidationResult();

			if (TryLoadDisplays(out var displays))
			{
				var active = displays.Where(d => d.IsActive);
				var count = active.Count();

				result.ExternalDisplays = active.Count(d => !d.IsInternal);
				result.InternalDisplays = active.Count(d => d.IsInternal);
				result.IsAllowed = count <= settings.AllowedDisplays;

				if (result.IsAllowed)
				{
					logger.Info($"Detected {count} active displays, {settings.AllowedDisplays} are allowed.");
				}
				else
				{
					logger.Warn($"Detected {count} active displays but only {settings.AllowedDisplays} are allowed!");
				}

				if (settings.InternalDisplayOnly && active.Any(d => !d.IsInternal))
				{
					result.IsAllowed = false;
					logger.Warn("Detected external display but only internal displays are allowed!");
				}
			}
			else
			{
				result.IsAllowed = settings.IgnoreError;
				logger.Warn($"Failed to validate display configuration, {(result.IsAllowed ? "ignoring error" : "active configuration is not allowed")}.");
			}

			return result;
		}

		private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
		{
			logger.Info("Display change detected!");
			Task.Run(() => DisplayChanged?.Invoke());
		}

		private void InitializeWorkingArea(int taskbarHeight)
		{
			var identifier = GetIdentifierForPrimaryDisplay();

			if (originalWorkingArea == null)
			{
				originalWorkingArea = nativeMethods.GetWorkingArea();
				LogWorkingArea($"Saved original working area for {identifier}", originalWorkingArea);
			}

			var area = new Bounds
			{
				Left = 0,
				Top = 0,
				Right = Screen.PrimaryScreen.Bounds.Width,
				Bottom = Screen.PrimaryScreen.Bounds.Height - taskbarHeight
			};

			LogWorkingArea($"Trying to set new working area for {identifier}", area);
			nativeMethods.SetWorkingArea(area);
			LogWorkingArea($"Working area of {identifier} is now set to", nativeMethods.GetWorkingArea());
		}

		private void InitializeWallpaper()
		{
			if (systemInfo.OperatingSystem == OperatingSystem.Windows7)
			{
				var path = nativeMethods.GetWallpaperPath();

				if (!string.IsNullOrEmpty(path))
				{
					wallpaper = path;
					logger.Info($"Saved wallpaper image: {wallpaper}.");
				}

				nativeMethods.RemoveWallpaper();
				logger.Info("Removed current wallpaper.");
			}
		}

		private bool TryLoadDisplays(out IList<Display> displays)
		{
			var success = true;

			displays = new List<Display>();

			try
			{
				using (var searcher = new ManagementObjectSearcher(@"Root\WMI", "SELECT * FROM WmiMonitorBasicDisplayParams"))
				using (var results = searcher.Get())
				{
					var displayParameters = results.Cast<ManagementObject>();

					foreach (var display in displayParameters)
					{
						displays.Add(new Display
						{
							Identifier = Convert.ToString(display["InstanceName"]),
							IsActive = Convert.ToBoolean(display["Active"])
						});
					}
				}

				using (var searcher = new ManagementObjectSearcher(@"Root\WMI", "SELECT * FROM WmiMonitorConnectionParams"))
				using (var results = searcher.Get())
				{
					var connectionParameters = results.Cast<ManagementObject>();

					foreach (var connection in connectionParameters)
					{
						var identifier = Convert.ToString(connection["InstanceName"]);
						var isActive = Convert.ToBoolean(connection["Active"]);
						var technologyValue = Convert.ToInt64(connection["VideoOutputTechnology"]);
						var technology = (VideoOutputTechnology) technologyValue;
						var display = displays.FirstOrDefault(d => d.Identifier?.Equals(identifier, StringComparison.OrdinalIgnoreCase) == true);

						if (!Enum.IsDefined(typeof(VideoOutputTechnology), technology))
						{
							logger.Warn($"Detected undefined video output technology '{technologyValue}' for display '{identifier}'!");
						}

						if (display != default(Display))
						{
							display.IsActive &= isActive;
							display.Technology = technology;
						}
					}
				}
			}
			catch (Exception e)
			{
				success = false;
				logger.Error("Failed to query displays!", e);
			}

			foreach (var display in displays)
			{
				logger.Info($"Detected {(display.IsActive ? "active" : "inactive")}, {(display.IsInternal ? "internal" : "external")} display '{display.Identifier}' connected via '{display.Technology}'.");
			}

			return success;
		}

		private void ResetWorkingArea()
		{
			var identifier = GetIdentifierForPrimaryDisplay();

			if (originalWorkingArea != null)
			{
				nativeMethods.SetWorkingArea(originalWorkingArea);
				LogWorkingArea($"Restored original working area for {identifier}", originalWorkingArea);
			}
			else
			{
				logger.Warn($"Could not restore original working area for {identifier}!");
			}
		}

		private void ResetWallpaper()
		{
			if (systemInfo.OperatingSystem == OperatingSystem.Windows7 && !String.IsNullOrEmpty(wallpaper))
			{
				nativeMethods.SetWallpaper(wallpaper);
				logger.Info($"Restored wallpaper image: {wallpaper}.");
			}
		}

		private string GetIdentifierForPrimaryDisplay()
		{
			var name = Screen.PrimaryScreen.DeviceName?.Replace(@"\\.\", string.Empty);
			var resolution = $"{Screen.PrimaryScreen.Bounds.Width}x{Screen.PrimaryScreen.Bounds.Height}";
			var identifier = $"{name} ({resolution})";

			return identifier;
		}

		private void LogWorkingArea(string message, IBounds area)
		{
			logger.Info($"{message}: Left = {area.Left}, Top = {area.Top}, Right = {area.Right}, Bottom = {area.Bottom}.");
		}
	}
}
