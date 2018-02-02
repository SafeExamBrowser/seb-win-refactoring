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
	internal class KeyboardInterceptorOperation : IOperation
	{
		private IKeyboardInterceptor keyboardInterceptor;
		private ILogger logger;
		private INativeMethods nativeMethods;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }
		
		public KeyboardInterceptorOperation(
			IKeyboardInterceptor keyboardInterceptor,
			ILogger logger,
			INativeMethods nativeMethods)
		{
			this.keyboardInterceptor = keyboardInterceptor;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public void Perform()
		{
			logger.Info("Starting keyboard interception...");
			ProgressIndicator?.UpdateText(TextKey.SplashScreen_StartKeyboardInterception);

			nativeMethods.RegisterKeyboardHook(keyboardInterceptor);
		}

		public void Repeat()
		{
			// Nothing to do here...
		}

		public void Revert()
		{
			logger.Info("Stopping keyboard interception...");
			ProgressIndicator?.UpdateText(TextKey.SplashScreen_StopKeyboardInterception);

			nativeMethods.DeregisterKeyboardHook(keyboardInterceptor);
		}
	}
}
