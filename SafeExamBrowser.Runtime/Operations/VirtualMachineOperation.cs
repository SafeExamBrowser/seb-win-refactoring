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
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class VirtualMachineOperation : SessionOperation
	{
		private readonly IVirtualMachineDetector detector;
		private readonly ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public VirtualMachineOperation(IVirtualMachineDetector detector, ILogger logger, SessionContext context) : base(context)
		{
			this.detector = detector;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			return ValidatePolicy();
		}

		public override OperationResult Repeat()
		{
			return ValidatePolicy();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult ValidatePolicy()
		{
			logger.Info($"Validating virtual machine policy...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateVirtualMachinePolicy);

			if (Context.Next.Settings.Security.VirtualMachinePolicy == VirtualMachinePolicy.Deny && detector.IsVirtualMachine())
			{
				var args = new MessageEventArgs
				{
					Icon = MessageBoxIcon.Error,
					Message = TextKey.MessageBox_VirtualMachineNotAllowed,
					Title = TextKey.MessageBox_VirtualMachineNotAllowedTitle
				};

				logger.Error("Detected virtual machine while SEB is not allowed to be run in a virtual machine! Aborting...");
				ActionRequired?.Invoke(args);

				return OperationResult.Aborted;
			}

			return OperationResult.Success;
		}
	}
}
