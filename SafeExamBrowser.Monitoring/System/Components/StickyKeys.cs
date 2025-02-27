/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using System.Timers;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System.Events;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.System.Components
{
	internal class StickyKeys
	{
		private readonly ILogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly Timer timer;

		private IStickyKeysState original;

		internal event SentinelEventHandler Changed;

		internal StickyKeys(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.timer = new Timer();
		}

		internal bool Disable()
		{
			var success = nativeMethods.TryGetStickyKeys(out var state);

			if (success)
			{
				success = nativeMethods.DisableStickyKeys();

				if (success)
				{
					original = state;
					logger.Info($"Disabled sticky keys (original state: {ToString(state)}).");
				}
				else
				{
					logger.Error($"Failed to disable sticky keys (original state: {ToString(state)})!");
				}
			}
			else
			{
				logger.Error("Failed to retrieve sticky keys state!");
			}

			return success;
		}

		internal bool Enable()
		{
			var success = nativeMethods.TryGetStickyKeys(out var state);

			if (success)
			{
				success = nativeMethods.EnableStickyKeys();

				if (success)
				{
					original = state;
					logger.Info($"Enabled sticky keys (original state: {ToString(state)}).");
				}
				else
				{
					logger.Error($"Failed to enable sticky keys (original state: {ToString(state)})!");
				}
			}
			else
			{
				logger.Error("Failed to retrieve sticky keys state!");
			}

			return success;
		}

		internal bool Revert()
		{
			var success = true;

			if (original != default)
			{
				success = nativeMethods.TrySetStickyKeys(original);

				if (success)
				{
					logger.Info($"Successfully reverted sticky keys (original state: {ToString(original)}).");
				}
				else
				{
					logger.Error($"Failed to revert sticky keys (original state: {ToString(original)})!");
				}
			}

			original = default;

			return success;
		}

		internal void StartMonitoring()
		{
			timer.AutoReset = true;
			timer.Elapsed += Timer_Elapsed;
			timer.Interval = 1000;
			timer.Start();

			logger.Info("Started monitoring sticky keys.");
		}

		internal void StopMonitoring()
		{
			timer.Elapsed -= Timer_Elapsed;
			timer.Stop();

			logger.Info("Stopped monitoring sticky keys.");
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (nativeMethods.TryGetStickyKeys(out var state))
			{
				if (state.IsEnabled || state.IsHotkeyActive)
				{
					HandleStickyKeysChange(state);
				}
			}
			else
			{
				logger.Error("Failed to monitor sticky keys state!");
			}
		}

		private void HandleStickyKeysChange(IStickyKeysState state)
		{
			var args = new SentinelEventArgs();

			logger.Warn($"The sticky keys state has changed: {ToString(state)}.");

			Task.Run(() => Changed?.Invoke(args)).ContinueWith((_) =>
			{
				if (args.Allow)
				{
					StopMonitoring();
				}
			});

			if (nativeMethods.DisableStickyKeys())
			{
				logger.Info($"Disabled sticky keys.");
			}
			else
			{
				logger.Error($"Failed to disable sticky keys!");
			}
		}

		private string ToString(IStickyKeysState state)
		{
			var availability = state.IsAvailable ? "available" : "unavailable";
			var status = state.IsEnabled ? "enabled" : "disabled";
			var hotkey = state.IsHotkeyActive ? "active" : "inactive";

			return $"functionality {availability} and {status}, hotkey {hotkey}";
		}
	}
}
