/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;

namespace SafeExamBrowser.Proctoring
{
	internal class ProctoringControl : WebView2, IProctoringControl
	{
		private readonly ILogger logger;

		internal ProctoringControl(ILogger logger)
		{
			this.logger = logger;
			CoreWebView2InitializationCompleted += ProctoringControl_CoreWebView2InitializationCompleted;
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

		private void ProctoringControl_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			if (e.IsSuccess)
			{
				CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
				logger.Info("Successfully initialized.");
			}
			else
			{
				logger.Error("Failed to initialize!", e.InitializationException);
			}
		}
	}
}
