/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class ProctoringResponsibility : ClientResponsibility
	{
		private readonly IUserInterfaceFactory uiFactory;

		private IProctoringController Proctoring => Context.Proctoring;

		public ProctoringResponsibility(ClientContext context, ILogger logger, IUserInterfaceFactory uiFactory) : base(context, logger)
		{
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
				var dialog = uiFactory.CreateProctoringFinalizationDialog();
				var handler = new RemainingWorkUpdatedEventHandler((args) => dialog.Update(args));

				Task.Run(() =>
				{
					Proctoring.RemainingWorkUpdated += handler;
					Proctoring.ExecuteRemainingWork();
					Proctoring.RemainingWorkUpdated -= handler;
				});

				dialog.Show();
			}
		}
	}
}
