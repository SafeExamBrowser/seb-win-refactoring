/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class MouseInterceptorOperation : IOperation
	{
		private ILogger logger;
		private IMouseInterceptor mouseInterceptor;
		private INativeMethods nativeMethods;

		public bool Abort { get; private set; }
		public ISplashScreen SplashScreen { private get; set; }

		public MouseInterceptorOperation(
			ILogger logger,
			IMouseInterceptor mouseInterceptor,
			INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.mouseInterceptor = mouseInterceptor;
			this.nativeMethods = nativeMethods;
		}

		public void Perform()
		{
			logger.Info("Starting mouse interception...");
			SplashScreen.UpdateText(TextKey.SplashScreen_StartMouseInterception);

			nativeMethods.RegisterMouseHook(mouseInterceptor);
		}

		public void Repeat()
		{
			// Nothing to do here...
		}

		public void Revert()
		{
			logger.Info("Stopping mouse interception...");
			SplashScreen.UpdateText(TextKey.SplashScreen_StopMouseInterception);

			nativeMethods.DeregisterMouseHook(mouseInterceptor);
		}
	}
}
