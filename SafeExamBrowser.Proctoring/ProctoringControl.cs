/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Proctoring.Events;

namespace SafeExamBrowser.Proctoring
{
	internal class ProctoringControl : WebView2, IProctoringControl
	{
		private readonly ILogger logger;
		private readonly ProctoringSettings settings;

		public event FullScreenChangedEventHandler FullScreenChanged;

		internal ProctoringControl(ILogger logger, ProctoringSettings settings)
		{
			this.logger = logger;
			this.settings = settings;

			CoreWebView2InitializationCompleted += ProctoringControl_CoreWebView2InitializationCompleted;
		}

		private void ProctoringControl_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			if (e.IsSuccess)
			{
				CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
				CoreWebView2.Settings.AreDevToolsEnabled = false;
				CoreWebView2.Settings.IsStatusBarEnabled = false;
				CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;
				CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
				CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
				logger.Info("Successfully initialized.");
			}
			else
			{
				logger.Error("Failed to initialize!", e.InitializationException);
			}
		}

		private void CoreWebView2_ContainsFullScreenElementChanged(object sender, object e)
		{
			FullScreenChanged?.Invoke(CoreWebView2.ContainsFullScreenElement);
			logger.Debug($"Full screen {(CoreWebView2.ContainsFullScreenElement ? "activated" : "deactivated")}.");
		}

		private void CoreWebView2_PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
		{
			if (e.PermissionKind == CoreWebView2PermissionKind.Camera || e.PermissionKind == CoreWebView2PermissionKind.Microphone)
			{
				e.State = CoreWebView2PermissionState.Allow;
				logger.Info($"Granted access to {e.PermissionKind}.");
			}
			else
			{
				logger.Info($"Denied access to {e.PermissionKind}.");
			}
		}

		private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			var message = e.TryGetWebMessageAsString();

			logger.Debug($"Received web message '{message}'.");

			switch (message)
			{
				case "credentials":
					SendCredentials();
					break;
			}
		}

		private void SendCredentials()
		{
			var message = new JObject();
			var credentials = new JObject();

			if (settings.JitsiMeet.Enabled)
			{
				credentials.Add(new JProperty("domain", settings.JitsiMeet.ServerUrl));
				credentials.Add(new JProperty("roomName", settings.JitsiMeet.RoomName));
				credentials.Add(new JProperty("subject", settings.JitsiMeet.ShowMeetingName ? settings.JitsiMeet.Subject : ""));
				credentials.Add(new JProperty("token", settings.JitsiMeet.Token));
			}
			else if (settings.Zoom.Enabled)
			{
				credentials.Add(new JProperty("meetingNumber", settings.Zoom.MeetingNumber));
				credentials.Add(new JProperty("password", settings.Zoom.Password));
				credentials.Add(new JProperty("sdkKey", settings.Zoom.SdkKey));
				credentials.Add(new JProperty("signature", settings.Zoom.Signature));
				credentials.Add(new JProperty("userName", settings.Zoom.UserName));
			}

			message.Add("credentials", credentials);
			logger.Debug("Sending credentials to proctoring client.");

			CoreWebView2.PostWebMessageAsJson(message.ToString());
		}
	}
}
