/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Client.Operations
{
	internal class ClipboardOperation : IOperation
	{
		private ILogger logger;
		private INativeMethods nativeMethods;

		public IProgressIndicator ProgressIndicator { private get; set; }

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

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			EmptyClipboard();
		}

		private void EmptyClipboard()
		{
			logger.Info("Emptying clipboard...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_EmptyClipboard);

			nativeMethods.EmptyClipboard();
		}
	}
}
