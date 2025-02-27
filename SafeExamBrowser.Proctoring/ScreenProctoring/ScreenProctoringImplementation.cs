/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Configuration.Contracts;
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

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class ScreenProctoringImplementation : ProctoringImplementation
	{
		private readonly DataCollector collector;
		private readonly IModuleLogger logger;
		private readonly ServiceProxy service;
		private readonly ScreenProctoringSettings settings;
		private readonly TransmissionSpooler spooler;
		private readonly IText text;

		internal override string Name => nameof(ScreenProctoring);

		internal ScreenProctoringImplementation(
			AppConfig appConfig,
			IApplicationMonitor applicationMonitor,
			IBrowserApplication browser,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			ServiceProxy service,
			ProctoringSettings settings,
			IText text)
		{
			this.collector = new DataCollector(applicationMonitor, browser, logger.CloneFor(nameof(DataCollector)), nativeMethods, settings.ScreenProctoring);
			this.logger = logger;
			this.service = service;
			this.settings = settings.ScreenProctoring;
			this.spooler = new TransmissionSpooler(appConfig, logger.CloneFor(nameof(TransmissionSpooler)), service, settings.ScreenProctoring);
			this.text = text;
		}

		internal override void ExecuteRemainingWork()
		{
			logger.Info("Starting execution of remaining work...");
			spooler.ExecuteRemainingWork(InvokeRemainingWorkUpdated);
			logger.Info("Terminated execution of remaining work.");
		}

		internal override bool HasRemainingWork()
		{
			var hasWork = spooler.HasRemainingWork();

			if (hasWork)
			{
				logger.Info("There is remaining work to be done.");
			}
			else
			{
				logger.Info("There is no remaining work to be done.");
			}

			return hasWork;
		}

		internal override void Initialize()
		{
			var start = true;

			start &= !string.IsNullOrWhiteSpace(settings.ClientId);
			start &= !string.IsNullOrWhiteSpace(settings.ClientSecret);
			start &= !string.IsNullOrWhiteSpace(settings.GroupId);
			start &= !string.IsNullOrWhiteSpace(settings.ServiceUrl);

			if (start)
			{
				logger.Info($"Initialized proctoring: All settings are valid, starting automatically...");

				Connect();
				Start();
			}
			else
			{
				UpdateNotification(false);
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
					settings.EncryptionSecret = instruction.EncryptionSecret;
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
			collector.DataCollected += Collector_DataCollected;
			collector.Start();
			spooler.Start();

			UpdateNotification(true);

			logger.Info($"Started proctoring.");
		}

		internal override void Stop()
		{
			collector.Stop();
			collector.DataCollected -= Collector_DataCollected;
			spooler.Stop();

			TerminateSession();
			UpdateNotification(false);

			logger.Info("Stopped proctoring.");
		}

		internal override void Terminate()
		{
			Stop();

			logger.Info("Terminated proctoring.");
		}

		private void Collector_DataCollected(MetaData metaData, ScreenShot screenShot)
		{
			spooler.Add(metaData, screenShot);
		}

		private void Connect(string sessionId = default)
		{
			logger.Info("Connecting to service...");

			var connect = service.Connect(settings.ClientId, settings.ClientSecret, settings.ServiceUrl);

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

		private void TerminateSession()
		{
			if (service.IsConnected)
			{
				logger.Info("Terminating session...");
				service.TerminateSession();
			}
		}

		private void UpdateNotification(bool active)
		{
			CanActivate = false;

			if (active)
			{
				IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ScreenProctoring_Active.xaml") };
				Tooltip = text.Get(TextKey.Notification_ProctoringActiveTooltip);
			}
			else
			{
				IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ScreenProctoring_Inactive.xaml") };
				Tooltip = text.Get(TextKey.Notification_ProctoringInactiveTooltip);
			}

			InvokeNotificationChanged();
		}
	}
}
