/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System.Events;

namespace SafeExamBrowser.Monitoring.System.Components
{
	internal class SystemEvents
	{
		private readonly ILogger logger;

		internal event SessionChangedEventHandler SessionChanged;

		internal SystemEvents(ILogger logger)
		{
			this.logger = logger;
		}

		internal void StartMonitoring()
		{
			Microsoft.Win32.SystemEvents.EventsThreadShutdown += SystemEvents_EventsThreadShutdown;
			Microsoft.Win32.SystemEvents.InstalledFontsChanged += SystemEvents_InstalledFontsChanged;
			Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
			Microsoft.Win32.SystemEvents.SessionEnded += SystemEvents_SessionEnded;
			Microsoft.Win32.SystemEvents.SessionEnding += SystemEvents_SessionEnding;
			Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionChanged;
			Microsoft.Win32.SystemEvents.TimeChanged += SystemEvents_TimeChanged;
			Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

			logger.Info("Started monitoring system events.");
		}

		internal void StopMonitoring()
		{
			Microsoft.Win32.SystemEvents.EventsThreadShutdown -= SystemEvents_EventsThreadShutdown;
			Microsoft.Win32.SystemEvents.InstalledFontsChanged -= SystemEvents_InstalledFontsChanged;
			Microsoft.Win32.SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
			Microsoft.Win32.SystemEvents.SessionEnded -= SystemEvents_SessionEnded;
			Microsoft.Win32.SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
			Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionChanged;
			Microsoft.Win32.SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
			Microsoft.Win32.SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

			logger.Info("Stopped monitoring system events.");
		}

		private void SystemEvents_EventsThreadShutdown(object sender, EventArgs e)
		{
			logger.Warn("System event thread is about to be terminated!");
		}

		private void SystemEvents_InstalledFontsChanged(object sender, EventArgs e)
		{
			logger.Info("Installed fonts changed.");
		}

		private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			logger.Info($"Power mode changed: {e.Mode}.");
		}

		private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
		{
			logger.Warn($"User session ended! Reason: {e.Reason}.");
		}

		private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
		{
			logger.Warn($"User session is ending! Reason: {e.Reason}.");
		}

		private void SystemEvents_SessionChanged(object sender, SessionSwitchEventArgs e)
		{
			logger.Info($"User session change detected: {e.Reason}.");
			Task.Run(() => SessionChanged?.Invoke());
		}

		private void SystemEvents_TimeChanged(object sender, EventArgs e)
		{
			logger.Info("Time changed.");
		}

		private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			logger.Info($"User preference changed. Category: {e.Category}.");
		}
	}
}
