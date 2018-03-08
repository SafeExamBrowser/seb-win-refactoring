/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class SessionSequenceEndOperation : IOperation
	{
		private SessionController controller;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public SessionSequenceEndOperation(SessionController controller)
		{
			this.controller = controller;
		}

		public OperationResult Perform()
		{
			controller.ProgressIndicator = ProgressIndicator;

			return controller.StartSession();
		}

		public OperationResult Repeat()
		{
			controller.ProgressIndicator = ProgressIndicator;

			return controller.StartSession();
		}

		public void Revert()
		{
			controller.ProgressIndicator = ProgressIndicator;
			controller.StopSession();
		}
	}
}
