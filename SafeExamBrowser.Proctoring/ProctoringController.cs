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
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Server.Contracts;
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
		private readonly IServerProxy server;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private string filePath;
		private ProctoringControl control;
		private ProctoringSettings settings;
		private IProctoringWindow window;
		private WindowVisibility windowVisibility;

		public IconResource IconResource { get; set; }
		public string Tooltip { get; set; }

		public event NotificationChangedEventHandler NotificationChanged;

		public ProctoringController(
			AppConfig appConfig,
			IFileSystem fileSystem,
			IModuleLogger logger,
			IServerProxy server,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.fileSystem = fileSystem;
			this.logger = logger;
			this.server = server;
			this.text = text;
			this.uiFactory = uiFactory;

			IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ProctoringNotification_Inactive.xaml") };
			Tooltip = text.Get(TextKey.Notification_ProctoringInactiveTooltip);
		}

		public void Activate()
		{
			if (settings.WindowVisibility == WindowVisibility.Visible)
			{
				window?.BringToForeground();
			}
			else if (settings.WindowVisibility == WindowVisibility.AllowToHide || settings.WindowVisibility == WindowVisibility.AllowToShow)
			{
				window?.Toggle();
			}
		}

		public void Initialize(ProctoringSettings settings)
		{
			var start = false;

			this.settings = settings;
			this.windowVisibility = settings.WindowVisibility;

			server.ProctoringConfigurationReceived += Server_ProctoringConfigurationReceived;
			server.ProctoringInstructionReceived += Server_ProctoringInstructionReceived;

			if (settings.JitsiMeet.Enabled)
			{
				this.settings.JitsiMeet.ServerUrl = Sanitize(settings.JitsiMeet.ServerUrl);

				start = !string.IsNullOrWhiteSpace(settings.JitsiMeet.RoomName);
				start &= !string.IsNullOrWhiteSpace(settings.JitsiMeet.ServerUrl);
			}
			else if (settings.Zoom.Enabled)
			{
				start = !string.IsNullOrWhiteSpace(settings.Zoom.ApiKey);
				start &= !string.IsNullOrWhiteSpace(settings.Zoom.ApiSecret);
				start &= !string.IsNullOrWhiteSpace(settings.Zoom.MeetingNumber);
				start &= !string.IsNullOrWhiteSpace(settings.Zoom.UserName);
			}

			if (start)
			{
				StartProctoring();
			}
		}

		public void Terminate()
		{
			StopProctoring();
		}

		private void Server_ProctoringInstructionReceived(string roomName, string serverUrl, string token)
		{
			logger.Info("Proctoring instruction received.");

			settings.JitsiMeet.RoomName = roomName;
			settings.JitsiMeet.ServerUrl = Sanitize(serverUrl);
			settings.JitsiMeet.Token = token;

			StopProctoring();
			StartProctoring();
		}

		private void Server_ProctoringConfigurationReceived(bool allowChat, bool receiveAudio, bool receiveVideo)
		{
			logger.Info("Proctoring configuration received.");

			settings.JitsiMeet.AllowChat = allowChat;
			settings.JitsiMeet.ReceiveAudio = receiveAudio;
			settings.JitsiMeet.ReceiveVideo = receiveVideo;

			if (allowChat || receiveVideo)
			{
				settings.WindowVisibility = WindowVisibility.AllowToHide;
			}
			else
			{
				settings.WindowVisibility = windowVisibility;
			}

			StopProctoring();
			StartProctoring();
		}

		private void StartProctoring()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					var content = LoadContent(settings);

					filePath = Path.Combine(appConfig.TemporaryDirectory, $"{Path.GetRandomFileName()}_index.html");
					fileSystem.Save(content, filePath);

					control = new ProctoringControl(logger.CloneFor(nameof(ProctoringControl)));
					control.CreationProperties = new CoreWebView2CreationProperties { UserDataFolder = appConfig.TemporaryDirectory };
					control.EnsureCoreWebView2Async().ContinueWith(_ =>
					{
						control.Dispatcher.Invoke(() =>
						{
							control.CoreWebView2.Navigate(filePath);
						});
					});

					window = uiFactory.CreateProctoringWindow(control);
					window.SetTitle(settings.JitsiMeet.Enabled ? settings.JitsiMeet.Subject : settings.Zoom.UserName);
					window.Show();

					if (settings.WindowVisibility == WindowVisibility.AllowToShow || settings.WindowVisibility == WindowVisibility.Hidden)
					{
						window.Hide();
					}

					IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/ProctoringNotification_Active.xaml") };
					Tooltip = text.Get(TextKey.Notification_ProctoringActiveTooltip);
					NotificationChanged?.Invoke();

					logger.Info($"Started proctoring with {(settings.JitsiMeet.Enabled ? "Jitsi Meet" : "Zoom")}.");
				}
				catch (Exception e)
				{
					logger.Error($"Failed to start proctoring! Reason: {e.Message}", e);
				}
			});
		}

		private void StopProctoring()
		{
			if (control != default(ProctoringControl) && window != default(IProctoringWindow))
			{
				control.Dispatcher.Invoke(() =>
				{
					control.ExecuteScriptAsync("api.executeCommand('hangup'); api.dispose();");
					window.Close();
					control = default(ProctoringControl);
					window = default(IProctoringWindow);
					fileSystem.Delete(filePath);

					logger.Info("Stopped proctoring.");
				});
			}
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
					html = html.Replace("%%_ALLOW_CHAT_%%", settings.JitsiMeet.AllowChat ? "chat" : "");
					html = html.Replace("%%_ALLOW_CLOSED_CAPTIONS_%%", settings.JitsiMeet.AllowCloseCaptions ? "closedcaptions" : "");
					html = html.Replace("%%_ALLOW_RAISE_HAND_%%", settings.JitsiMeet.AllowRaiseHand ? "raisehand" : "");
					html = html.Replace("%%_ALLOW_RECORDING_%%", settings.JitsiMeet.AllowRecording ? "recording" : "");
					html = html.Replace("%%_ALLOW_TILE_VIEW", settings.JitsiMeet.AllowTileView ? "tileview" : "");
					html = html.Replace("'%_AUDIO_MUTED_%'", settings.JitsiMeet.AudioMuted && settings.WindowVisibility != WindowVisibility.Hidden ? "true" : "false");
					html = html.Replace("'%_AUDIO_ONLY_%'", settings.JitsiMeet.AudioOnly ? "true" : "false");
					html = html.Replace("%%_SUBJECT_%%", settings.JitsiMeet.ShowMeetingName ? settings.JitsiMeet.Subject : "   ");
					html = html.Replace("%%_DOMAIN_%%", settings.JitsiMeet.ServerUrl);
					html = html.Replace("%%_ROOM_NAME_%%", settings.JitsiMeet.RoomName);
					html = html.Replace("%%_TOKEN_%%", settings.JitsiMeet.Token);
					html = html.Replace("'%_VIDEO_MUTED_%'", settings.JitsiMeet.VideoMuted && settings.WindowVisibility != WindowVisibility.Hidden ? "true" : "false");
				}
				else if (settings.Zoom.Enabled)
				{
					html = html.Replace("%%_API_KEY_%%", settings.Zoom.ApiKey);
					html = html.Replace("%%_API_SECRET_%%", settings.Zoom.ApiSecret);
					html = html.Replace("%%_MEETING_NUMBER_%%", settings.Zoom.MeetingNumber);
					html = html.Replace("%%_USER_NAME_%%", settings.Zoom.UserName);
				}

				return html;
			}
		}

		private string Sanitize(string serverUrl)
		{
			return serverUrl?.Replace($"{Uri.UriSchemeHttp}{Uri.SchemeDelimiter}", "").Replace($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}", "");
		}
	}
}
