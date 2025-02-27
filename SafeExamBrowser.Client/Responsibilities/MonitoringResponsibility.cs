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
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Monitoring.Contracts.System.Events;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class MonitoringResponsibility : ClientResponsibility
	{
		private readonly IActionCenter actionCenter;
		private readonly IApplicationMonitor applicationMonitor;
		private readonly ICoordinator coordinator;
		private readonly IDisplayMonitor displayMonitor;
		private readonly IExplorerShell explorerShell;
		private readonly ISystemSentinel sentinel;
		private readonly ITaskbar taskbar;
		private readonly IText text;

		public MonitoringResponsibility(
			IActionCenter actionCenter,
			IApplicationMonitor applicationMonitor,
			ClientContext context,
			ICoordinator coordinator,
			IDisplayMonitor displayMonitor,
			IExplorerShell explorerShell,
			ILogger logger,
			ISystemSentinel sentinel,
			ITaskbar taskbar,
			IText text) : base(context, logger)
		{
			this.actionCenter = actionCenter;
			this.applicationMonitor = applicationMonitor;
			this.coordinator = coordinator;
			this.displayMonitor = displayMonitor;
			this.explorerShell = explorerShell;
			this.sentinel = sentinel;
			this.taskbar = taskbar;
			this.text = text;
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.DeregisterEvents:
					DeregisterEvents();
					break;
				case ClientTask.PrepareShutdown_Wave2:
					StopMonitoring();
					break;
				case ClientTask.RegisterEvents:
					RegisterEvents();
					break;
				case ClientTask.StartMonitoring:
					StartMonitoring();
					break;
			}
		}

		private void DeregisterEvents()
		{
			applicationMonitor.ExplorerStarted -= ApplicationMonitor_ExplorerStarted;
			applicationMonitor.TerminationFailed -= ApplicationMonitor_TerminationFailed;
			displayMonitor.DisplayChanged -= DisplayMonitor_DisplaySettingsChanged;
			sentinel.CursorChanged -= Sentinel_CursorChanged;
			sentinel.EaseOfAccessChanged -= Sentinel_EaseOfAccessChanged;
			sentinel.SessionChanged -= Sentinel_SessionChanged;
			sentinel.StickyKeysChanged -= Sentinel_StickyKeysChanged;
		}

		private void StopMonitoring()
		{
			sentinel.StopMonitoring();
		}

		private void RegisterEvents()
		{
			applicationMonitor.ExplorerStarted += ApplicationMonitor_ExplorerStarted;
			applicationMonitor.TerminationFailed += ApplicationMonitor_TerminationFailed;
			displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
			sentinel.CursorChanged += Sentinel_CursorChanged;
			sentinel.EaseOfAccessChanged += Sentinel_EaseOfAccessChanged;
			sentinel.SessionChanged += Sentinel_SessionChanged;
			sentinel.StickyKeysChanged += Sentinel_StickyKeysChanged;
		}

		private void StartMonitoring()
		{
			sentinel.StartMonitoringSystemEvents();

			if (!Settings.Security.AllowStickyKeys)
			{
				sentinel.StartMonitoringStickyKeys();
			}

			if (Settings.Security.VerifyCursorConfiguration)
			{
				sentinel.StartMonitoringCursors();
			}

			if (Settings.Service.IgnoreService)
			{
				sentinel.StartMonitoringEaseOfAccess();
			}
		}

		private void ApplicationMonitor_ExplorerStarted()
		{
			Logger.Info("Trying to terminate Windows explorer...");
			explorerShell.Terminate();

			Logger.Info("Re-initializing working area...");
			displayMonitor.InitializePrimaryDisplay(Settings.UserInterface.Taskbar.EnableTaskbar ? taskbar.GetAbsoluteHeight() : 0);

			Logger.Info("Re-initializing shell...");
			actionCenter.InitializeBounds();
			taskbar.InitializeBounds();

			Logger.Info("Desktop successfully restored.");
		}

		private void ApplicationMonitor_TerminationFailed(IEnumerable<RunningApplication> applications)
		{
			var applicationList = string.Join(Environment.NewLine, applications.Select(a => $"- {a.Name}"));
			var message = $"{text.Get(TextKey.LockScreen_ApplicationsMessage)}{Environment.NewLine}{Environment.NewLine}{applicationList}";
			var title = text.Get(TextKey.LockScreen_Title);
			var allowOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_ApplicationsAllowOption) };
			var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_ApplicationsTerminateOption) };

			Logger.Warn("Detected termination failure of blacklisted application(s)!");

			var result = ShowLockScreen(message, title, new[] { allowOption, terminateOption });

			if (result.OptionId == allowOption.Id)
			{
				Logger.Info($"The blacklisted application(s) {string.Join(", ", applications.Select(a => $"'{a.Name}'"))} will be temporarily allowed.");
			}
			else if (result.OptionId == terminateOption.Id)
			{
				Logger.Info("Attempting to shutdown as requested by the user...");
				TryRequestShutdown();
			}
		}

		private void DisplayMonitor_DisplaySettingsChanged()
		{
			Logger.Info("Re-initializing working area...");
			displayMonitor.InitializePrimaryDisplay(Settings.UserInterface.Taskbar.EnableTaskbar ? taskbar.GetAbsoluteHeight() : 0);

			Logger.Info("Re-initializing shell...");
			actionCenter.InitializeBounds();
			Context.LockScreen?.InitializeBounds();
			taskbar.InitializeBounds();

			Logger.Info("Desktop successfully restored.");

			if (!displayMonitor.ValidateConfiguration(Settings.Display).IsAllowed)
			{
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_DisplayConfigurationContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_DisplayConfigurationTerminateOption) };
				var message = text.Get(TextKey.LockScreen_DisplayConfigurationMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

				if (result.OptionId == terminateOption.Id)
				{
					Logger.Info("Attempting to shutdown as requested by the user...");
					TryRequestShutdown();
				}
			}
		}

		private void Sentinel_CursorChanged(SentinelEventArgs args)
		{
			if (coordinator.RequestSessionLock())
			{
				var message = text.Get(TextKey.LockScreen_CursorMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_CursorContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_CursorTerminateOption) };

				args.Allow = true;
				Logger.Info("Cursor changed! Attempting to show lock screen...");

				var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

				if (result.OptionId == continueOption.Id)
				{
					Logger.Info("The session will be allowed to resume as requested by the user...");
				}
				else if (result.OptionId == terminateOption.Id)
				{
					Logger.Info("Attempting to shutdown as requested by the user...");
					TryRequestShutdown();
				}

				coordinator.ReleaseSessionLock();
			}
			else
			{
				Logger.Info("Cursor changed but lock screen is already active.");
			}
		}

		private void Sentinel_EaseOfAccessChanged(SentinelEventArgs args)
		{
			if (coordinator.RequestSessionLock())
			{
				var message = text.Get(TextKey.LockScreen_EaseOfAccessMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_EaseOfAccessContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_EaseOfAccessTerminateOption) };

				args.Allow = true;
				Logger.Info("Ease of access changed! Attempting to show lock screen...");

				var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

				if (result.OptionId == continueOption.Id)
				{
					Logger.Info("The session will be allowed to resume as requested by the user...");
				}
				else if (result.OptionId == terminateOption.Id)
				{
					Logger.Info("Attempting to shutdown as requested by the user...");
					TryRequestShutdown();
				}

				coordinator.ReleaseSessionLock();
			}
			else
			{
				Logger.Info("Ease of access changed but lock screen is already active.");
			}
		}

		private void Sentinel_SessionChanged()
		{
			var allow = !Settings.Service.IgnoreService && (!Settings.Service.DisableUserLock || !Settings.Service.DisableUserSwitch);
			var disable = Settings.Security.DisableSessionChangeLockScreen;

			if (allow || disable)
			{
				Logger.Info($"Detected user session change, but {(allow ? "session locking and/or switching is allowed" : "lock screen is deactivated")}.");
			}
			else
			{
				var message = text.Get(TextKey.LockScreen_UserSessionMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_UserSessionContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_UserSessionTerminateOption) };

				Logger.Warn("User session changed! Attempting to show lock screen...");

				if (coordinator.RequestSessionLock())
				{
					var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

					if (result.OptionId == continueOption.Id)
					{
						Logger.Info("The session will be allowed to resume as requested by the user...");
					}
					else if (result.OptionId == terminateOption.Id)
					{
						Logger.Info("Attempting to shutdown as requested by the user...");
						TryRequestShutdown();
					}

					coordinator.ReleaseSessionLock();
				}
				else
				{
					Logger.Warn("User session changed but lock screen is already active.");
				}
			}
		}

		private void Sentinel_StickyKeysChanged(SentinelEventArgs args)
		{
			if (coordinator.RequestSessionLock())
			{
				var message = text.Get(TextKey.LockScreen_StickyKeysMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_StickyKeysContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_StickyKeysTerminateOption) };

				args.Allow = true;
				Logger.Info("Sticky keys changed! Attempting to show lock screen...");

				var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

				if (result.OptionId == continueOption.Id)
				{
					Logger.Info("The session will be allowed to resume as requested by the user...");
				}
				else if (result.OptionId == terminateOption.Id)
				{
					Logger.Info("Attempting to shutdown as requested by the user...");
					TryRequestShutdown();
				}

				coordinator.ReleaseSessionLock();
			}
			else
			{
				Logger.Info("Sticky keys changed but lock screen is already active.");
			}
		}
	}
}
