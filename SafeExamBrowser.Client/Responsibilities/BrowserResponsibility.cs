/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class BrowserResponsibility : ClientResponsibility
	{
		private readonly ICoordinator coordinator;
		private readonly IMessageBox messageBox;
		private readonly IRuntimeProxy runtime;
		private readonly ISplashScreen splashScreen;
		private readonly ITaskbar taskbar;

		private IBrowserApplication Browser => Context.Browser;

		public BrowserResponsibility(
			ClientContext context,
			ICoordinator coordinator,
			ILogger logger,
			IMessageBox messageBox,
			IRuntimeProxy runtime,
			ISplashScreen splashScreen,
			ITaskbar taskbar) : base(context, logger)
		{
			this.coordinator = coordinator;
			this.messageBox = messageBox;
			this.runtime = runtime;
			this.splashScreen = splashScreen;
			this.taskbar = taskbar;
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.AutoStartApplications:
					AutoStartBrowser();
					break;
				case ClientTask.DeregisterEvents:
					DeregisterEvents();
					break;
				case ClientTask.RegisterEvents:
					RegisterEvents();
					break;
			}
		}

		private void AutoStartBrowser()
		{
			if (Settings.Browser.EnableBrowser && Browser.AutoStart)
			{
				Logger.Info("Auto-starting browser...");
				Browser.Start();
			}
		}

		private void DeregisterEvents()
		{
			if (Browser != default)
			{
				Browser.ConfigurationDownloadRequested -= Browser_ConfigurationDownloadRequested;
				Browser.LoseFocusRequested -= Browser_LoseFocusRequested;
				Browser.TerminationRequested -= Browser_TerminationRequested;
				Browser.UserIdentifierDetected -= Browser_UserIdentifierDetected;
			}
		}

		private void RegisterEvents()
		{
			Browser.ConfigurationDownloadRequested += Browser_ConfigurationDownloadRequested;
			Browser.LoseFocusRequested += Browser_LoseFocusRequested;
			Browser.TerminationRequested += Browser_TerminationRequested;
			Browser.UserIdentifierDetected += Browser_UserIdentifierDetected;
		}

		private void Browser_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			args.AllowDownload = false;

			if (IsAllowedToReconfigure(args.Url))
			{
				if (coordinator.RequestReconfigurationLock())
				{
					args.AllowDownload = true;
					args.Callback = Browser_ConfigurationDownloadFinished;
					args.DownloadPath = Path.Combine(Context.AppConfig.TemporaryDirectory, fileName);

					splashScreen.Show();
					splashScreen.BringToForeground();
					splashScreen.SetIndeterminate();
					splashScreen.UpdateStatus(TextKey.OperationStatus_InitializeSession, true);

					Logger.Info($"Allowed download request for configuration file '{fileName}'.");
				}
				else
				{
					Logger.Warn($"A reconfiguration is already in progress, denied download request for configuration file '{fileName}'!");
				}
			}
			else
			{
				Logger.Info($"Reconfiguration is not allowed, denied download request for configuration file '{fileName}'.");
			}
		}

		private bool IsAllowedToReconfigure(string url)
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
					var sebUrl = url.Replace(Uri.UriSchemeHttps, Context.AppConfig.SebUriSchemeSecure).Replace(Uri.UriSchemeHttp, Context.AppConfig.SebUriScheme);

					allow = Settings.Security.AllowReconfiguration && (regex.IsMatch(url) || regex.IsMatch(sebUrl));
				}
				else
				{
					Logger.Warn("The active configuration does not contain a valid reconfiguration URL!");
				}
			}
			else
			{
				allow = Settings.ConfigurationMode == ConfigurationMode.ConfigureClient || Settings.Security.AllowReconfiguration;
			}

			return allow;
		}

		private void Browser_ConfigurationDownloadFinished(bool success, string url, string filePath = null)
		{
			if (success)
			{
				PrepareShutdown();

				var communication = runtime.RequestReconfiguration(filePath, url);

				if (communication.Success)
				{
					Logger.Info($"Sent reconfiguration request for '{filePath}' to the runtime.");
				}
				else
				{
					Logger.Error($"Failed to communicate reconfiguration request for '{filePath}'!");

					messageBox.Show(TextKey.MessageBox_ReconfigurationError, TextKey.MessageBox_ReconfigurationErrorTitle, icon: MessageBoxIcon.Error, parent: splashScreen);
					splashScreen.Hide();
					coordinator.ReleaseReconfigurationLock();
				}
			}
			else
			{
				Logger.Error($"Failed to download configuration file '{filePath}'!");

				messageBox.Show(TextKey.MessageBox_ConfigurationDownloadError, TextKey.MessageBox_ConfigurationDownloadErrorTitle, icon: MessageBoxIcon.Error, parent: splashScreen);
				splashScreen.Hide();
				coordinator.ReleaseReconfigurationLock();
			}
		}

		private void Browser_LoseFocusRequested(bool forward)
		{
			taskbar.Focus(forward);
		}

		private void Browser_UserIdentifierDetected(string identifier)
		{
			if (Settings.SessionMode == SessionMode.Server)
			{
				var response = Context.Server.SendUserIdentifier(identifier);

				while (!response.Success)
				{
					Logger.Error($"Failed to communicate user identifier with server! {response.Message}");
					Thread.Sleep(Settings.Server.RequestAttemptInterval);
					response = Context.Server.SendUserIdentifier(identifier);
				}
			}
		}

		private void Browser_TerminationRequested()
		{
			Logger.Info("Attempting to shutdown as requested by the browser...");
			TryRequestShutdown();
		}
	}
}
