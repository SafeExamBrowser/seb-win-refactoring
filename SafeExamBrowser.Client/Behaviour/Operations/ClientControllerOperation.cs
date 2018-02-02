/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class ClientControllerOperation : IOperation
	{
		private ILogger logger;
		private IClientController controller;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public ClientControllerOperation(IClientController controller, ILogger logger)
		{
			this.controller = controller;
			this.logger = logger;
		}

		public void Perform()
		{
			logger.Info("Starting event handling...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StartEventHandling);

			controller.Start();
		}

		public void Repeat()
		{
			// Nothing to do here...
		}

		public void Revert()
		{
			logger.Info("Stopping event handling...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopEventHandling);

			controller.Stop();
		}
	}
}
