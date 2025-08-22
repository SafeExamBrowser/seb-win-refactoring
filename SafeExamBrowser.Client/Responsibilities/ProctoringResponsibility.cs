/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Proctoring.Events;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class ProctoringResponsibility : ClientResponsibility
	{
		private readonly IMessageBox messageBox;
		private readonly IUserInterfaceFactory uiFactory;

		private bool cancel;

		private IProctoringController Proctoring => Context.Proctoring;

		public ProctoringResponsibility(
			ClientContext context,
			ILogger logger,
			IMessageBox messageBox,
			IUserInterfaceFactory uiFactory) : base(context, logger)
		{
			this.messageBox = messageBox;
			this.uiFactory = uiFactory;
		}

		public override void Assume(ClientTask task)
		{
			if (task == ClientTask.PrepareShutdown_Wave1)
			{
				FinalizeProctoring();
			}
		}

		private void FinalizeProctoring()
		{
			if (Proctoring != default && Proctoring.HasRemainingWork())
			{
				var dialog = uiFactory.CreateProctoringFinalizationDialog(!Context.QuitPasswordValidated);
				var handler = new RemainingWorkUpdatedEventHandler((args) => Proctoring_RemainingWorkUpdated(dialog, args));

				dialog.CancellationRequested += new CancellationRequestedEventHandler(() => Dialog_CancellationRequested(dialog));

				Task.Run(() =>
				{
					Proctoring.RemainingWorkUpdated += handler;
					Proctoring.ExecuteRemainingWork();
					Proctoring.RemainingWorkUpdated -= handler;
				});

				dialog.Show();
			}
		}

		private void Dialog_CancellationRequested(IProctoringFinalizationDialog dialog)
		{
			var alreadyValidated = Context.QuitPasswordValidated;

			if (alreadyValidated || IsValidQuitPassword(dialog.QuitPassword))
			{
				cancel = true;
				Logger.Info($"The user {(alreadyValidated ? "already " : "")}entered the correct quit password, cancelling remaining work...");
			}
			else
			{
				cancel = false;
				Logger.Info("The user entered the wrong quit password, remaining work will continue.");
				messageBox.Show(TextKey.MessageBox_InvalidQuitPassword, TextKey.MessageBox_InvalidQuitPasswordTitle, icon: MessageBoxIcon.Warning, parent: dialog);
			}
		}

		private void Proctoring_RemainingWorkUpdated(IProctoringFinalizationDialog dialog, RemainingWorkUpdatedEventArgs args)
		{
			dialog.Update(args);
			args.CancellationRequested = cancel;
		}
	}
}
