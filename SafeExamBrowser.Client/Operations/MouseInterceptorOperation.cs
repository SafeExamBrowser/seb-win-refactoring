/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Client.Operations
{
	internal class MouseInterceptorOperation : IOperation
	{
		private ILogger logger;
		private IMouseInterceptor mouseInterceptor;
		private INativeMethods nativeMethods;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public MouseInterceptorOperation(
			ILogger logger,
			IMouseInterceptor mouseInterceptor,
			INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.mouseInterceptor = mouseInterceptor;
			this.nativeMethods = nativeMethods;
		}

		public OperationResult Perform()
		{
			logger.Info("Starting mouse interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StartMouseInterception);

			nativeMethods.RegisterMouseHook(mouseInterceptor);

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			throw new InvalidOperationException($"The '{nameof(MouseInterceptorOperation)}' is not meant to be repeated!");
		}

		public void Revert()
		{
			logger.Info("Stopping mouse interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopMouseInterception);

			nativeMethods.DeregisterMouseHook(mouseInterceptor);
		}
	}
}
