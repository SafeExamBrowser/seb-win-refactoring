/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class DeviceInterceptionOperation : IOperation
	{
		private IKeyboardInterceptor keyboardInterceptor;
		private ILogger logger;
		private IMouseInterceptor mouseInterceptor;
		private INativeMethods nativeMethods;

		public ISplashScreen SplashScreen { private get; set; }
		
		public DeviceInterceptionOperation(
			IKeyboardInterceptor keyboardInterceptor,
			ILogger logger,
			IMouseInterceptor mouseInterceptor,
			INativeMethods nativeMethods)
		{
			this.keyboardInterceptor = keyboardInterceptor;
			this.logger = logger;
			this.mouseInterceptor = mouseInterceptor;
			this.nativeMethods = nativeMethods;
		}

		public void Perform()
		{
			logger.Info("Starting keyboard and mouse interception...");

			nativeMethods.RegisterKeyboardHook(keyboardInterceptor);
			nativeMethods.RegisterMouseHook(mouseInterceptor);
		}

		public void Revert()
		{
			logger.Info("Stopping keyboard and mouse interception...");

			nativeMethods.DeregisterMouseHook(mouseInterceptor);
			nativeMethods.DeregisterKeyboardHook(keyboardInterceptor);
		}
	}
}
