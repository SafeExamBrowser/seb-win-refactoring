/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using KGySoft.CoreLibraries;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Events.Proctoring;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Proctoring
{
	public class ProctoringController : IProctoringController
	{
		private readonly ProctoringFactory factory;
		private readonly IModuleLogger logger;
		private readonly IServerProxy server;

		private IEnumerable<ProctoringImplementation> implementations;

		public bool IsHandRaised { get; private set; }
		public IEnumerable<INotification> Notifications => new List<INotification>(implementations);

		public event ProctoringEventHandler HandLowered;
		public event ProctoringEventHandler HandRaised;
		public event RemainingWorkUpdatedEventHandler RemainingWorkUpdated
		{
			add { implementations.ForEach(i => i.RemainingWorkUpdated += value); }
			remove { implementations.ForEach(i => i.RemainingWorkUpdated -= value); }
		}

		public ProctoringController(
			AppConfig appConfig,
			IApplicationMonitor applicationMonitor,
			IBrowserApplication browser,
			IFileSystem fileSystem,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			IServerProxy server,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.server = server;

			factory = new ProctoringFactory(appConfig, applicationMonitor, browser, fileSystem, logger, nativeMethods, text, uiFactory);
			implementations = new List<ProctoringImplementation>();
		}

		public void ExecuteRemainingWork()
		{
			foreach (var implementation in implementations)
			{
				try
				{
					implementation.ExecuteRemainingWork();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to execute remaining work for '{implementation.Name}'!", e);
				}
			}
		}

		public bool HasRemainingWork()
		{
			var hasWork = false;

			foreach (var implementation in implementations)
			{
				try
				{
					if (implementation.HasRemainingWork())
					{
						hasWork = true;
					}
				}
				catch (Exception e)
				{
					logger.Error($"Failed to check whether has remaining work for '{implementation.Name}'!", e);
				}
			}

			return hasWork;
		}

		public void Initialize(ProctoringSettings settings)
		{
			implementations = factory.CreateAllActive(settings);

			server.HandConfirmed += Server_HandConfirmed;
			server.ProctoringConfigurationReceived += Server_ProctoringConfigurationReceived;
			server.ProctoringInstructionReceived += Server_ProctoringInstructionReceived;

			foreach (var implementation in implementations)
			{
				try
				{
					implementation.Initialize();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to initialize proctoring implementation '{implementation.Name}'!", e);
				}
			}
		}

		public void LowerHand()
		{
			var response = server.LowerHand();

			if (response.Success)
			{
				IsHandRaised = false;
				HandLowered?.Invoke();

				logger.Info("Hand lowered.");
			}
			else
			{
				logger.Error($"Failed to send lower hand notification to server! Message: {response.Message}.");
			}
		}

		public void RaiseHand(string message = null)
		{
			var response = server.RaiseHand(message);

			if (response.Success)
			{
				IsHandRaised = true;
				HandRaised?.Invoke();

				logger.Info("Hand raised.");
			}
			else
			{
				logger.Error($"Failed to send raise hand notification to server! Message: {response.Message}.");
			}
		}

		public void Terminate()
		{
			foreach (var implementation in implementations)
			{
				try
				{
					implementation.Terminate();
				}
				catch (Exception e)
				{
					logger.Error($"Failed to terminate proctoring implementation '{implementation.Name}'!", e);
				}
			}
		}

		private void Server_HandConfirmed()
		{
			logger.Info("Hand confirmation received.");

			IsHandRaised = false;
			HandLowered?.Invoke();
		}

		private void Server_ProctoringConfigurationReceived(bool allowChat, bool receiveAudio, bool receiveVideo)
		{
			foreach (var implementation in implementations)
			{
				try
				{
					implementation.ProctoringConfigurationReceived(allowChat, receiveAudio, receiveVideo);
				}
				catch (Exception e)
				{
					logger.Error($"Failed to update proctoring configuration for '{implementation.Name}'!", e);
				}
			}
		}

		private void Server_ProctoringInstructionReceived(InstructionEventArgs args)
		{
			foreach (var implementation in implementations)
			{
				try
				{
					implementation.ProctoringInstructionReceived(args);
				}
				catch (Exception e)
				{
					logger.Error($"Failed to process proctoring instruction for '{implementation.Name}'!", e);
				}
			}
		}
	}
}
