/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class ClipboardOperation : ClientOperation
	{
		private ILogger logger;
		private INativeMethods nativeMethods;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ClipboardOperation(ClientContext context, ILogger logger, INativeMethods nativeMethods) : base(context)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public override OperationResult Perform()
		{
			EmptyClipboard();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			EmptyClipboard();

			return OperationResult.Success;
		}

		private void EmptyClipboard()
		{
			logger.Info("Emptying clipboard...");
			StatusChanged?.Invoke(TextKey.OperationStatus_EmptyClipboard);

			nativeMethods.EmptyClipboard();
		}
	}
}
