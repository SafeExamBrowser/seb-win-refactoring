/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service;
using SafeExamBrowser.Server.Contracts.Events.Proctoring;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using MouseButton = SafeExamBrowser.WindowsApi.Contracts.Events.MouseButton;
using MouseButtonState = SafeExamBrowser.WindowsApi.Contracts.Events.MouseButtonState;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class ScreenProctoringImplementation : ProctoringImplementation
	{
		private readonly object @lock = new object();

		private readonly IApplicationMonitor applicationMonitor;
		private readonly IBrowserApplication browser;
		private readonly IModuleLogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly ServiceProxy service;
		private readonly ScreenProctoringSettings settings;
		private readonly IText text;
		private readonly Timer timer;

		private DateTime last;
		private Guid? keyboardHookId;
		private Guid? mouseHookId;

		internal override string Name => nameof(ScreenProctoring);

		public override event NotificationChangedEventHandler NotificationChanged;

		internal ScreenProctoringImplementation(
			IApplicationMonitor applicationMonitor,
			IBrowserApplication browser,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			ServiceProxy service,
			ProctoringSettings settings,
			IText text)
		{
			this.applicationMonitor = applicationMonitor;
			this.browser = browser;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.service = service;
			this.settings = settings.ScreenProctoring;
			this.text = text;
			this.timer = new Timer();
		}

		internal override void Initialize()
		{
			var start = true;

			start &= !string.IsNullOrWhiteSpace(settings.ClientId);
			start &= !string.IsNullOrWhiteSpace(settings.ClientSecret);
			start &= !string.IsNullOrWhiteSpace(settings.GroupId);
			start &= !string.IsNullOrWhiteSpace(settings.ServiceUrl);

			timer.AutoReset = false;
			timer.Interval = settings.MaxInterval;

			if (start)
			{
				logger.Info($"Initialized proctoring: All settings are valid, starting automatically...");

				Connect();
				Start();
			}
			else
			{
				ShowNotificationInactive();
				logger.Info($"Initialized proctoring: Not all settings are valid or a server session is active, not starting automatically.");
			}
		}

		internal override void ProctoringConfigurationReceived(bool allowChat, bool receiveAudio, bool receiveVideo)
		{
			// Nothing to do here for now...
		}

		internal override void ProctoringInstructionReceived(InstructionEventArgs args)
		{
			if (args is ScreenProctoringInstruction instruction)
			{
				logger.Info($"Proctoring instruction received: {instruction.Method}.");

				if (instruction.Method == InstructionMethod.Join)
				{
					settings.ClientId = instruction.ClientId;
					settings.ClientSecret = instruction.ClientSecret;
					settings.GroupId = instruction.GroupId;
					settings.ServiceUrl = instruction.ServiceUrl;

					Connect(instruction.SessionId);
					Start();
				}
				else
				{
					Stop();
				}

				logger.Info("Successfully processed instruction.");
			}
		}
		internal override void Start()
		{
			last = DateTime.Now;
			keyboardHookId = nativeMethods.RegisterKeyboardHook(KeyboardHookCallback);
			mouseHookId = nativeMethods.RegisterMouseHook(MouseHookCallback);

			timer.Elapsed += Timer_Elapsed;
			timer.Start();

			ShowNotificationActive();

			logger.Info($"Started proctoring.");
		}

		internal override void Stop()
		{
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

			timer.Elapsed -= Timer_Elapsed;
			timer.Stop();

			TerminateServiceSession();
			ShowNotificationInactive();

			logger.Info("Stopped proctoring.");
		}

		internal override void Terminate()
		{
			Stop();
			TerminateNotification();

			logger.Info("Terminated proctoring.");
		}

		protected override void ActivateNotification()
		{
			// Nothing to do here for now...
		}

		protected override void TerminateNotification()
		{
			// Nothing to do here for now...
		}

		private void Connect(string sessionId = default)
		{
			logger.Info("Connecting to service...");

			var connect = service.Connect(settings.ServiceUrl);

			if (connect.Success)
			{
				if (sessionId == default)
				{
					logger.Info("Creating session...");
					service.CreateSession(settings.GroupId);
				}
				else
				{
					service.SessionId = sessionId;
				}
			}
		}

		private bool KeyboardHookCallback(int keyCode, KeyModifier modifier, KeyState state)
		{
			var trigger = new KeyboardTrigger
			{
				Key = KeyInterop.KeyFromVirtualKey(keyCode),
				Modifier = modifier,
				State = state
			};

			TryExecute(keyboard: trigger);

			return false;
		}

		private bool MouseHookCallback(MouseButton button, MouseButtonState state, MouseInformation info)
		{
			var trigger = new MouseTrigger
			{
				Button = button,
				Info = info,
				State = state
			};

			TryExecute(mouse: trigger);

			return false;
		}

		private void ShowNotificationActive()
		{
			// TODO: Replace with actual icon!
			// TODO: Extend INotification with IsEnabled or CanActivate, as the screen proctoring notification does not have any action or window!
			IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ProctoringNotification_Active.xaml") };
			Tooltip = text.Get(TextKey.Notification_ProctoringActiveTooltip);
			NotificationChanged?.Invoke();
		}

		private void ShowNotificationInactive()
		{
			// TODO: Replace with actual icon!
			IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ProctoringNotification_Inactive.xaml") };
			Tooltip = text.Get(TextKey.Notification_ProctoringInactiveTooltip);
			NotificationChanged?.Invoke();
		}

		private void TerminateServiceSession()
		{
			if (service.IsConnected)
			{
				logger.Info("Terminating session...");
				service.TerminateSession();
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs args)
		{
			var trigger = new IntervalTrigger
			{
				ConfigurationValue = settings.MaxInterval,
				TimeElapsed = Convert.ToInt32(DateTime.Now.Subtract(last).TotalMilliseconds)
			};

			TryExecute(interval: trigger);
		}

		private void TryExecute(IntervalTrigger interval = default, KeyboardTrigger keyboard = default, MouseTrigger mouse = default)
		{
			if (MinimumIntervalElapsed() && Monitor.TryEnter(@lock))
			{
				last = DateTime.Now;
				timer.Stop();

				Task.Run(() =>
				{
					try
					{
						var metadata = new Metadata(applicationMonitor, browser, logger.CloneFor(nameof(Metadata)));

						using (var screenShot = new ScreenShot(logger.CloneFor(nameof(ScreenShot)), settings))
						{
							metadata.Capture(interval, keyboard, mouse);
							screenShot.Take();
							screenShot.Compress();

							if (service.IsConnected)
							{
								service.Send(metadata, screenShot);
							}
							else
							{
								logger.Warn("Cannot send screen shot as service is disconnected!");
							}
						}
					}
					catch (Exception e)
					{
						logger.Error("Failed to execute capturing and/or transmission!", e);
					}
				});

				timer.Start();
				Monitor.Exit(@lock);
			}
		}

		private bool MinimumIntervalElapsed()
		{
			return DateTime.Now.Subtract(last) >= new TimeSpan(0, 0, 0, 0, settings.MinInterval);
		}
	}
}
