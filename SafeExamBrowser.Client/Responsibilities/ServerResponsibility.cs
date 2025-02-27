/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class ServerResponsibility : ClientResponsibility
	{
		private readonly ICoordinator coordinator;
		private readonly IText text;

		private IServerProxy Server => Context.Server;

		public ServerResponsibility(ClientContext context, ICoordinator coordinator, ILogger logger, IText text) : base(context, logger)
		{
			this.coordinator = coordinator;
			this.text = text;
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.DeregisterEvents:
					DeregisterEvents();
					break;
				case ClientTask.RegisterEvents:
					RegisterEvents();
					break;
			}
		}

		private void DeregisterEvents()
		{
			if (Server != default)
			{
				Server.LockScreenConfirmed -= Server_LockScreenConfirmed;
				Server.LockScreenRequested -= Server_LockScreenRequested;
				Server.TerminationRequested -= Server_TerminationRequested;
			}
		}

		private void RegisterEvents()
		{
			Server.LockScreenConfirmed += Server_LockScreenConfirmed;
			Server.LockScreenRequested += Server_LockScreenRequested;
			Server.TerminationRequested += Server_TerminationRequested;
		}

		private void Server_LockScreenConfirmed()
		{
			Logger.Info("Closing lock screen as requested by the server...");
			Context.LockScreen?.Cancel();
		}

		private void Server_LockScreenRequested(string message)
		{
			Logger.Info("Attempting to show lock screen as requested by the server...");

			if (coordinator.RequestSessionLock())
			{
				ShowLockScreen(message, text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
				coordinator.ReleaseSessionLock();
			}
			else
			{
				Logger.Info("Lock screen is already active.");
			}
		}

		private void Server_TerminationRequested()
		{
			Logger.Info("Attempting to shutdown as requested by the server...");
			TryRequestShutdown();
		}
	}
}
