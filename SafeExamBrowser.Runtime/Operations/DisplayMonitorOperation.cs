/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class DisplayMonitorOperation : SessionOperation
	{
		private readonly IDisplayMonitor displayMonitor;
		private readonly ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public DisplayMonitorOperation(IDisplayMonitor displayMonitor, ILogger logger, SessionContext context) : base(context)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			return CheckDisplayConfiguration();
		}

		public override OperationResult Repeat()
		{
			return CheckDisplayConfiguration();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult CheckDisplayConfiguration()
		{
			var args = new MessageEventArgs
			{
				Action = MessageBoxAction.Ok,
				Icon = MessageBoxIcon.Error,
				Message = TextKey.MessageBox_DisplayConfigurationError,
				Title = TextKey.MessageBox_DisplayConfigurationErrorTitle
			};
			var result = OperationResult.Aborted;

			logger.Info("Validating display configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateDisplayConfiguration);

			if (displayMonitor.IsAllowedConfiguration(Context.Next.Settings.Display))
			{
				logger.Info("Display configuration is allowed.");
				result = OperationResult.Success;
			}
			else
			{
				logger.Error("Display configuration is not allowed!");
				ActionRequired?.Invoke(args);
			}

			return result;
		}
	}
}
