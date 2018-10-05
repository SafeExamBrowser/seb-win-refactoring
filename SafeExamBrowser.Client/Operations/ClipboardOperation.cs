/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

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

		public OperationResult Repeat()
		{
			throw new InvalidOperationException($"The '{nameof(ClipboardOperation)}' is not meant to be repeated!");
		}

		public void Revert()
		{
			EmptyClipboard();
		}

		private void EmptyClipboard()
		{
			logger.Info("Emptying clipboard...");
			StatusChanged?.Invoke(TextKey.OperationStatus_EmptyClipboard);

			nativeMethods.EmptyClipboard();
		}
	}
}
