/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.Core.OperationModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Operations
{
	internal class ClientOperationSequence : OperationSequence<IOperation>
	{
		private readonly ISplashScreen splashScreen;

		public ClientOperationSequence(ILogger logger, IEnumerable<IOperation> operations, ISplashScreen splashScreen) : base(logger, operations)
		{
			this.splashScreen = splashScreen;

			ProgressChanged += Operations_ProgressChanged;
			StatusChanged += Operations_StatusChanged;
		}

		private void Operations_ProgressChanged(ProgressChangedEventArgs args)
		{
			if (args.CurrentValue.HasValue)
			{
				splashScreen.SetValue(args.CurrentValue.Value);
			}

			if (args.IsIndeterminate == true)
			{
				splashScreen.SetIndeterminate();
			}

			if (args.MaxValue.HasValue)
			{
				splashScreen.SetMaxValue(args.MaxValue.Value);
			}

			if (args.Progress == true)
			{
				splashScreen.Progress();
			}

			if (args.Regress == true)
			{
				splashScreen.Regress();
			}
		}

		private void Operations_StatusChanged(TextKey status)
		{
			splashScreen.UpdateStatus(status, true);
		}
	}
}
