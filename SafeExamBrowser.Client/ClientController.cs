/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
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
		private IDisplayMonitor displayMonitor;
		private IExplorerShell explorerShell;
		private IHashAlgorithm hashAlgorithm;
		private ILogger logger;
		private IMessageBox messageBox;
		private IOperationSequence operations;
		private IProcessMonitor processMonitor;
		private IRuntimeProxy runtime;
		private Action shutdown;
		private ISplashScreen splashScreen;
		private ITaskbar taskbar;
		private ITerminationActivator terminationActivator;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private IWindowMonitor windowMonitor;
		private AppConfig appConfig;

		public IBrowserApplication Browser { private get; set; }
		public IClientHost ClientHost { private get; set; }
		public Guid SessionId { private get; set; }
		public Settings Settings { private get; set; }

		public AppConfig AppConfig
		{
			set
			{
				appConfig = value;

				if (splashScreen != null)
				{
					splashScreen.AppConfig = value;
				}
			}
		}

		public ClientController(
			IActionCenter actionCenter,
			IDisplayMonitor displayMonitor,
			IExplorerShell explorerShell,
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			IMessageBox messageBox,
			IOperationSequence operations,
			IProcessMonitor processMonitor,
			IRuntimeProxy runtime,
			Action shutdown,
			ITaskbar taskbar,
			ITerminationActivator terminationActivator,
			IText text,
			IUserInterfaceFactory uiFactory,
			IWindowMonitor windowMonitor)
		{
			this.actionCenter = actionCenter;
			this.displayMonitor = displayMonitor;
			this.explorerShell = explorerShell;
			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
			this.messageBox = messageBox;
			this.operations = operations;
			this.processMonitor = processMonitor;
			this.runtime = runtime;
			this.shutdown = shutdown;
			this.taskbar = taskbar;
			this.terminationActivator = terminationActivator;
			this.text = text;
			this.uiFactory = uiFactory;
			this.windowMonitor = windowMonitor;
		}

		public bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			splashScreen = uiFactory.CreateSplashScreen();
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

			splashScreen = uiFactory.CreateSplashScreen(appConfig);
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

		private void RegisterEvents()
		{
			actionCenter.QuitButtonClicked += Shell_QuitButtonClicked;
			Browser.ConfigurationDownloadRequested += Browser_ConfigurationDownloadRequested;
			ClientHost.MessageBoxRequested += ClientHost_MessageBoxRequested;
			ClientHost.PasswordRequested += ClientHost_PasswordRequested;
			ClientHost.ReconfigurationDenied += ClientHost_ReconfigurationDenied;
			ClientHost.Shutdown += ClientHost_Shutdown;
			displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
			processMonitor.ExplorerStarted += ProcessMonitor_ExplorerStarted;
			runtime.ConnectionLost += Runtime_ConnectionLost;
			taskbar.QuitButtonClicked += Shell_QuitButtonClicked;
			terminationActivator.Activated += TerminationActivator_Activated;
			windowMonitor.WindowChanged += WindowMonitor_WindowChanged;
		}

		private void DeregisterEvents()
		{
			actionCenter.QuitButtonClicked -= Shell_QuitButtonClicked;
			displayMonitor.DisplayChanged -= DisplayMonitor_DisplaySettingsChanged;
			processMonitor.ExplorerStarted -= ProcessMonitor_ExplorerStarted;
			runtime.ConnectionLost -= Runtime_ConnectionLost;
			taskbar.QuitButtonClicked -= Shell_QuitButtonClicked;
			terminationActivator.Activated -= TerminationActivator_Activated;
			windowMonitor.WindowChanged -= WindowMonitor_WindowChanged;

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

		private void Browser_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			if (Settings.ConfigurationMode == ConfigurationMode.ConfigureClient)
			{
				logger.Info($"Received download request for configuration file '{fileName}'. Asking user to confirm the reconfiguration...");

				var message = TextKey.MessageBox_ReconfigurationQuestion;
				var title = TextKey.MessageBox_ReconfigurationQuestionTitle;
				var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question/*// TODO , args.BrowserWindow*/);
				var reconfigure = result == MessageBoxResult.Yes;

				logger.Info($"The user chose to {(reconfigure ? "start" : "abort")} the reconfiguration.");

				if (reconfigure)
				{
					args.AllowDownload = true;
					args.Callback = Browser_ConfigurationDownloadFinished;
					args.DownloadPath = Path.Combine(appConfig.DownloadDirectory, fileName);
				}
			}
			else
			{
				logger.Info($"Denied download request for configuration file '{fileName}' due to '{Settings.ConfigurationMode}' mode.");
				messageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle/*,// TODO  parent: args.BrowserWindow*/);
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

					splashScreen = uiFactory.CreateSplashScreen(appConfig);
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

		private void ProcessMonitor_ExplorerStarted()
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

		private void WindowMonitor_WindowChanged(IntPtr window)
		{
			var allowed = processMonitor.BelongsToAllowedProcess(window);

			if (!allowed)
			{
				var success = windowMonitor.Hide(window);

				if (!success)
				{
					windowMonitor.Close(window);
				}
			}
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
