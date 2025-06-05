/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class SessionOperationSequence : RepeatableOperationSequence<SessionOperation>
	{
		private readonly IRuntimeWindow runtimeWindow;

		internal SessionOperationSequence(ILogger logger, IEnumerable<SessionOperation> operations, IRuntimeWindow runtimeWindow) : base(logger, operations)
		{
			this.runtimeWindow = runtimeWindow;

			ProgressChanged += SessionSequence_ProgressChanged;
			StatusChanged += SessionSequence_StatusChanged;
		}

		private void SessionSequence_ProgressChanged(ProgressChangedEventArgs args)
		{
			if (args.CurrentValue.HasValue)
			{
				runtimeWindow?.SetValue(args.CurrentValue.Value);
			}

			if (args.IsIndeterminate == true)
			{
				runtimeWindow?.SetIndeterminate();
			}

			if (args.MaxValue.HasValue)
			{
				runtimeWindow?.SetMaxValue(args.MaxValue.Value);
			}

			if (args.Progress == true)
			{
				runtimeWindow?.Progress();
			}

			if (args.Regress == true)
			{
				runtimeWindow?.Regress();
			}
		}

		private void SessionSequence_StatusChanged(TextKey status)
		{
			runtimeWindow?.UpdateStatus(status, true);
		}
	}
}
