/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;

namespace SafeExamBrowser.Proctoring
{
	public class ProctoringController : IProctoringController, INotification
	{
		private readonly AppConfig appConfig;
		private readonly IFileSystem fileSystem;
		private readonly IModuleLogger logger;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private string filePath;
		private IProctoringWindow window;
		private ProctoringSettings settings;

		public string Tooltip { get; }
		public IconResource IconResource { get; set; }

		public event NotificationChangedEventHandler NotificationChanged;

		public ProctoringController(AppConfig appConfig, IFileSystem fileSystem, IModuleLogger logger, IText text, IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.fileSystem = fileSystem;
			this.logger = logger;
			this.text = text;
			this.uiFactory = uiFactory;

			IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ProctoringNotification_Inactive.xaml") };
			Tooltip = text.Get(TextKey.Notification_ProctoringTooltip);
		}

		public void Activate()
		{
			if (settings.WindowVisibility == WindowVisibility.Visible)
			{
				window?.BringToForeground();
			}
			else if (settings.WindowVisibility == WindowVisibility.AllowToHide || settings.WindowVisibility == WindowVisibility.AllowToShow)
			{
				window.Toggle();
			}
		}

		public void Initialize(ProctoringSettings settings)
		{
			this.settings = settings;

			if (settings.JitsiMeet.Enabled || settings.Zoom.Enabled)
			{
				var content = LoadContent(settings);
				var control = new ProctoringControl(logger.CloneFor(nameof(ProctoringControl)));

				filePath = Path.Combine(appConfig.TemporaryDirectory, $"{Path.GetRandomFileName()}_index.html");
				fileSystem.Save(content, filePath);

				control.EnsureCoreWebView2Async().ContinueWith(_ =>
				{
					control.Dispatcher.Invoke(() => control.CoreWebView2.Navigate(filePath));
				});

				window = uiFactory.CreateProctoringWindow(control);

				if (settings.WindowVisibility == WindowVisibility.AllowToHide || settings.WindowVisibility == WindowVisibility.Visible)
				{
					window.SetTitle(settings.JitsiMeet.Enabled ? settings.JitsiMeet.Subject : settings.Zoom.UserName);
					window.Show();
				}

				logger.Info($"Initialized proctoring with {(settings.JitsiMeet.Enabled ? "Jitsi Meet" : "Zoom")}.");

				IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ProctoringNotification_Active.xaml") };
				NotificationChanged?.Invoke();
			}
			else
			{
				logger.Warn("Failed to initialize remote proctoring because no provider is enabled in the active configuration.");
			}
		}

		void INotification.Terminate()
		{
			window?.Close();
		}

		void IProctoringController.Terminate()
		{
			fileSystem.Delete(filePath);
		}

		private string LoadContent(ProctoringSettings settings)
		{
			var provider = settings.JitsiMeet.Enabled ? "JitsiMeet" : "Zoom";
			var assembly = Assembly.GetAssembly(typeof(ProctoringController));
			var path = $"{typeof(ProctoringController).Namespace}.{provider}.index.html";

			using (var stream = assembly.GetManifestResourceStream(path))
			using (var reader = new StreamReader(stream))
			{
				var html = reader.ReadToEnd();

				if (settings.JitsiMeet.Enabled)
				{
					html = html.Replace("%%_DOMAIN_%%", settings.JitsiMeet.ServerUrl);
					html = html.Replace("%%_ROOM_NAME_%%", settings.JitsiMeet.RoomName);
					html = html.Replace("%%_SUBJECT_%%", settings.JitsiMeet.Subject);
					html = html.Replace("%%_TOKEN_%%", settings.JitsiMeet.Token);
				}
				else if (settings.Zoom.Enabled)
				{
					html = html.Replace("%%_API_KEY_%%", settings.Zoom.ApiKey);
					html = html.Replace("%%_API_SECRET_%%", settings.Zoom.ApiSecret);
					html = html.Replace("123456789", Convert.ToString(settings.Zoom.MeetingNumber));
					html = html.Replace("%%_USER_NAME_%%", settings.Zoom.UserName);
				}

				return html;
			}
		}
	}
}
