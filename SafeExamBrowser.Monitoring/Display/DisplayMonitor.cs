/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Monitoring.Contracts.Display.Events;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using OperatingSystem = SafeExamBrowser.SystemComponents.Contracts.OperatingSystem;

namespace SafeExamBrowser.Monitoring.Display
{
	public class DisplayMonitor : IDisplayMonitor
	{
		private IBounds originalWorkingArea;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private ISystemInfo systemInfo;
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

		public void PreventSleepMode()
		{
			nativeMethods.PreventSleepMode();
			logger.Info("Disabled sleep mode and display timeout.");
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

				if (!String.IsNullOrEmpty(path))
				{
					wallpaper = path;
					logger.Info($"Saved wallpaper image: {wallpaper}.");
				}

				nativeMethods.RemoveWallpaper();
				logger.Info("Removed current wallpaper.");
			}
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
			var display = Screen.PrimaryScreen.DeviceName?.Replace(@"\\.\", string.Empty);

			return $"{display} ({Screen.PrimaryScreen.Bounds.Width}x{Screen.PrimaryScreen.Bounds.Height})";
		}

		private void LogWorkingArea(string message, IBounds area)
		{
			logger.Info($"{message}: Left = {area.Left}, Top = {area.Top}, Right = {area.Right}, Bottom = {area.Bottom}.");
		}
	}
}
