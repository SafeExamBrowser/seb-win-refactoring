/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Events;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server
{
	public class Invigilator : IInvigilator
	{
		private readonly ILogger logger;
		private readonly IServerProxy server;

		private InvigilationSettings settings;

		public bool IsHandRaised { get; private set; }

		public event InvigilationEventHandler HandLowered;
		public event InvigilationEventHandler HandRaised;

		public Invigilator(ILogger logger, IServerProxy server)
		{
			this.logger = logger;
			this.server = server;
		}

		public void Initialize(InvigilationSettings settings)
		{
			var raiseHand = $"Raise hand {(settings.ShowRaiseHandNotification ? "enabled" : "disabled")}";
			var message = $"message {(settings.ForceRaiseHandMessage ? "required" : "disabled")}";

			this.settings = settings;
			server.HandConfirmed += Server_HandConfirmed;

			logger.Info($"Initialized functionality: {raiseHand}, {message}.");
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

		private void Server_HandConfirmed()
		{
			logger.Info("Hand confirmation received.");

			IsHandRaised = false;
			HandLowered?.Invoke();
		}
	}
}
