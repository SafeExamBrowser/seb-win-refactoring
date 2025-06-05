/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class ClientResponsibility : RuntimeResponsibility
	{
		private readonly IMessageBox messageBox;
		private readonly IRuntimeWindow runtimeWindow;
		private readonly Action shutdown;

		internal ClientResponsibility(
			ILogger logger,
			IMessageBox messageBox,
			RuntimeContext runtimeContext,
			IRuntimeWindow runtimeWindow,
			Action shutdown) : base(logger, runtimeContext)
		{
			this.messageBox = messageBox;
			this.runtimeWindow = runtimeWindow;
			this.shutdown = shutdown;
		}

		public override void Assume(RuntimeTask task)
		{
			switch (task)
			{
				case RuntimeTask.DeregisterSessionEvents:
					DeregisterEvents();
					break;
				case RuntimeTask.RegisterSessionEvents:
					RegisterEvents();
					break;
			}
		}

		private void DeregisterEvents()
		{
			if (Context.ClientProcess != default)
			{
				Context.ClientProcess.Terminated -= ClientProcess_Terminated;
			}

			if (Context.ClientProxy != default)
			{
				Context.ClientProxy.ConnectionLost -= ClientProxy_ConnectionLost;
			}
		}

		private void RegisterEvents()
		{
			Context.ClientProcess.Terminated += ClientProcess_Terminated;
			Context.ClientProxy.ConnectionLost += ClientProxy_ConnectionLost;
		}

		private void ClientProcess_Terminated(int exitCode)
		{
			Logger.Error($"Client application has unexpectedly terminated with exit code {exitCode}!");

			if (SessionIsRunning)
			{
				Context.Responsibilities.Delegate(RuntimeTask.StopSession);
			}

			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
			shutdown.Invoke();
		}

		private void ClientProxy_ConnectionLost()
		{
			Logger.Error("Lost connection to the client application!");

			if (SessionIsRunning)
			{
				Context.Responsibilities.Delegate(RuntimeTask.StopSession);
			}

			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
			shutdown.Invoke();
		}
	}
}
