/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Client.Operations.Events;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client
{
	internal class ClientController
	{
		private readonly IActionCenter actionCenter;
		private readonly IApplicationMonitor applicationMonitor;
		private readonly ClientContext context;
		private readonly IDisplayMonitor displayMonitor;
		private readonly IExplorerShell explorerShell;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly IHashAlgorithm hashAlgorithm;
		private readonly ILogger logger;
		private readonly IMessageBox messageBox;
		private readonly IOperationSequence operations;
		private readonly IRegistry registry;
		private readonly IRuntimeProxy runtime;
		private readonly Action shutdown;
		private readonly ISplashScreen splashScreen;
		private readonly ISystemMonitor systemMonitor;
		private readonly ITaskbar taskbar;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private IBrowserApplication Browser => context.Browser;
		private IClientHost ClientHost => context.ClientHost;
		private IIntegrityModule IntegrityModule => context.IntegrityModule;
		private IServerProxy Server => context.Server;
		private AppSettings Settings => context.Settings;

		private ILockScreen lockScreen;
		private bool sessionLocked;

		internal ClientController(
			IActionCenter actionCenter,
			IApplicationMonitor applicationMonitor,
			ClientContext context,
			IDisplayMonitor displayMonitor,
			IExplorerShell explorerShell,
			IFileSystemDialog fileSystemDialog,
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			IMessageBox messageBox,
			IOperationSequence operations,
			IRegistry registry,
			IRuntimeProxy runtime,
			Action shutdown,
			ISplashScreen splashScreen,
			ISystemMonitor systemMonitor,
			ITaskbar taskbar,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.actionCenter = actionCenter;
			this.applicationMonitor = applicationMonitor;
			this.context = context;
			this.displayMonitor = displayMonitor;
			this.explorerShell = explorerShell;
			this.fileSystemDialog = fileSystemDialog;
			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
			this.messageBox = messageBox;
			this.operations = operations;
			this.registry = registry;
			this.runtime = runtime;
			this.shutdown = shutdown;
			this.splashScreen = splashScreen;
			this.systemMonitor = systemMonitor;
			this.taskbar = taskbar;
			this.text = text;
			this.uiFactory = uiFactory;
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
				RegisterEvents();
				ShowShell();
				AutoStartApplications();
				ScheduleIntegrityVerification();

				var communication = runtime.InformClientReady();

				if (communication.Success)
				{
					logger.Info("Application successfully initialized.");
					logger.Log(string.Empty);

					VerifySessionIntegrity();
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

			CloseShell();
			DeregisterEvents();
			UpdateSessionIntegrity();
			TerminateIntegrityVerification();

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

		private void RegisterEvents()
		{
			actionCenter.QuitButtonClicked += Shell_QuitButtonClicked;
			applicationMonitor.ExplorerStarted += ApplicationMonitor_ExplorerStarted;
			applicationMonitor.TerminationFailed += ApplicationMonitor_TerminationFailed;
			Browser.ConfigurationDownloadRequested += Browser_ConfigurationDownloadRequested;
			Browser.SessionIdentifierDetected += Browser_SessionIdentifierDetected;
			Browser.TerminationRequested += Browser_TerminationRequested;
			Browser.LoseFocusRequested += Browser_LoseFocusRequested;
			ClientHost.ExamSelectionRequested += ClientHost_ExamSelectionRequested;
			ClientHost.MessageBoxRequested += ClientHost_MessageBoxRequested;
			ClientHost.PasswordRequested += ClientHost_PasswordRequested;
			ClientHost.ReconfigurationAborted += ClientHost_ReconfigurationAborted;
			ClientHost.ReconfigurationDenied += ClientHost_ReconfigurationDenied;
			ClientHost.ServerFailureActionRequested += ClientHost_ServerFailureActionRequested;
			ClientHost.Shutdown += ClientHost_Shutdown;
			displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
			registry.ValueChanged += Registry_ValueChanged;
			runtime.ConnectionLost += Runtime_ConnectionLost;
			systemMonitor.SessionSwitched += SystemMonitor_SessionSwitched;
			taskbar.LoseFocusRequested += Taskbar_LoseFocusRequested;
			taskbar.QuitButtonClicked += Shell_QuitButtonClicked;

			foreach (var activator in context.Activators.OfType<ITerminationActivator>())
			{
				activator.Activated += TerminationActivator_Activated;
			}

			if (Server != null)
			{
				Server.LockScreenConfirmed += Server_LockScreenConfirmed;
				Server.LockScreenRequested += Server_LockScreenRequested;
				Server.TerminationRequested += Server_TerminationRequested;
			}
		}

		private void Taskbar_LoseFocusRequested(bool forward)
		{
			Browser.Focus(forward);
		}

		private void Browser_LoseFocusRequested(bool forward)
		{
			taskbar.Focus(forward);
		}

		private void DeregisterEvents()
		{
			actionCenter.QuitButtonClicked -= Shell_QuitButtonClicked;
			applicationMonitor.ExplorerStarted -= ApplicationMonitor_ExplorerStarted;
			applicationMonitor.TerminationFailed -= ApplicationMonitor_TerminationFailed;
			displayMonitor.DisplayChanged -= DisplayMonitor_DisplaySettingsChanged;
			registry.ValueChanged -= Registry_ValueChanged;
			runtime.ConnectionLost -= Runtime_ConnectionLost;
			systemMonitor.SessionSwitched -= SystemMonitor_SessionSwitched;
			taskbar.QuitButtonClicked -= Shell_QuitButtonClicked;

			if (Browser != null)
			{
				Browser.ConfigurationDownloadRequested -= Browser_ConfigurationDownloadRequested;
				Browser.SessionIdentifierDetected -= Browser_SessionIdentifierDetected;
				Browser.TerminationRequested -= Browser_TerminationRequested;
				Browser.LoseFocusRequested -= Browser_LoseFocusRequested;
			}

			if (ClientHost != null)
			{
				ClientHost.ExamSelectionRequested -= ClientHost_ExamSelectionRequested;
				ClientHost.MessageBoxRequested -= ClientHost_MessageBoxRequested;
				ClientHost.PasswordRequested -= ClientHost_PasswordRequested;
				ClientHost.ReconfigurationAborted -= ClientHost_ReconfigurationAborted;
				ClientHost.ReconfigurationDenied -= ClientHost_ReconfigurationDenied;
				ClientHost.ServerFailureActionRequested -= ClientHost_ServerFailureActionRequested;
				ClientHost.Shutdown -= ClientHost_Shutdown;
			}

			if (Server != null)
			{
				Server.LockScreenConfirmed -= Server_LockScreenConfirmed;
				Server.LockScreenRequested -= Server_LockScreenRequested;
				Server.TerminationRequested -= Server_TerminationRequested;
			}

			foreach (var activator in context.Activators.OfType<ITerminationActivator>())
			{
				activator.Activated -= TerminationActivator_Activated;
			}
		}

		private void CloseShell()
		{
			if (Settings?.ActionCenter.EnableActionCenter == true)
			{
				actionCenter.Close();
			}

			if (Settings?.Taskbar.EnableTaskbar == true)
			{
				taskbar.Close();
			}
		}

		private void ShowShell()
		{
			if (Settings.ActionCenter.EnableActionCenter)
			{
				actionCenter.Promote();
			}

			if (Settings.Taskbar.EnableTaskbar)
			{
				taskbar.Show();
			}
		}

		private void AutoStartApplications()
		{
			if (Settings.Browser.EnableBrowser && Browser.AutoStart)
			{
				logger.Info("Auto-starting browser...");
				Browser.Start();
			}

			foreach (var application in context.Applications)
			{
				if (application.AutoStart)
				{
					logger.Info($"Auto-starting '{application.Name}'...");
					application.Start();
				}
			}
		}

		private void ScheduleIntegrityVerification()
		{
			const int FIVE_MINUTES = 300000;
			const int TEN_MINUTES = 600000;

			var timer = new System.Timers.Timer();

			timer.AutoReset = false;
			timer.Elapsed += (o, args) =>
			{
				logger.Info($"Attempting to verify application integrity...");

				if (IntegrityModule.TryVerifyCodeSignature(out var isValid))
				{
					if (isValid)
					{
						logger.Info("Application integrity successfully verified.");
					}
					else
					{
						logger.Warn("Application integrity is compromised!");
						ShowLockScreen(text.Get(TextKey.LockScreen_ApplicationIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
					}
				}
				else
				{
					logger.Warn("Failed to verify application integrity!");
				}
			};
			timer.Interval = TEN_MINUTES + (new Random().NextDouble() * FIVE_MINUTES);
			timer.Start();

			if (Settings.Service.IgnoreService)
			{
				registry.StartMonitoring(RegistryValue.MachineHive.EaseOfAccess_Key, RegistryValue.MachineHive.EaseOfAccess_Name);
			}
		}

		private void VerifySessionIntegrity()
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings.Security.QuitPasswordHash);

			if (hasQuitPassword)
			{
				logger.Info($"Attempting to verify session integrity...");

				if (IntegrityModule.TryVerifySessionIntegrity(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl, out var isValid))
				{
					if (isValid)
					{
						logger.Info("Session integrity successfully verified.");
						IntegrityModule.CacheSession(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl);
					}
					else
					{
						logger.Warn("Session integrity is compromised!");
						Task.Delay(1000).ContinueWith(_ =>
						{
							ShowLockScreen(text.Get(TextKey.LockScreen_SessionIntegrityMessage), text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
						});
					}
				}
				else
				{
					logger.Warn("Failed to verify session integrity!");
				}
			}
		}

		private void UpdateSessionIntegrity()
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings?.Security.QuitPasswordHash);

			if (hasQuitPassword)
			{
				IntegrityModule?.ClearSession(Settings.Browser.ConfigurationKey, Settings.Browser.StartUrl);
			}
		}

		private void TerminateIntegrityVerification()
		{
			registry.StopMonitoring();
		}

		private void ApplicationMonitor_ExplorerStarted()
		{
			logger.Info("Trying to terminate Windows explorer...");
			explorerShell.Terminate();
			logger.Info("Re-initializing working area...");
			displayMonitor.InitializePrimaryDisplay(Settings.Taskbar.EnableTaskbar ? taskbar.GetAbsoluteHeight() : 0);
			logger.Info("Re-initializing shell...");
			actionCenter.InitializeBounds();
			taskbar.InitializeBounds();
			logger.Info("Desktop successfully restored.");
		}

		private void ApplicationMonitor_TerminationFailed(IEnumerable<RunningApplication> applications)
		{
			var applicationList = string.Join(Environment.NewLine, applications.Select(a => $"- {a.Name}"));
			var message = $"{text.Get(TextKey.LockScreen_ApplicationsMessage)}{Environment.NewLine}{Environment.NewLine}{applicationList}";
			var title = text.Get(TextKey.LockScreen_Title);
			var allowOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_ApplicationsAllowOption) };
			var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_ApplicationsTerminateOption) };

			logger.Warn("Detected termination failure of blacklisted application(s)!");

			var result = ShowLockScreen(message, title, new[] { allowOption, terminateOption });

			if (result.OptionId == allowOption.Id)
			{
				logger.Info($"The blacklisted application(s) {string.Join(", ", applications.Select(a => $"'{a.Name}'"))} will be temporarily allowed.");
			}
			else if (result.OptionId == terminateOption.Id)
			{
				logger.Info("Attempting to shutdown as requested by the user...");
				TryRequestShutdown();
			}
		}

		private void Browser_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			var allow = false;
			var hasQuitPassword = !string.IsNullOrWhiteSpace(Settings.Security.QuitPasswordHash);
			var hasUrl = !string.IsNullOrWhiteSpace(Settings.Security.ReconfigurationUrl);

			if (hasQuitPassword)
			{
				if (hasUrl)
				{
					var expression = Regex.Escape(Settings.Security.ReconfigurationUrl).Replace(@"\*", ".*");
					var regex = new Regex($"^{expression}$", RegexOptions.IgnoreCase);
					var sebUrl = args.Url.Replace(Uri.UriSchemeHttps, context.AppConfig.SebUriSchemeSecure).Replace(Uri.UriSchemeHttp, context.AppConfig.SebUriScheme);

					allow = Settings.Security.AllowReconfiguration && (regex.IsMatch(args.Url) || regex.IsMatch(sebUrl));
				}
				else
				{
					logger.Warn("The active configuration does not contain a valid reconfiguration URL!");
				}
			}
			else
			{
				allow = Settings.ConfigurationMode == ConfigurationMode.ConfigureClient || Settings.Security.AllowReconfiguration;
			}

			if (allow)
			{
				args.AllowDownload = true;
				args.Callback = Browser_ConfigurationDownloadFinished;
				args.DownloadPath = Path.Combine(context.AppConfig.TemporaryDirectory, fileName);

				splashScreen.Show();
				splashScreen.BringToForeground();
				splashScreen.SetIndeterminate();
				splashScreen.UpdateStatus(TextKey.OperationStatus_InitializeSession, true);

				logger.Info($"Allowed download request for configuration file '{fileName}'.");
			}
			else
			{
				args.AllowDownload = false;
				logger.Info($"Denied download request for configuration file '{fileName}'.");
			}
		}

		private void Browser_ConfigurationDownloadFinished(bool success, string url, string filePath = null)
		{
			if (success)
			{
				var communication = runtime.RequestReconfiguration(filePath, url);

				if (communication.Success)
				{
					logger.Info($"Sent reconfiguration request for '{filePath}' to the runtime.");
				}
				else
				{
					logger.Error($"Failed to communicate reconfiguration request for '{filePath}'!");
					messageBox.Show(TextKey.MessageBox_ReconfigurationError, TextKey.MessageBox_ReconfigurationErrorTitle, icon: MessageBoxIcon.Error, parent: splashScreen);
					splashScreen.Hide();
				}
			}
			else
			{
				logger.Error($"Failed to download configuration file '{filePath}'!");
				messageBox.Show(TextKey.MessageBox_ConfigurationDownloadError, TextKey.MessageBox_ConfigurationDownloadErrorTitle, icon: MessageBoxIcon.Error, parent: splashScreen);
				splashScreen.Hide();
			}
		}

		private void Browser_SessionIdentifierDetected(string identifier)
		{
			if (Settings.SessionMode == SessionMode.Server)
			{
				var response = Server.SendSessionIdentifier(identifier);

				while (!response.Success)
				{
					logger.Error($"Failed to communicate session identifier with server! {response.Message}");
					Thread.Sleep(Settings.Server.RequestAttemptInterval);
					response = Server.SendSessionIdentifier(identifier);
				}
			}
		}

		private void Browser_TerminationRequested()
		{
			logger.Info("Attempting to shutdown as requested by the browser...");
			TryRequestShutdown();
		}

		private void ClientHost_ExamSelectionRequested(ExamSelectionRequestEventArgs args)
		{
			logger.Info($"Received exam selection request with id '{args.RequestId}'.");

			var exams = args.Exams.Select(e => new Exam { Id = e.id, LmsName = e.lms, Name = e.name, Url = e.url });
			var dialog = uiFactory.CreateExamSelectionDialog(exams);
			var result = dialog.Show();

			runtime.SubmitExamSelectionResult(args.RequestId, result.Success, result.SelectedExam?.Id);
			logger.Info($"Exam selection request with id '{args.RequestId}' is complete.");
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
		}

		private void ClientHost_ReconfigurationAborted()
		{
			logger.Info("The reconfiguration was aborted by the runtime.");
			splashScreen.Hide();
		}

		private void ClientHost_ReconfigurationDenied(ReconfigurationEventArgs args)
		{
			logger.Info($"The reconfiguration request for '{args.ConfigurationPath}' was denied by the runtime!");
			messageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle, parent: splashScreen);
			splashScreen.Hide();
		}

		private void ClientHost_ServerFailureActionRequested(ServerFailureActionRequestEventArgs args)
		{
			logger.Info($"Received server failure action request with id '{args.RequestId}'.");

			var dialog = uiFactory.CreateServerFailureDialog(args.Message, args.ShowFallback);
			var result = dialog.Show(splashScreen);

			runtime.SubmitServerFailureActionResult(args.RequestId, result.Abort, result.Fallback, result.Retry);
			logger.Info($"Server failure action request with id '{args.RequestId}' is complete.");
		}

		private void ClientHost_Shutdown()
		{
			shutdown.Invoke();
		}

		private void DisplayMonitor_DisplaySettingsChanged()
		{
			logger.Info("Re-initializing working area...");
			displayMonitor.InitializePrimaryDisplay(Settings.Taskbar.EnableTaskbar ? taskbar.GetAbsoluteHeight() : 0);

			logger.Info("Re-initializing shell...");
			actionCenter.InitializeBounds();
			lockScreen?.InitializeBounds();
			taskbar.InitializeBounds();

			logger.Info("Desktop successfully restored.");

			if (!displayMonitor.ValidateConfiguration(Settings.Display).IsAllowed)
			{
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_DisplayConfigurationContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_DisplayConfigurationTerminateOption) };
				var message = text.Get(TextKey.LockScreen_DisplayConfigurationMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

				if (result.OptionId == terminateOption.Id)
				{
					logger.Info("Attempting to shutdown as requested by the user...");
					TryRequestShutdown();
				}
			}
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

		private void Registry_ValueChanged(object oldValue, object newValue)
		{
			logger.Warn($"The ease of access registry value has changed from '{oldValue}' to '{newValue}'! Attempting to show lock screen...");

			if (!sessionLocked)
			{
				var message = text.Get(TextKey.LockScreen_EaseOfAccessMessage);
				var title = text.Get(TextKey.LockScreen_Title);
				var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_EaseOfAccessContinueOption) };
				var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_EaseOfAccessTerminateOption) };

				sessionLocked = true;
				registry.StopMonitoring();

				var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

				if (result.OptionId == continueOption.Id)
				{
					logger.Info("The session will be allowed to resume as requested by the user...");
				}
				else if (result.OptionId == terminateOption.Id)
				{
					logger.Info("Attempting to shutdown as requested by the user...");
					TryRequestShutdown();
				}

				sessionLocked = false;
			}
			else
			{
				logger.Info("Lock screen is already active.");
			}
		}

		private void Runtime_ConnectionLost()
		{
			logger.Error("Lost connection to the runtime!");
			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error);

			shutdown.Invoke();
		}

		private void Server_LockScreenConfirmed()
		{
			logger.Info("Closing lock screen as requested by the server...");
			lockScreen?.Cancel();
		}

		private void Server_LockScreenRequested(string message)
		{
			logger.Info("Attempting to show lock screen as requested by the server...");

			if (!sessionLocked)
			{
				sessionLocked = true;
				ShowLockScreen(message, text.Get(TextKey.LockScreen_Title), Enumerable.Empty<LockScreenOption>());
				sessionLocked = false;
			}
			else
			{
				logger.Info("Lock screen is already active.");
			}
		}

		private void Server_TerminationRequested()
		{
			logger.Info("Attempting to shutdown as requested by the server...");
			TryRequestShutdown();
		}

		private void Shell_QuitButtonClicked(System.ComponentModel.CancelEventArgs args)
		{
			PauseActivators();
			args.Cancel = !TryInitiateShutdown();
			ResumeActivators();
		}

		private void SystemMonitor_SessionSwitched()
		{
			var allow = !Settings.Service.IgnoreService && (!Settings.Service.DisableUserLock || !Settings.Service.DisableUserSwitch);
			var message = text.Get(TextKey.LockScreen_UserSessionMessage);
			var title = text.Get(TextKey.LockScreen_Title);
			var continueOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_UserSessionContinueOption) };
			var terminateOption = new LockScreenOption { Text = text.Get(TextKey.LockScreen_UserSessionTerminateOption) };

			if (allow)
			{
				logger.Info("Detected user session switch, but user lock and/or user switch are allowed.");
			}
			else
			{
				logger.Warn("Detected user session switch!");

				if (!sessionLocked)
				{
					sessionLocked = true;

					var result = ShowLockScreen(message, title, new[] { continueOption, terminateOption });

					if (result.OptionId == terminateOption.Id)
					{
						logger.Info("Attempting to shutdown as requested by the user...");
						TryRequestShutdown();
					}

					sessionLocked = false;
				}
				else
				{
					logger.Info("Lock screen is already active.");
				}
			}
		}

		private void TerminationActivator_Activated()
		{
			PauseActivators();
			TryInitiateShutdown();
			ResumeActivators();
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

			switch (args.Result)
			{
				case FactoryResult.NotFound:
					messageKey = TextKey.MessageBox_ApplicationNotFound;
					titleKey = TextKey.MessageBox_ApplicationNotFoundTitle;
					break;
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

		private void PauseActivators()
		{
			foreach (var activator in context.Activators)
			{
				activator.Pause();
			}
		}

		private void ResumeActivators()
		{
			foreach (var activator in context.Activators)
			{
				activator.Resume();
			}
		}

		private LockScreenResult ShowLockScreen(string message, string title, IEnumerable<LockScreenOption> options)
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings.Security.QuitPasswordHash);
			var result = default(LockScreenResult);

			logger.Info("Showing lock screen...");
			PauseActivators();
			lockScreen = uiFactory.CreateLockScreen(message, title, options);
			lockScreen.Show();

			if (Settings.SessionMode == SessionMode.Server)
			{
				var response = Server.LockScreen(message);

				if (!response.Success)
				{
					logger.Error($"Failed to send lock screen notification to server! Message: {response.Message}.");
				}
			}

			for (var unlocked = false; !unlocked;)
			{
				result = lockScreen.WaitForResult();

				if (result.Canceled)
				{
					logger.Info("The lock screen has been automaticaly canceled.");
					unlocked = true;
				}
				else if (hasQuitPassword)
				{
					var passwordHash = hashAlgorithm.GenerateHashFor(result.Password);
					var isCorrect = Settings.Security.QuitPasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase);

					if (isCorrect)
					{
						logger.Info("The user entered the correct unlock password.");
						unlocked = true;
					}
					else
					{
						logger.Info("The user entered the wrong unlock password.");
						messageBox.Show(TextKey.MessageBox_InvalidUnlockPassword, TextKey.MessageBox_InvalidUnlockPasswordTitle, icon: MessageBoxIcon.Warning, parent: lockScreen);
					}
				}
				else
				{
					logger.Warn($"No unlock password is defined, allowing user to resume session!");
					unlocked = true;
				}
			}

			lockScreen.Close();
			ResumeActivators();
			logger.Info("Closed lock screen.");

			if (Settings.SessionMode == SessionMode.Server)
			{
				var response = Server.ConfirmLockScreen();

				if (!response.Success)
				{
					logger.Error($"Failed to send lock screen confirm notification to server! Message: {response.Message}.");
				}
			}

			return result;
		}

		private bool TryInitiateShutdown()
		{
			var hasQuitPassword = !string.IsNullOrEmpty(Settings.Security.QuitPasswordHash);
			var requestShutdown = false;
			var succes = false;

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
				succes = TryRequestShutdown();
			}

			return succes;
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
				var isCorrect = Settings.Security.QuitPasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase);

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

		private bool TryRequestShutdown()
		{
			var communication = runtime.RequestShutdown();

			if (!communication.Success)
			{
				logger.Error("Failed to communicate shutdown request to the runtime!");
				messageBox.Show(TextKey.MessageBox_QuitError, TextKey.MessageBox_QuitErrorTitle, icon: MessageBoxIcon.Error);
			}

			return communication.Success;
		}
	}
}
