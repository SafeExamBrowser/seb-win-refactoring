/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class CommunicationResponsibility : ClientResponsibility
	{
		private readonly ICoordinator coordinator;
		private readonly IMessageBox messageBox;
		private readonly IRuntimeProxy runtime;
		private readonly Action shutdown;
		private readonly ISplashScreen splashScreen;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private IClientHost ClientHost => Context.ClientHost;

		public CommunicationResponsibility(
			ClientContext context,
			ICoordinator coordinator,
			ILogger logger,
			IMessageBox messageBox,
			IRuntimeProxy runtime,
			Action shutdown,
			ISplashScreen splashScreen,
			IText text,
			IUserInterfaceFactory uiFactory) : base(context, logger)
		{
			this.coordinator = coordinator;
			this.messageBox = messageBox;
			this.runtime = runtime;
			this.shutdown = shutdown;
			this.splashScreen = splashScreen;
			this.text = text;
			this.uiFactory = uiFactory;
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
			if (ClientHost != default)
			{
				ClientHost.ExamSelectionRequested -= ClientHost_ExamSelectionRequested;
				ClientHost.MessageBoxRequested -= ClientHost_MessageBoxRequested;
				ClientHost.PasswordRequested -= ClientHost_PasswordRequested;
				ClientHost.ReconfigurationAborted -= ClientHost_ReconfigurationAborted;
				ClientHost.ReconfigurationDenied -= ClientHost_ReconfigurationDenied;
				ClientHost.ServerFailureActionRequested -= ClientHost_ServerFailureActionRequested;
				ClientHost.Shutdown -= ClientHost_Shutdown;
			}

			runtime.ConnectionLost -= Runtime_ConnectionLost;
		}

		private void RegisterEvents()
		{
			ClientHost.ExamSelectionRequested += ClientHost_ExamSelectionRequested;
			ClientHost.MessageBoxRequested += ClientHost_MessageBoxRequested;
			ClientHost.PasswordRequested += ClientHost_PasswordRequested;
			ClientHost.ReconfigurationAborted += ClientHost_ReconfigurationAborted;
			ClientHost.ReconfigurationDenied += ClientHost_ReconfigurationDenied;
			ClientHost.ServerFailureActionRequested += ClientHost_ServerFailureActionRequested;
			ClientHost.Shutdown += ClientHost_Shutdown;

			runtime.ConnectionLost += Runtime_ConnectionLost;
		}

		private void ClientHost_ExamSelectionRequested(ExamSelectionRequestEventArgs args)
		{
			Logger.Info($"Received exam selection request with id '{args.RequestId}'.");

			var exams = args.Exams.Select(e => new Exam { Id = e.id, LmsName = e.lms, Name = e.name, Url = e.url });
			var dialog = uiFactory.CreateExamSelectionDialog(exams);
			var result = dialog.Show();

			runtime.SubmitExamSelectionResult(args.RequestId, result.Success, result.SelectedExam?.Id);
			Logger.Info($"Exam selection request with id '{args.RequestId}' is complete.");
		}

		private void ClientHost_MessageBoxRequested(MessageBoxRequestEventArgs args)
		{
			Logger.Info($"Received message box request with id '{args.RequestId}'.");

			var action = (MessageBoxAction) args.Action;
			var icon = (MessageBoxIcon) args.Icon;
			var result = messageBox.Show(args.Message, args.Title, action, icon, parent: splashScreen);

			runtime.SubmitMessageBoxResult(args.RequestId, (int) result);
			Logger.Info($"Message box request with id '{args.RequestId}' yielded result '{result}'.");
		}

		private void ClientHost_PasswordRequested(PasswordRequestEventArgs args)
		{
			var message = default(TextKey);
			var title = default(TextKey);

			Logger.Info($"Received input request with id '{args.RequestId}' for the {args.Purpose.ToString().ToLower()} password.");

			switch (args.Purpose)
			{
				case PasswordRequestPurpose.LocalAdministrator:
					message = TextKey.PasswordDialog_LocalAdminPasswordRequired;
					title = TextKey.PasswordDialog_LocalAdminPasswordRequiredTitle;
					break;
				case PasswordRequestPurpose.LocalSettings:
					message = TextKey.PasswordDialog_LocalSettingsPasswordRequired;
					title = TextKey.PasswordDialog_LocalSettingsPasswordRequiredTitle;
					break;
				case PasswordRequestPurpose.Settings:
					message = TextKey.PasswordDialog_SettingsPasswordRequired;
					title = TextKey.PasswordDialog_SettingsPasswordRequiredTitle;
					break;
			}

			var dialog = uiFactory.CreatePasswordDialog(text.Get(message), text.Get(title));
			var result = dialog.Show();

			runtime.SubmitPassword(args.RequestId, result.Success, result.Password);
			Logger.Info($"Password request with id '{args.RequestId}' was {(result.Success ? "successful" : "aborted by the user")}.");
		}

		private void ClientHost_ReconfigurationAborted()
		{
			Logger.Info("The reconfiguration was aborted by the runtime.");
			splashScreen.Hide();
			coordinator.ReleaseReconfigurationLock();
		}

		private void ClientHost_ReconfigurationDenied(ReconfigurationEventArgs args)
		{
			Logger.Info($"The reconfiguration request for '{args.ConfigurationPath}' was denied by the runtime!");
			messageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle, parent: splashScreen);
			splashScreen.Hide();
			coordinator.ReleaseReconfigurationLock();
		}

		private void ClientHost_ServerFailureActionRequested(ServerFailureActionRequestEventArgs args)
		{
			Logger.Info($"Received server failure action request with id '{args.RequestId}'.");

			var dialog = uiFactory.CreateServerFailureDialog(args.Message, args.ShowFallback);
			var result = dialog.Show();

			runtime.SubmitServerFailureActionResult(args.RequestId, result.Abort, result.Fallback, result.Retry);
			Logger.Info($"Server failure action request with id '{args.RequestId}' is complete, the user chose to {(result.Abort ? "abort" : (result.Fallback ? "fallback" : "retry"))}.");
		}

		private void ClientHost_Shutdown()
		{
			shutdown.Invoke();
		}

		private void Runtime_ConnectionLost()
		{
			Logger.Error("Lost connection to the runtime!");

			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error);
			shutdown.Invoke();
		}
	}
}
