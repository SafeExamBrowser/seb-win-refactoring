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
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class DisclaimerOperation : SessionOperation
	{
		public override event StatusChangedEventHandler StatusChanged;

		public DisclaimerOperation(Dependencies dependencies) : base(dependencies)
		{
		}

		public override OperationResult Perform()
		{
			var result = OperationResult.Success;

			// if (Context.Next.Settings.Proctoring.ScreenProctoring.Enabled)
			// {
			// 	result = ShowScreenProctoringDisclaimer();
			// }

			return result;
		}

		public override OperationResult Repeat()
		{
			var result = OperationResult.Success;

			// if (Context.Next.Settings.Proctoring.ScreenProctoring.Enabled)
			// {
			// 	result = ShowScreenProctoringDisclaimer();
			// }

			return result;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult ShowScreenProctoringDisclaimer()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitDisclaimerConfirmation);

			var message = TextKey.MessageBox_ScreenProctoringDisclaimer;
			var title = TextKey.MessageBox_ScreenProctoringDisclaimerTitle;
			var result = ShowMessageBox(message, title, action: MessageBoxAction.OkCancel);
			var operationResult = result == MessageBoxResult.Ok ? OperationResult.Success : OperationResult.Aborted;

			if (result == MessageBoxResult.Ok)
			{
				Logger.Info("The user confirmed the screen proctoring disclaimer.");
			}
			else
			{
				Logger.Warn("The user did not confirm the screen proctoring disclaimer! Aborting session initialization...");
			}

			return operationResult;
		}
	}
}
