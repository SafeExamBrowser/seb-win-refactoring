/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Keyboard;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class KeyboardInterceptorOperation : IOperation
	{
		private IKeyboardInterceptor keyboardInterceptor;
		private ILogger logger;
		private INativeMethods nativeMethods;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public KeyboardInterceptorOperation(
			IKeyboardInterceptor keyboardInterceptor,
			ILogger logger,
			INativeMethods nativeMethods)
		{
			this.keyboardInterceptor = keyboardInterceptor;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public OperationResult Perform()
		{
			logger.Info("Starting keyboard interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StartKeyboardInterception);

			nativeMethods.RegisterKeyboardHook(keyboardInterceptor);

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Stopping keyboard interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopKeyboardInterception);

			nativeMethods.DeregisterKeyboardHook(keyboardInterceptor);

			return OperationResult.Success;
		}
	}
}
