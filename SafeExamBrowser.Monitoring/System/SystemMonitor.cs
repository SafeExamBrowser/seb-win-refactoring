/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Monitoring.Contracts.System.Events;

namespace SafeExamBrowser.Monitoring.System
{
	public class SystemMonitor : ISystemMonitor
	{
		private readonly ILogger logger;

		public event SessionSwitchedEventHandler SessionSwitched;

		public SystemMonitor(ILogger logger)
		{
			this.logger = logger;
		}

		public void Start()
		{
			SystemEvents.EventsThreadShutdown += SystemEvents_EventsThreadShutdown;
			SystemEvents.InstalledFontsChanged += SystemEvents_InstalledFontsChanged;
			SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
			SystemEvents.SessionEnded += SystemEvents_SessionEnded;
			SystemEvents.SessionEnding += SystemEvents_SessionEnding;
			SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
			SystemEvents.TimeChanged += SystemEvents_TimeChanged;
			SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
			logger.Info("Started monitoring the operating system.");
		}

		public void Stop()
		{
			SystemEvents.EventsThreadShutdown -= SystemEvents_EventsThreadShutdown;
			SystemEvents.InstalledFontsChanged -= SystemEvents_InstalledFontsChanged;
			SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
			SystemEvents.SessionEnded -= SystemEvents_SessionEnded;
			SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
			SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
			SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
			SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
			logger.Info("Stopped monitoring the operating system.");
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
			logger.Info($"Power mode changed: {e.Mode}");
		}

		private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
		{
			logger.Warn($"User session ended! Reason: {e.Reason}");
		}

		private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
		{
			logger.Warn($"User session is ending! Reason: {e.Reason}");
		}

		private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			logger.Info($"User session switch detected! Reason: {e.Reason}");
			Task.Run(() => SessionSwitched?.Invoke());
		}

		private void SystemEvents_TimeChanged(object sender, EventArgs e)
		{
			logger.Info("Time changed.");
		}

		private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			logger.Info($"User preference changed. Category: {e.Category}");
		}
	}
}
