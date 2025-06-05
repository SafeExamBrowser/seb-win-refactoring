/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class SessionResponsibility : RuntimeResponsibility
	{
		private readonly AppConfig appConfig;
		private readonly IMessageBox messageBox;
		private readonly IRuntimeWindow runtimeWindow;
		private readonly IRepeatableOperationSequence sessionSequence;
		private readonly Action shutdown;
		private readonly IText text;

		internal SessionResponsibility(
			AppConfig appConfig,
			ILogger logger,
			IMessageBox messageBox,
			RuntimeContext runtimeContext,
			IRuntimeWindow runtimeWindow,
			IRepeatableOperationSequence sessionSequence,
			Action shutdown,
			IText text) : base(logger, runtimeContext)
		{
			this.appConfig = appConfig;
			this.messageBox = messageBox;
			this.runtimeWindow = runtimeWindow;
			this.sessionSequence = sessionSequence;
			this.shutdown = shutdown;
			this.text = text;
		}

		public override void Assume(RuntimeTask task)
		{
			switch (task)
			{
				case RuntimeTask.StartSession:
					StartSession();
					break;
				case RuntimeTask.StopSession:
					StopSession();
					break;
			}
		}

		private void StartSession()
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar = true;

			Logger.Info(AppendDivider("Session Start Procedure"));

			if (SessionIsRunning)
			{
				Context.Responsibilities.Delegate(RuntimeTask.DeregisterSessionEvents);
			}

			var result = SessionIsRunning ? sessionSequence.TryRepeat() : sessionSequence.TryPerform();

			if (result == OperationResult.Success)
			{
				Logger.Info(AppendDivider("Session Running"));

				HandleSessionStartSuccess();
			}
			else if (result == OperationResult.Failed)
			{
				Logger.Info(AppendDivider("Session Start Failed"));

				HandleSessionStartFailure();
			}
			else if (result == OperationResult.Aborted)
			{
				Logger.Info(AppendDivider("Session Start Aborted"));

				HandleSessionStartAbortion();
			}
		}

		private void StopSession()
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar = true;

			Logger.Info(AppendDivider("Session Stop Procedure"));

			Context.Responsibilities.Delegate(RuntimeTask.DeregisterSessionEvents);

			var success = sessionSequence.TryRevert() == OperationResult.Success;

			if (success)
			{
				Logger.Info(AppendDivider("Session Terminated"));
			}
			else
			{
				Logger.Info(AppendDivider("Session Stop Failed"));
			}
		}

		private void HandleSessionStartSuccess()
		{
			Context.Responsibilities.Delegate(RuntimeTask.RegisterSessionEvents);

			runtimeWindow.ShowProgressBar = false;
			runtimeWindow.ShowLog = Session.Settings.Security.AllowApplicationLogAccess;
			runtimeWindow.TopMost = Session.Settings.Security.KioskMode != KioskMode.None;
			runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_ApplicationRunning);

			if (Session.Settings.Security.KioskMode == KioskMode.DisableExplorerShell)
			{
				runtimeWindow.Hide();
			}
		}

		private void HandleSessionStartFailure()
		{
			var message = AppendLogFilePaths(appConfig, text.Get(TextKey.MessageBox_SessionStartError));
			var title = text.Get(TextKey.MessageBox_SessionStartErrorTitle);

			if (SessionIsRunning)
			{
				StopSession();

				messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: runtimeWindow);

				Logger.Info("Terminating application...");
				shutdown.Invoke();
			}
			else
			{
				messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: runtimeWindow);
			}
		}

		private void HandleSessionStartAbortion()
		{
			if (SessionIsRunning)
			{
				Context.Responsibilities.Delegate(RuntimeTask.RegisterSessionEvents);

				runtimeWindow.ShowProgressBar = false;
				runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_ApplicationRunning);
				runtimeWindow.TopMost = Session.Settings.Security.KioskMode != KioskMode.None;

				if (Session.Settings.Security.KioskMode == KioskMode.DisableExplorerShell)
				{
					runtimeWindow.Hide();
				}

				Context.ClientProxy.InformReconfigurationAborted();
			}
		}

		private string AppendDivider(string message)
		{
			var dashesLeft = new string('-', 48 - message.Length / 2 - message.Length % 2);
			var dashesRight = new string('-', 48 - message.Length / 2);

			return $"### {dashesLeft} {message} {dashesRight} ###";
		}
	}
}
