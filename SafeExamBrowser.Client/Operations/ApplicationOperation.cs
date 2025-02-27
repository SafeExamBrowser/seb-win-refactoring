/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Operations
{
	internal class ApplicationOperation : ClientOperation
	{
		private readonly IApplicationFactory factory;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly ILogger logger;
		private readonly IMessageBox messageBox;
		private readonly IApplicationMonitor monitor;
		private readonly ISplashScreen splashScreen;
		private readonly IText text;

		public override event StatusChangedEventHandler StatusChanged;

		public ApplicationOperation(
			ClientContext context,
			IApplicationFactory factory,
			IFileSystemDialog fileSystemDialog,
			ILogger logger,
			IMessageBox messageBox,
			IApplicationMonitor monitor,
			ISplashScreen splashScreen,
			IText text) : base(context)
		{
			this.factory = factory;
			this.fileSystemDialog = fileSystemDialog;
			this.logger = logger;
			this.messageBox = messageBox;
			this.monitor = monitor;
			this.splashScreen = splashScreen;
			this.text = text;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing applications...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeApplications);

			var result = InitializeApplications();

			if (result == OperationResult.Success)
			{
				StartMonitor();
			}

			return result;
		}

		public override OperationResult Revert()
		{
			logger.Info("Finalizing applications...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeApplications);

			FinalizeApplications();
			StopMonitor();

			return OperationResult.Success;
		}

		private OperationResult InitializeApplications()
		{
			var initialization = monitor.Initialize(Context.Settings.Applications);
			var result = OperationResult.Success;

			if (initialization.FailedAutoTerminations.Any())
			{
				result = HandleAutoTerminationFailure(initialization.FailedAutoTerminations);
			}
			else if (initialization.RunningApplications.Any())
			{
				result = TryTerminate(initialization.RunningApplications);
			}

			if (result == OperationResult.Success)
			{
				foreach (var application in Context.Settings.Applications.Whitelist)
				{
					Initialize(application);
				}
			}

			return result;
		}

		private void Initialize(WhitelistApplication settings)
		{
			var result = factory.TryCreate(settings, out var application);

			while (result == FactoryResult.NotFound && settings.AllowCustomPath)
			{
				if (TryAskForApplicationPath(settings.DisplayName, settings.ExecutableName, out var customPath))
				{
					settings.ExecutablePath = customPath;
					result = factory.TryCreate(settings, out application);
				}
				else
				{
					break;
				}
			}

			if (result == FactoryResult.Success)
			{
				Context.Applications.Add(application);
			}
			else
			{
				logger.Error($"Failed to initialize application '{settings.DisplayName}' ({settings.ExecutableName}). Reason: {result}.");
				InformAboutFailedApplicationInitialization(settings.DisplayName, settings.ExecutableName, result);
			}
		}

		private void FinalizeApplications()
		{
			foreach (var application in Context.Applications)
			{
				application.Terminate();
			}
		}

		private OperationResult HandleAutoTerminationFailure(IList<RunningApplication> applications)
		{
			logger.Error($"{applications.Count} application(s) could not be automatically terminated: {string.Join(", ", applications.Select(a => a.Name))}");
			InformAboutFailedApplicationTermination(applications);

			return OperationResult.Failed;
		}

		private void StartMonitor()
		{
			if (Context.Settings.Security.KioskMode != KioskMode.None)
			{
				monitor.Start();
			}
		}

		private void StopMonitor()
		{
			if (Context.Settings.Security.KioskMode != KioskMode.None)
			{
				monitor.Stop();
			}
		}

		private OperationResult TryTerminate(IEnumerable<RunningApplication> applications)
		{
			var failed = new List<RunningApplication>();
			var result = OperationResult.Success;

			logger.Info($"The following applications need to be terminated: {string.Join(", ", applications.Select(a => a.Name))}.");

			if (TryAskForAutomaticApplicationTermination(applications))
			{
				logger.Info($"The user chose to automatically terminate all running applications.");

				foreach (var application in applications)
				{
					var success = monitor.TryTerminate(application);

					if (success)
					{
						logger.Info($"Successfully terminated application '{application.Name}'.");
					}
					else
					{
						result = OperationResult.Failed;
						failed.Add(application);
						logger.Error($"Failed to automatically terminate application '{application.Name}'!");
					}
				}
			}
			else
			{
				logger.Info("The user chose not to automatically terminate all running applications. Aborting...");
				result = OperationResult.Aborted;
			}

			if (failed.Any())
			{
				InformAboutFailedApplicationTermination(failed);
			}

			return result;
		}

		private void InformAboutFailedApplicationInitialization(string displayName, string executableName, FactoryResult result)
		{
			var messageKey = TextKey.MessageBox_ApplicationInitializationFailure;
			var titleKey = TextKey.MessageBox_ApplicationInitializationFailureTitle;

			if (result == FactoryResult.NotFound)
			{
				messageKey = TextKey.MessageBox_ApplicationNotFound;
				titleKey = TextKey.MessageBox_ApplicationNotFoundTitle;
			}

			var message = text.Get(messageKey).Replace("%%NAME%%", $"'{displayName}' ({executableName})");
			var title = text.Get(titleKey);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}

		private void InformAboutFailedApplicationTermination(IEnumerable<RunningApplication> applications)
		{
			var applicationList = string.Join(Environment.NewLine, applications.Select(a => a.Name));
			var message = $"{text.Get(TextKey.MessageBox_ApplicationTerminationFailure)}{Environment.NewLine}{Environment.NewLine}{applicationList}";
			var title = text.Get(TextKey.MessageBox_ApplicationTerminationFailureTitle);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}

		private bool TryAskForApplicationPath(string displayName, string executableName, out string customPath)
		{
			var message = text.Get(TextKey.FolderDialog_ApplicationLocation).Replace("%%NAME%%", displayName).Replace("%%EXECUTABLE%%", executableName);
			var result = fileSystemDialog.Show(FileSystemElement.Folder, FileSystemOperation.Open, message: message, parent: splashScreen);

			customPath = result.FullPath;

			return result.Success;
		}

		private bool TryAskForAutomaticApplicationTermination(IEnumerable<RunningApplication> applications)
		{
			var nl = Environment.NewLine;
			var applicationList = string.Join(Environment.NewLine, applications.Select(a => a.Name));
			var warning = text.Get(TextKey.MessageBox_ApplicationAutoTerminationDataLossWarning);
			var message = $"{text.Get(TextKey.MessageBox_ApplicationAutoTerminationQuestion)}{nl}{nl}{warning}{nl}{nl}{applicationList}";
			var title = text.Get(TextKey.MessageBox_ApplicationAutoTerminationQuestionTitle);
			var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, parent: splashScreen);

			return result == MessageBoxResult.Yes;
		}
	}
}
