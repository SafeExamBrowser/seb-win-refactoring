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
using SafeExamBrowser.Monitoring.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class ClipboardOperation : ClientOperation
	{
		private readonly IClipboard clipboard;
		private readonly ILogger logger;

		public override event StatusChangedEventHandler StatusChanged;

		public ClipboardOperation(ClientContext context, IClipboard clipboard, ILogger logger) : base(context)
		{
			this.clipboard = clipboard;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			InitializeClipboard();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			FinalizeClipboard();

			return OperationResult.Success;
		}

		private void InitializeClipboard()
		{
			logger.Info("Initializing clipboard...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeClipboard);
			clipboard.Initialize(Context.Settings.Security.ClipboardPolicy);
		}

		private void FinalizeClipboard()
		{
			logger.Info("Finalizing clipboard...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeClipboard);
			clipboard.Terminate();
		}
	}
}
