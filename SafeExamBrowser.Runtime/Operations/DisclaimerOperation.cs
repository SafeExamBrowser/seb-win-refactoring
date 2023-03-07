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
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class DisclaimerOperation : SessionOperation
	{
		private readonly ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public DisclaimerOperation(ILogger logger, SessionContext context) : base(context)
		{
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			if (Context.Next.Settings.Proctoring.Enabled)
			{
				return ShowDisclaimer();
			}

			logger.Info("Remote proctoring is disabled, skipping disclaimer.");

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			if (Context.Next.Settings.Proctoring.Enabled)
			{
				return ShowDisclaimer();
			}

			logger.Info("Remote proctoring is disabled, skipping disclaimer.");

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult ShowDisclaimer()
		{
			var args = new MessageEventArgs
			{
				Action = MessageBoxAction.OkCancel,
				Icon = MessageBoxIcon.Information,
				Message = TextKey.MessageBox_ProctoringDisclaimer,
				Title = TextKey.MessageBox_ProctoringDisclaimerTitle
			};

			StatusChanged?.Invoke(TextKey.OperationStatus_WaitDisclaimerConfirmation);
			ActionRequired?.Invoke(args);

			if (args.Result == MessageBoxResult.Ok)
			{
				logger.Info("The user confirmed the remote proctoring disclaimer.");

				return OperationResult.Success;
			}
			else
			{
				logger.Warn("The user did not confirm the remote proctoring disclaimer! Aborting session initialization...");

				return OperationResult.Aborted;
			}
		}
	}
}
