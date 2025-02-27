/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Events;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using MouseButton = SafeExamBrowser.WindowsApi.Contracts.Events.MouseButton;
using MouseButtonState = SafeExamBrowser.WindowsApi.Contracts.Events.MouseButtonState;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class DataCollector
	{
		private readonly object @lock = new object();

		private readonly IApplicationMonitor applicationMonitor;
		private readonly IBrowserApplication browser;
		private readonly IModuleLogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly ScreenProctoringSettings settings;
		private readonly Timer timer;

		private DateTime last;
		private Guid? keyboardHookId;
		private Guid? mouseHookId;

		internal event DataCollectedEventHandler DataCollected;

		internal DataCollector(
			IApplicationMonitor applicationMonitor,
			IBrowserApplication browser,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			ScreenProctoringSettings settings)
		{
			this.applicationMonitor = applicationMonitor;
			this.browser = browser;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.settings = settings;
			this.timer = new Timer();
		}

		internal void Start()
		{
			last = DateTime.Now;

			keyboardHookId = nativeMethods.RegisterKeyboardHook(KeyboardHookCallback);
			mouseHookId = nativeMethods.RegisterMouseHook(MouseHookCallback);

			timer.AutoReset = false;
			timer.Elapsed += IntervalMaximumElapsed;
			timer.Interval = settings.IntervalMaximum;
			timer.Start();

			logger.Debug("Started.");
		}

		internal void Stop()
		{
			last = DateTime.Now;

			if (keyboardHookId.HasValue)
			{
				nativeMethods.DeregisterKeyboardHook(keyboardHookId.Value);
			}

			if (mouseHookId.HasValue)
			{
				nativeMethods.DeregisterMouseHook(mouseHookId.Value);
			}

			keyboardHookId = default;
			mouseHookId = default;

			timer.Elapsed -= IntervalMaximumElapsed;
			timer.Stop();

			logger.Debug("Stopped.");
		}

		private bool KeyboardHookCallback(int keyCode, KeyModifier modifier, KeyState state)
		{
			var trigger = new KeyboardTrigger
			{
				Key = KeyInterop.KeyFromVirtualKey(keyCode),
				Modifier = modifier,
				State = state
			};

			TryCollect(keyboard: trigger);

			return false;
		}

		private void IntervalMaximumElapsed(object sender, ElapsedEventArgs args)
		{
			var trigger = new IntervalTrigger
			{
				ConfigurationValue = settings.IntervalMaximum,
			};

			TryCollect(interval: trigger);
		}

		private bool MouseHookCallback(MouseButton button, MouseButtonState state, MouseInformation info)
		{
			var trigger = new MouseTrigger
			{
				Button = button,
				Info = info,
				State = state
			};

			TryCollect(mouse: trigger);

			return false;
		}

		private void TryCollect(IntervalTrigger interval = default, KeyboardTrigger keyboard = default, MouseTrigger mouse = default)
		{
			if (HasIntervalMinimumElapsed() && Monitor.TryEnter(@lock))
			{
				var elapsed = DateTime.Now.Subtract(last);

				last = DateTime.Now;
				timer.Stop();

				Task.Run(() =>
				{
					try
					{
						var metaData = new MetaDataAggregator(applicationMonitor, browser, elapsed, logger.CloneFor(nameof(MetaDataAggregator)), settings.MetaData);
						var screenShot = new ScreenShotProcessor(logger.CloneFor(nameof(ScreenShotProcessor)), settings);

						metaData.Capture(interval, keyboard, mouse);
						screenShot.Take();
						screenShot.Compress();

						DataCollected?.Invoke(metaData.Data, screenShot.Data);

						screenShot.Dispose();
					}
					catch (Exception e)
					{
						logger.Error("Failed to execute data collection!", e);
					}
				});

				timer.Start();
				Monitor.Exit(@lock);
			}
		}

		private bool HasIntervalMinimumElapsed()
		{
			return DateTime.Now.Subtract(last) >= new TimeSpan(0, 0, 0, 0, settings.IntervalMinimum);
		}
	}
}
