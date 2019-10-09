/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client
{
	internal class ClientController : IClientController
	{
		private IActionCenter actionCenter;
		private IApplicationMonitor applicationMonitor;
		private ClientContext context;
		private IDisplayMonitor displayMonitor;
		private IExplorerShell explorerShell;
		private IHashAlgorithm hashAlgorithm;
		private ILogger logger;
		private IMessageBox messageBox;
		private IOperationSequence operations;
		private IRuntimeProxy runtime;
		private Action shutdown;
		private ISplashScreen splashScreen;
		private ITaskbar taskbar;
		private ITerminationActivator terminationActivator;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		private IBrowserApplication Browser => context.Browser;
		private IClientHost ClientHost => context.ClientHost;
		private AppSettings Settings => context.Settings;

		public ClientController(
			IActionCenter actionCenter,
			IApplicationMonitor applicationMonitor,
			ClientContext context,
			IDisplayMonitor displayMonitor,
			IExplorerShell explorerShell,
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			IMessageBox messageBox,
			IOperationSequence operations,
			IRuntimeProxy runtime,
			Action shutdown,
			ITaskbar taskbar,
			ITerminationActivator terminationActivator,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.actionCenter = actionCenter;
			this.applicationMonitor = applicationMonitor;
			this.context = context;
			this.displayMonitor = displayMonitor;
			this.explorerShell = explorerShell;
			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
			this.messageBox = messageBox;
			this.operations = operations;
			this.runtime = runtime;
			this.shutdown = shutdown;
			this.taskbar = taskbar;
			this.terminationActivator = terminationActivator;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			splashScreen = uiFactory.CreateSplashScreen();
			operations.ActionRequired += Operations_ActionRequired;
			operations.ProgressChanged += Operations_ProgressChanged;
			operations.StatusChanged += Operations_StatusChanged;

			var success = operations.TryPerform() == OperationResult.Success;

			if (success)
			{
				RegisterEvents();
				ShowShell();
				StartBrowser();

				var communication = runtime.InformClientReady();

				if (communication.Success)
				{
					logger.Info("Application successfully initialized.");
					logger.Log(string.Empty);
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

			splashScreen.Close();

			return success;
		}

		public void Terminate()
		{
			logger.Log(string.Empty);
			logger.Info("Initiating shutdown procedure...");

			splashScreen = uiFactory.CreateSplashScreen(context.AppConfig);
			actionCenter.Close();
			taskbar.Close();

			DeregisterEvents();

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

		public void UpdateAppConfig()
		{
			if (splashScreen != null)
			{
				splashScreen.AppConfig = context.AppConfig;
			}
		}

		private void RegisterEvents()
		{
			actionCenter.QuitButtonClicked += Shell_QuitButtonClicked;
			applicationMonitor.ExplorerStarted += ApplicationMonitor_ExplorerStarted;
			applicationMonitor.TerminationFailed += ApplicationMonitor_TerminationFailed;
			Browser.ConfigurationDownloadRequested += Browser_ConfigurationDownloadRequested;
			ClientHost.MessageBoxRequested += ClientHost_MessageBoxRequested;
			ClientHost.PasswordRequested += ClientHost_PasswordRequested;
			ClientHost.ReconfigurationDenied += ClientHost_ReconfigurationDenied;
			ClientHost.Shutdown += ClientHost_Shutdown;
			displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
			runtime.ConnectionLost += Runtime_ConnectionLost;
			taskbar.QuitButtonClicked += Shell_QuitButtonClicked;
			terminationActivator.Activated += TerminationActivator_Activated;
		}

		private void DeregisterEvents()
		{
			actionCenter.QuitButtonClicked -= Shell_QuitButtonClicked;
			applicationMonitor.ExplorerStarted -= ApplicationMonitor_ExplorerStarted;
			applicationMonitor.TerminationFailed -= ApplicationMonitor_TerminationFailed;
			displayMonitor.DisplayChanged -= DisplayMonitor_DisplaySettingsChanged;
			runtime.ConnectionLost -= Runtime_ConnectionLost;
			taskbar.QuitButtonClicked -= Shell_QuitButtonClicked;
			terminationActivator.Activated -= TerminationActivator_Activated;

			if (Browser != null)
			{
				Browser.ConfigurationDownloadRequested -= Browser_ConfigurationDownloadRequested;
			}

			if (ClientHost != null)
			{
				ClientHost.MessageBoxRequested -= ClientHost_MessageBoxRequested;
				ClientHost.PasswordRequested -= ClientHost_PasswordRequested;
				ClientHost.ReconfigurationDenied -= ClientHost_ReconfigurationDenied;
				ClientHost.Shutdown -= ClientHost_Shutdown;
			}
		}

		private void ShowShell()
		{
			if (Settings.Taskbar.EnableTaskbar)
			{
				taskbar.Show();
			}
		}

		private void StartBrowser()
		{
			logger.Info("Starting browser application...");
			Browser.Start();
		}

		private void ApplicationMonitor_ExplorerStarted()
		{
			logger.Info("Trying to terminate Windows explorer...");
			explorerShell.Terminate();
			logger.Info("Reinitializing working area...");
			displayMonitor.InitializePrimaryDisplay(taskbar.GetAbsoluteHeight());
			logger.Info("Reinitializing shell...");
			actionCenter.InitializeBounds();
			taskbar.InitializeBounds();
			logger.Info("Desktop successfully restored.");
		}

		private void ApplicationMonitor_TerminationFailed(IEnumerable<RunningApplication> applications)
		{
			// foreach actionCenterActivator -> Pause
			// TODO: Show lock screen!
			// foreach actionCenterActivator -> Resume
		}

		private void Browser_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			if (Settings.ConfigurationMode == ConfigurationMode.ConfigureClient)
			{
				args.AllowDownload = true;
				args.Callback = Browser_ConfigurationDownloadFinished;
				args.DownloadPath = Path.Combine(context.AppConfig.DownloadDirectory, fileName);
				logger.Info($"Allowed download request for configuration file '{fileName}'.");
			}
			else
			{
				args.AllowDownload = false;
				logger.Info($"Denied download request for configuration file '{fileName}' due to '{Settings.ConfigurationMode}' mode.");
			}
		}

		private void Browser_ConfigurationDownloadFinished(bool success, string filePath = null)
		{
			if (success)
			{
				var communication = runtime.RequestReconfiguration(filePath);

				if (communication.Success)
				{
					logger.Info($"Sent reconfiguration request for '{filePath}' to the runtime.");

					splashScreen = uiFactory.CreateSplashScreen(context.AppConfig);
					splashScreen.SetIndeterminate();
					splashScreen.UpdateStatus(TextKey.OperationStatus_InitializeSession, true);
					splashScreen.Show();
				}
				else
				{
					logger.Error($"Failed to communicate reconfiguration request for '{filePath}'!");
					messageBox.Show(TextKey.MessageBox_ReconfigurationError, TextKey.MessageBox_ReconfigurationErrorTitle, icon: MessageBoxIcon.Error);
				}
			}
			else
			{
				logger.Error($"Failed to download configuration file '{filePath}'!");
				messageBox.Show(TextKey.MessageBox_ConfigurationDownloadError, TextKey.MessageBox_ConfigurationDownloadErrorTitle, icon: MessageBoxIcon.Error);
			}
		}

		private void ClientHost_MessageBoxRequested(MessageBoxRequestEventArgs args)
		{
			logger.Info($"Received message box request with id '{args.RequestId}'.");

			var action = (MessageBoxAction) args.Action;
			var icon = (MessageBoxIcon) args.Icon;
			var result = messageBox.Show(args.Message, args.Title, action, icon, parent: splashScreen);

			runtime.SubmitMessageBoxResult(args.RequestId, (int) result);
			logger.Info($"Message box request with id '{args.RequestId}' yielded result '{result}'.");
		}

		private void ClientHost_PasswordRequested(PasswordRequestEventArgs args)
		{
			var message = default(TextKey);
			var title = default(TextKey);

			logger.Info($"Received input request with id '{args.RequestId}' for the {args.Purpose.ToString().ToLower()} password.");

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
			logger.Info($"Password request with id '{args.RequestId}' was {(result.Success ? "successful" : "aborted by the user")}.");

			if (!result.Success)
			{
				splashScreen?.Close();
			}
		}

		private void ClientHost_ReconfigurationDenied(ReconfigurationEventArgs args)
		{
			logger.Info($"The reconfiguration request for '{args.ConfigurationPath}' was denied by the runtime!");
			messageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle, parent: splashScreen);
			splashScreen?.Close();
		}

		private void ClientHost_Shutdown()
		{
			shutdown.Invoke();
		}

		private void DisplayMonitor_DisplaySettingsChanged()
		{
			logger.Info("Reinitializing working area...");
			displayMonitor.InitializePrimaryDisplay(taskbar.GetAbsoluteHeight());
			logger.Info("Reinitializing shell...");
			actionCenter.InitializeBounds();
			taskbar.InitializeBounds();
			logger.Info("Desktop successfully restored.");
		}

		private void Operations_ActionRequired(ActionRequiredEventArgs args)
		{
			switch (args)
			{
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
				splashScreen?.SetValue(args.CurrentValue.Value);
			}

			if (args.IsIndeterminate == true)
			{
				splashScreen?.SetIndeterminate();
			}

			if (args.MaxValue.HasValue)
			{
				splashScreen?.SetMaxValue(args.MaxValue.Value);
			}

			if (args.Progress == true)
			{
				splashScreen?.Progress();
			}

			if (args.Regress == true)
			{
				splashScreen?.Regress();
			}
		}

		private void Operations_StatusChanged(TextKey status)
		{
			splashScreen?.UpdateStatus(status, true);
		}

		private void Runtime_ConnectionLost()
		{
			logger.Error("Lost connection to the runtime!");
			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error);

			shutdown.Invoke();
		}

		private void Shell_QuitButtonClicked(System.ComponentModel.CancelEventArgs args)
		{
			terminationActivator.Pause();
			args.Cancel = !TryInitiateShutdown();
			terminationActivator.Resume();
		}

		private void TerminationActivator_Activated()
		{
			terminationActivator.Pause();
			TryInitiateShutdown();
			terminationActivator.Resume();
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

		private void InformAboutFailedApplicationTermination(ApplicationTerminationFailedEventArgs args)
		{
			var applicationList = string.Join(Environment.NewLine, args.Applications.Select(a => a.Name));
			var message = $"{text.Get(TextKey.MessageBox_ApplicationTerminationFailure)}{Environment.NewLine}{Environment.NewLine}{applicationList}";
			var title = text.Get(TextKey.MessageBox_ApplicationTerminationFailureTitle);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}

		private bool TryInitiateShutdown()
		{
			var hasQuitPassword = !String.IsNullOrEmpty(Settings.QuitPasswordHash);
			var requestShutdown = false;

			if (hasQuitPassword)
			{
				requestShutdown = TryValidateQuitPassword();
			}
			else
			{
				requestShutdown = TryConfirmShutdown();
			}

			if (requestShutdown)
			{
				var communication = runtime.RequestShutdown();

				if (communication.Success)
				{
					return true;
				}
				else
				{
					logger.Error("Failed to communicate shutdown request to the runtime!");
					messageBox.Show(TextKey.MessageBox_QuitError, TextKey.MessageBox_QuitErrorTitle, icon: MessageBoxIcon.Error);
				}
			}

			return false;
		}

		private bool TryConfirmShutdown()
		{
			var result = messageBox.Show(TextKey.MessageBox_Quit, TextKey.MessageBox_QuitTitle, MessageBoxAction.YesNo, MessageBoxIcon.Question);
			var quit = result == MessageBoxResult.Yes;
			
			if (quit)
			{
				logger.Info("The user chose to terminate the application.");
			}

			return quit;
		}

		private bool TryValidateQuitPassword()
		{
			var dialog = uiFactory.CreatePasswordDialog(TextKey.PasswordDialog_QuitPasswordRequired, TextKey.PasswordDialog_QuitPasswordRequiredTitle);
			var result = dialog.Show();

			if (result.Success)
			{
				var passwordHash = hashAlgorithm.GenerateHashFor(result.Password);
				var isCorrect = Settings.QuitPasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase);

				if (isCorrect)
				{
					logger.Info("The user entered the correct quit password, the application will now terminate.");
				}
				else
				{
					logger.Info("The user entered the wrong quit password.");
					messageBox.Show(TextKey.MessageBox_InvalidQuitPassword, TextKey.MessageBox_InvalidQuitPasswordTitle, icon: MessageBoxIcon.Warning);
				}

				return isCorrect;
			}

			return false;
		}
	}
}
