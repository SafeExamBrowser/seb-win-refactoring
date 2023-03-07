/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Logging.Contracts;

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
			SwitchLogSeverity();
			ActivateNewSession();

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			SwitchLogSeverity();
			ActivateNewSession();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private void SwitchLogSeverity()
		{
			if (logger.LogLevel != Context.Next.Settings.LogLevel)
			{
				var current = logger.LogLevel.ToString().ToUpper();
				var next = Context.Next.Settings.LogLevel.ToString().ToUpper();

				logger.Info($"Switching from log severity '{current}' to '{next}' for new session.");
				logger.LogLevel = Context.Next.Settings.LogLevel;
			}
		}

		private void ActivateNewSession()
		{
			var isFirstSession = Context.Current == null;

			if (isFirstSession)
			{
				logger.Info($"Successfully activated first session '{Context.Next.SessionId}'.");
			}
			else
			{
				logger.Info($"Successfully terminated old session '{Context.Current.SessionId}' and activated new session '{Context.Next.SessionId}'.");
			}

			Context.Current = Context.Next;
			Context.Next = null;
		}
	}
}
