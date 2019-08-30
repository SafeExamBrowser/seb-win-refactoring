/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
	internal class ClipboardOperation : IOperation
	{
		private ILogger logger;
		private INativeMethods nativeMethods;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ClipboardOperation(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public OperationResult Perform()
		{
			EmptyClipboard();

			return OperationResult.Success;
		}

		public OperationResult Revert()
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
