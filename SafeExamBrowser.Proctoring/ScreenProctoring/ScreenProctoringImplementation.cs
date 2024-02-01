/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Timers;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service;
using SafeExamBrowser.Server.Contracts.Events.Proctoring;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class ScreenProctoringImplementation : ProctoringImplementation
	{
		private readonly IModuleLogger logger;
		private readonly ServiceProxy service;
		private readonly ScreenProctoringSettings settings;
		private readonly IText text;
		private readonly Timer timer;

		internal override string Name => nameof(ScreenProctoring);

		public override event NotificationChangedEventHandler NotificationChanged;

		internal ScreenProctoringImplementation(IModuleLogger logger, ServiceProxy service, ProctoringSettings settings, IText text)
		{
			this.logger = logger;
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
			timer.Elapsed += Timer_Elapsed;
			timer.Start();

			ShowNotificationActive();

			logger.Info($"Started proctoring.");
		}

		internal override void Stop()
		{
			timer.Elapsed -= Timer_Elapsed;
			timer.Stop();

			TerminateServiceSession();
			ShowNotificationInactive();

			logger.Info("Stopped proctoring.");
		}

		internal override void Terminate()
		{
			if (timer.Enabled)
			{
				Stop();
			}

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

		private void ShowNotificationActive()
		{
			// TODO: Replace with actual icon!
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
			try
			{
				using (var screenShot = new ScreenShot(logger.CloneFor(nameof(ScreenShot)), settings))
				{
					screenShot.Take();
					screenShot.Compress();
					service.SendScreenShot(screenShot);
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to process screen shot!", e);
			}

			timer.Start();
		}
	}
}
