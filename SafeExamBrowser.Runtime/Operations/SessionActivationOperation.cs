/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class SessionActivationOperation : SessionOperation
	{
		private ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public SessionActivationOperation(ILogger logger, SessionContext sessionContext) : base(sessionContext)
		{
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			ActivateNewSession();

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			ActivateNewSession();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private void ActivateNewSession()
		{
			var isFirstSession = Context.Current == null;

			if (isFirstSession)
			{
				logger.Info($"Successfully activated first session '{Context.Next.Id}'.");
			}
			else
			{
				logger.Info($"Successfully terminated old session '{Context.Current.Id}' and activated new session '{Context.Next.Id}'.");
			}

			Context.Current = Context.Next;
			Context.Next = null;
		}
	}
}
