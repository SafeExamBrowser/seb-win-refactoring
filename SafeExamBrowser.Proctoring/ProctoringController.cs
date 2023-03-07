/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Events;
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
		public bool IsHandRaised { get; private set; }
		public string Tooltip { get; set; }

		public event ProctoringEventHandler HandLowered;
		public event ProctoringEventHandler HandRaised;
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

			server.HandConfirmed += Server_HandConfirmed;
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
				start = !string.IsNullOrWhiteSpace(settings.Zoom.SdkKey) && !string.IsNullOrWhiteSpace(settings.Zoom.Signature);
				start &= !string.IsNullOrWhiteSpace(settings.Zoom.MeetingNumber);
				start &= !string.IsNullOrWhiteSpace(settings.Zoom.UserName);
			}

			if (start)
			{
				StartProctoring();
			}
		}

		public void LowerHand()
		{
			var response = server.LowerHand();

			if (response.Success)
			{
				IsHandRaised = false;
				HandLowered?.Invoke();
				logger.Info("Hand lowered.");
			}
			else
			{
				logger.Error($"Failed to send lower hand notification to server! Message: {response.Message}.");
			}
		}

		public void RaiseHand(string message = null)
		{
			var response = server.RaiseHand(message);

			if (response.Success)
			{
				IsHandRaised = true;
				HandRaised?.Invoke();
				logger.Info("Hand raised.");
			}
			else
			{
				logger.Error($"Failed to send raise hand notification to server! Message: {response.Message}.");
			}
		}

		public void Terminate()
		{
			StopProctoring();
		}

		private void Server_HandConfirmed()
		{
			logger.Info("Hand confirmation received.");

			IsHandRaised = false;
			HandLowered?.Invoke();
		}

		private void Server_ProctoringInstructionReceived(ProctoringInstructionEventArgs args)
		{
			logger.Info("Proctoring instruction received.");

			settings.JitsiMeet.RoomName = args.JitsiMeetRoomName;
			settings.JitsiMeet.ServerUrl = args.JitsiMeetServerUrl;
			settings.JitsiMeet.Token = args.JitsiMeetToken;

			settings.Zoom.MeetingNumber = args.ZoomMeetingNumber;
			settings.Zoom.Password = args.ZoomPassword;
			settings.Zoom.SdkKey = args.ZoomSdkKey;
			settings.Zoom.Signature = args.ZoomSignature;
			settings.Zoom.Subject = args.ZoomSubject;
			settings.Zoom.UserName = args.ZoomUserName;

			StopProctoring();
			StartProctoring();
		}

		private void Server_ProctoringConfigurationReceived(bool allowChat, bool receiveAudio, bool receiveVideo)
		{
			logger.Info("Proctoring configuration received.");

			settings.JitsiMeet.AllowChat = allowChat;
			settings.JitsiMeet.ReceiveAudio = receiveAudio;
			settings.JitsiMeet.ReceiveVideo = receiveVideo;

			settings.Zoom.AllowChat = allowChat;
			settings.Zoom.ReceiveAudio = receiveAudio;
			settings.Zoom.ReceiveVideo = receiveVideo;

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

					control = new ProctoringControl(logger.CloneFor(nameof(ProctoringControl)), settings);
					control.CreationProperties = new CoreWebView2CreationProperties { UserDataFolder = appConfig.TemporaryDirectory };
					control.EnsureCoreWebView2Async().ContinueWith(_ =>
					{
						control.Dispatcher.Invoke(() =>
						{
							control.CoreWebView2.Navigate(filePath);
						});
					});

					window = uiFactory.CreateProctoringWindow(control);
					window.SetTitle(settings.JitsiMeet.Enabled ? settings.JitsiMeet.Subject : settings.Zoom.Subject);
					window.Show();

					if (settings.WindowVisibility == WindowVisibility.AllowToShow || settings.WindowVisibility == WindowVisibility.Hidden)
					{
						if (settings.Zoom.Enabled)
						{
							window.HideWithDelay();
						}
						else
						{
							window.Hide();
						}
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
					if (settings.JitsiMeet.Enabled)
					{
						control.ExecuteScriptAsync("api.executeCommand('hangup'); api.dispose();");
					}
					else if (settings.Zoom.Enabled)
					{
						control.ExecuteScriptAsync("ZoomMtg.leaveMeeting({});");
					}

					Thread.Sleep(2000);

					window.Close();
					control = default;
					window = default;
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
					html = html.Replace("%%_ALLOW_CLOSED_CAPTIONS_%%", settings.JitsiMeet.AllowClosedCaptions ? "closedcaptions" : "");
					html = html.Replace("%%_ALLOW_RAISE_HAND_%%", settings.JitsiMeet.AllowRaiseHand ? "raisehand" : "");
					html = html.Replace("%%_ALLOW_RECORDING_%%", settings.JitsiMeet.AllowRecording ? "recording" : "");
					html = html.Replace("%%_ALLOW_TILE_VIEW", settings.JitsiMeet.AllowTileView ? "tileview" : "");
					html = html.Replace("'%_AUDIO_MUTED_%'", settings.JitsiMeet.AudioMuted && settings.WindowVisibility != WindowVisibility.Hidden ? "true" : "false");
					html = html.Replace("'%_AUDIO_ONLY_%'", settings.JitsiMeet.AudioOnly ? "true" : "false");
					html = html.Replace("'%_VIDEO_MUTED_%'", settings.JitsiMeet.VideoMuted && settings.WindowVisibility != WindowVisibility.Hidden ? "true" : "false");
				}
				else if (settings.Zoom.Enabled)
				{
					html = html.Replace("'%_ALLOW_CHAT_%'", settings.Zoom.AllowChat ? "true" : "false");
					html = html.Replace("'%_ALLOW_CLOSED_CAPTIONS_%'", settings.Zoom.AllowClosedCaptions ? "true" : "false");
					html = html.Replace("'%_ALLOW_RAISE_HAND_%'", settings.Zoom.AllowRaiseHand ? "true" : "false");
					html = html.Replace("'%_AUDIO_MUTED_%'", settings.Zoom.AudioMuted && settings.WindowVisibility != WindowVisibility.Hidden ? "true" : "false");
					html = html.Replace("'%_VIDEO_MUTED_%'", settings.Zoom.VideoMuted && settings.WindowVisibility != WindowVisibility.Hidden ? "true" : "false");
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
