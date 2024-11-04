/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client
{
	internal class ClientController
	{
		private readonly ClientContext context;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly ILogger logger;
		private readonly IMessageBox messageBox;
		private readonly IOperationSequence operations;
		private readonly IResponsibilityCollection<ClientTask> responsibilities;
		private readonly IRuntimeProxy runtime;
		private readonly ISplashScreen splashScreen;
		private readonly IText text;

		internal ClientController(
			ClientContext context,
			IFileSystemDialog fileSystemDialog,
			ILogger logger,
			IMessageBox messageBox,
			IOperationSequence operations,
			IResponsibilityCollection<ClientTask> responsibilities,
			IRuntimeProxy runtime,
			ISplashScreen splashScreen,
			IText text)
		{
			this.context = context;
			this.fileSystemDialog = fileSystemDialog;
			this.logger = logger;
			this.messageBox = messageBox;
			this.operations = operations;
			this.responsibilities = responsibilities;
			this.runtime = runtime;
			this.splashScreen = splashScreen;
			this.text = text;
		}

		internal bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			operations.ActionRequired += Operations_ActionRequired;
			operations.ProgressChanged += Operations_ProgressChanged;
			operations.StatusChanged += Operations_StatusChanged;

			splashScreen.Show();
			splashScreen.BringToForeground();

			var success = operations.TryPerform() == OperationResult.Success;

			if (success)
			{
				responsibilities.Delegate(ClientTask.RegisterEvents);
				responsibilities.Delegate(ClientTask.ShowShell);
				responsibilities.Delegate(ClientTask.AutoStartApplications);
				responsibilities.Delegate(ClientTask.ScheduleIntegrityVerification);
				responsibilities.Delegate(ClientTask.StartMonitoring);

				var communication = runtime.InformClientReady();

				if (communication.Success)
				{
					logger.Info("Application successfully initialized.");
					logger.Log(string.Empty);

					responsibilities.Delegate(ClientTask.VerifySessionIntegrity);
				}
				else
				{
					success = false;
					logger.Error("Failed to inform runtime that client is ready!");
				}
			}
			else
			{
				logger.Info("Application startup aborted!");
				logger.Log(string.Empty);
			}

			splashScreen.Hide();

			return success;
		}

		internal void Terminate()
		{
			logger.Log(string.Empty);
			logger.Info("Initiating shutdown procedure...");

			splashScreen.Show();
			splashScreen.BringToForeground();

			responsibilities.Delegate(ClientTask.CloseShell);
			responsibilities.Delegate(ClientTask.DeregisterEvents);
			responsibilities.Delegate(ClientTask.UpdateSessionIntegrity);

			var success = operations.TryRevert() == OperationResult.Success;

			if (success)
			{
				logger.Info("Application successfully finalized.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Shutdown procedure failed!");
				logger.Log(string.Empty);
			}

			splashScreen.Close();
		}

		internal void UpdateAppConfig()
		{
			splashScreen.AppConfig = context.AppConfig;
		}

		private void Operations_ActionRequired(ActionRequiredEventArgs args)
		{
			switch (args)
			{
				case ApplicationNotFoundEventArgs a:
					AskForApplicationPath(a);
					break;
				case ApplicationInitializationFailedEventArgs a:
					InformAboutFailedApplicationInitialization(a);
					break;
				case ApplicationTerminationEventArgs a:
					AskForAutomaticApplicationTermination(a);
					break;
				case ApplicationTerminationFailedEventArgs a:
					InformAboutFailedApplicationTermination(a);
					break;
			}
		}

		private void Operations_ProgressChanged(ProgressChangedEventArgs args)
		{
			if (args.CurrentValue.HasValue)
			{
				splashScreen.SetValue(args.CurrentValue.Value);
			}

			if (args.IsIndeterminate == true)
			{
				splashScreen.SetIndeterminate();
			}

			if (args.MaxValue.HasValue)
			{
				splashScreen.SetMaxValue(args.MaxValue.Value);
			}

			if (args.Progress == true)
			{
				splashScreen.Progress();
			}

			if (args.Regress == true)
			{
				splashScreen.Regress();
			}
		}

		private void Operations_StatusChanged(TextKey status)
		{
			splashScreen.UpdateStatus(status, true);
		}

		private void AskForAutomaticApplicationTermination(ApplicationTerminationEventArgs args)
		{
			var nl = Environment.NewLine;
			var applicationList = string.Join(Environment.NewLine, args.RunningApplications.Select(a => a.Name));
			var warning = text.Get(TextKey.MessageBox_ApplicationAutoTerminationDataLossWarning);
			var message = $"{text.Get(TextKey.MessageBox_ApplicationAutoTerminationQuestion)}{nl}{nl}{warning}{nl}{nl}{applicationList}";
			var title = text.Get(TextKey.MessageBox_ApplicationAutoTerminationQuestionTitle);
			var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, parent: splashScreen);

			args.TerminateProcesses = result == MessageBoxResult.Yes;
		}

		private void AskForApplicationPath(ApplicationNotFoundEventArgs args)
		{
			var message = text.Get(TextKey.FolderDialog_ApplicationLocation).Replace("%%NAME%%", args.DisplayName).Replace("%%EXECUTABLE%%", args.ExecutableName);
			var result = fileSystemDialog.Show(FileSystemElement.Folder, FileSystemOperation.Open, message: message, parent: splashScreen);

			if (result.Success)
			{
				args.CustomPath = result.FullPath;
				args.Success = true;
			}
		}

		private void InformAboutFailedApplicationInitialization(ApplicationInitializationFailedEventArgs args)
		{
			var messageKey = TextKey.MessageBox_ApplicationInitializationFailure;
			var titleKey = TextKey.MessageBox_ApplicationInitializationFailureTitle;

			if (args.Result == FactoryResult.NotFound)
			{
				messageKey = TextKey.MessageBox_ApplicationNotFound;
				titleKey = TextKey.MessageBox_ApplicationNotFoundTitle;
			}

			var message = text.Get(messageKey).Replace("%%NAME%%", $"'{args.DisplayName}' ({args.ExecutableName})");
			var title = text.Get(titleKey);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}

		private void InformAboutFailedApplicationTermination(ApplicationTerminationFailedEventArgs args)
		{
			var applicationList = string.Join(Environment.NewLine, args.Applications.Select(a => a.Name));
			var message = $"{text.Get(TextKey.MessageBox_ApplicationTerminationFailure)}{Environment.NewLine}{Environment.NewLine}{applicationList}";
			var title = text.Get(TextKey.MessageBox_ApplicationTerminationFailureTitle);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}
	}
}
