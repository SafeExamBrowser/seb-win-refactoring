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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class SessionIntegrityOperation : SessionOperation
	{
		private readonly ILogger logger;
		private readonly ISystemSentinel sentinel;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public SessionIntegrityOperation(ILogger logger, ISystemSentinel sentinel, SessionContext context) : base(context)
		{
			this.logger = logger;
			this.sentinel = sentinel;
		}

		public override OperationResult Perform()
		{
			var success = true;

			StatusChanged?.Invoke(TextKey.OperationStatus_VerifySessionIntegrity);

			success &= InitializeStickyKeys();
			success &= VerifyCursors();
			success &= VerifyEaseOfAccess();

			LogResult(success);

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Repeat()
		{
			var success = true;

			StatusChanged?.Invoke(TextKey.OperationStatus_VerifySessionIntegrity);

			success &= InitializeStickyKeys();
			success &= VerifyCursors();
			success &= VerifyEaseOfAccess();

			LogResult(success);

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			FinalizeStickyKeys();

			return OperationResult.Success;
		}

		private void FinalizeStickyKeys()
		{
			sentinel.RevertStickyKeys();
		}

		private bool InitializeStickyKeys()
		{
			var success = true;

			sentinel.RevertStickyKeys();

			if (!Context.Next.Settings.Security.AllowStickyKeys)
			{
				success = sentinel.DisableStickyKeys();
			}

			return success;
		}

		private void LogResult(bool success)
		{
			if (success)
			{
				logger.Info("Successfully ensured session integrity.");
			}
			else
			{
				logger.Error("Failed to ensure session integrity! Aborting session initialization...");
			}
		}

		private bool VerifyCursors()
		{
			var success = true;

			if (Context.Next.Settings.Security.VerifyCursorConfiguration)
			{
				success = sentinel.VerifyCursors();
			}
			else
			{
				logger.Debug("Verification of cursor configuration is disabled.");
			}

			return success;
		}

		private bool VerifyEaseOfAccess()
		{
			var success = sentinel.VerifyEaseOfAccess();

			if (!success)
			{
				if (Context.Current?.Settings.Service.IgnoreService == false)
				{
					logger.Info($"Ease of access configuration is compromised but service was active in the current session.");
					success = true;
				}
				else if (!Context.Next.Settings.Service.IgnoreService)
				{
					logger.Info($"Ease of access configuration is compromised but service will be active in the next session.");
					success = true;
				}
			}

			return success;
		}
	}
}
