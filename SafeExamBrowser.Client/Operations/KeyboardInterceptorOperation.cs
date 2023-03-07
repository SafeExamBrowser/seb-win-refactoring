/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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

namespace SafeExamBrowser.Client.Operations
{
	internal class KeyboardInterceptorOperation : ClientOperation
	{
		private IKeyboardInterceptor keyboardInterceptor;
		private ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public KeyboardInterceptorOperation(ClientContext context, IKeyboardInterceptor keyboardInterceptor, ILogger logger) : base(context)
		{
			this.keyboardInterceptor = keyboardInterceptor;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			logger.Info("Starting keyboard interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StartKeyboardInterception);

			keyboardInterceptor.Start();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info("Stopping keyboard interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopKeyboardInterception);

			keyboardInterceptor.Stop();

			return OperationResult.Success;
		}
	}
}
