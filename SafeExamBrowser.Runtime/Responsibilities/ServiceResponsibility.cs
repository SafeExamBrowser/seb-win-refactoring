/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class ServiceResponsibility : RuntimeResponsibility
	{
		private readonly IMessageBox messageBox;
		private readonly IRuntimeWindow runtimeWindow;
		private readonly IServiceProxy serviceProxy;
		private readonly Action shutdown;

		internal ServiceResponsibility(
			ILogger logger,
			IMessageBox messageBox,
			RuntimeContext runtimeContext,
			IRuntimeWindow runtimeWindow,
			IServiceProxy serviceProxy,
			Action shutdown) : base(logger, runtimeContext)
		{
			this.messageBox = messageBox;
			this.runtimeWindow = runtimeWindow;
			this.serviceProxy = serviceProxy;
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
			serviceProxy.ConnectionLost -= ServiceProxy_ConnectionLost;
		}

		private void RegisterEvents()
		{
			serviceProxy.ConnectionLost += ServiceProxy_ConnectionLost;
		}

		private void ServiceProxy_ConnectionLost()
		{
			if (SessionIsRunning && Session.Settings.Service.Policy == ServicePolicy.Mandatory)
			{
				Logger.Error("Lost connection to the service component!");

				Context.Responsibilities.Delegate(RuntimeTask.StopSession);

				messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
				shutdown.Invoke();
			}
			else
			{
				Logger.Warn("Lost connection to the service component!");
			}
		}
	}
}
