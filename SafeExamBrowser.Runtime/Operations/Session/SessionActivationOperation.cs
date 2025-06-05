/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class SessionActivationOperation : SessionOperation
	{
		public override event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public SessionActivationOperation(Dependencies dependencies) : base(dependencies)
		{
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
			if (Logger.LogLevel != Context.Next.Settings.LogLevel)
			{
				var current = Logger.LogLevel.ToString().ToUpper();
				var next = Context.Next.Settings.LogLevel.ToString().ToUpper();

				Logger.Info($"Switching from log severity '{current}' to '{next}' for new session.");
				Logger.LogLevel = Context.Next.Settings.LogLevel;
			}
		}

		private void ActivateNewSession()
		{
			var isFirstSession = Context.Current == default;

			if (isFirstSession)
			{
				Logger.Info($"Successfully activated first session '{Context.Next.SessionId}'.");
			}
			else
			{
				Logger.Info($"Successfully terminated old session '{Context.Current.SessionId}' and activated new session '{Context.Next.SessionId}'.");
			}

			Context.Current = Context.Next;
			Context.Next = default;
		}
	}
}
