/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class WindowGuardOperation : ClientOperation
	{
		private readonly ILogger logger;
		private readonly IWindowGuard guard;

		public WindowGuardOperation(ClientContext context, ILogger logger, IWindowGuard guard) : base(context)
		{
			this.logger = logger;
			this.guard = guard;
		}

		public override event StatusChangedEventHandler StatusChanged;

		public override OperationResult Perform()
		{
			logger.Info("Initializing window guard...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeWindowGuard);

			if (Context.Settings.Proctoring.Enabled || Context.Settings.Security.AllowWindowCapture)
			{
				guard.Deactivate();
				logger.Info($"Deactivated window guard because {(Context.Settings.Proctoring.Enabled ? "proctoring" : "window capturing")} is enabled.");
			}
			else
			{
				guard.Activate();
				logger.Info("Activated window guard.");
			}

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
