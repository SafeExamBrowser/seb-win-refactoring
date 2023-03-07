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
using SafeExamBrowser.Monitoring.Contracts.Mouse;

namespace SafeExamBrowser.Client.Operations
{
	internal class MouseInterceptorOperation : ClientOperation
	{
		private ILogger logger;
		private IMouseInterceptor mouseInterceptor;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public MouseInterceptorOperation(ClientContext context, ILogger logger, IMouseInterceptor mouseInterceptor) : base(context)
		{
			this.logger = logger;
			this.mouseInterceptor = mouseInterceptor;
		}

		public override OperationResult Perform()
		{
			logger.Info("Starting mouse interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StartMouseInterception);

			mouseInterceptor.Start();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info("Stopping mouse interception...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopMouseInterception);

			mouseInterceptor.Stop();

			return OperationResult.Success;
		}
	}
}
