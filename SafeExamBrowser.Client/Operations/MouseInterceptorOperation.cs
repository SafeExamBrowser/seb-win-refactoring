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
using SafeExamBrowser.Monitoring.Contracts.Mouse;

namespace SafeExamBrowser.Client.Operations
{
	internal class MouseInterceptorOperation : IOperation
	{
		private ILogger logger;
		private IMouseInterceptor mouseInterceptor;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public MouseInterceptorOperation(ILogger logger, IMouseInterceptor mouseInterceptor)
		{
			this.logger = logger;
			this.mouseInterceptor = mouseInterceptor;
		}

		public OperationResult Perform()
		{
			logger.Info("Starting mouse interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StartMouseInterception);

			mouseInterceptor.Start();

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Stopping mouse interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopMouseInterception);

			mouseInterceptor.Stop();

			return OperationResult.Success;
		}
	}
}
