/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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
		private readonly IText text;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public DisplayMonitorOperation(IDisplayMonitor displayMonitor, ILogger logger, SessionContext context, IText text) : base(context)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
			this.text = text;
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
			logger.Info("Validating display configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateDisplayConfiguration);

			var result = OperationResult.Failed;
			var validation = displayMonitor.ValidateConfiguration(Context.Next.Settings.Display);

			if (validation.IsAllowed)
			{
				logger.Info("Display configuration is allowed.");
				result = OperationResult.Success;
			}
			else
			{
				var args = new MessageEventArgs
				{
					Action = MessageBoxAction.Ok,
					Icon = MessageBoxIcon.Error,
					Message = TextKey.MessageBox_DisplayConfigurationError,
					Title = TextKey.MessageBox_DisplayConfigurationErrorTitle
				};

				logger.Error("Display configuration is not allowed!");

				args.MessagePlaceholders.Add("%%_ALLOWED_COUNT_%%", Convert.ToString(Context.Next.Settings.Display.AllowedDisplays));
				args.MessagePlaceholders.Add("%%_TYPE_%%", Context.Next.Settings.Display.InternalDisplayOnly ? text.Get(TextKey.MessageBox_DisplayConfigurationInternal) : text.Get(TextKey.MessageBox_DisplayConfigurationInternalOrExternal));
				args.MessagePlaceholders.Add("%%_EXTERNAL_COUNT_%%", Convert.ToString(validation.ExternalDisplays));
				args.MessagePlaceholders.Add("%%_INTERNAL_COUNT_%%", Convert.ToString(validation.InternalDisplays));

				ActionRequired?.Invoke(args);
			}

			return result;
		}
	}
}
