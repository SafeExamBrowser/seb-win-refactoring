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
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class VirtualMachineOperation : SessionOperation
	{
		private readonly IVirtualMachineDetector detector;

		public override event StatusChangedEventHandler StatusChanged;

		public VirtualMachineOperation(Dependencies dependencies, IVirtualMachineDetector detector) : base(dependencies)
		{
			this.detector = detector;
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
			var result = OperationResult.Success;

			Logger.Info($"Validating virtual machine policy...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateVirtualMachinePolicy);

			if (Context.Next.Settings.Security.VirtualMachinePolicy == VirtualMachinePolicy.Deny && detector.IsVirtualMachine())
			{
				result = OperationResult.Aborted;
				Logger.Error("Detected virtual machine while SEB is not allowed to be run in a virtual machine! Aborting...");
				ShowMessageBox(TextKey.MessageBox_VirtualMachineNotAllowed, TextKey.MessageBox_VirtualMachineNotAllowedTitle, icon: MessageBoxIcon.Error);
			}

			return result;
		}
	}
}
