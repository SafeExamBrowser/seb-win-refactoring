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
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class RemoteSessionOperation : SessionOperation
	{
		private readonly IRemoteSessionDetector detector;

		public override event StatusChangedEventHandler StatusChanged;

		public RemoteSessionOperation(Dependencies dependencies, IRemoteSessionDetector detector) : base(dependencies)
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

			Logger.Info($"Validating remote session policy...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateRemoteSessionPolicy);

			if (Context.Next.Settings.Service.DisableRemoteConnections && detector.IsRemoteSession())
			{
				result = OperationResult.Aborted;
				Logger.Error("Detected remote session while SEB is not allowed to be run in a remote session! Aborting...");
				ShowMessageBox(TextKey.MessageBox_RemoteSessionNotAllowed, TextKey.MessageBox_RemoteSessionNotAllowedTitle, icon: MessageBoxIcon.Error);
			}

			return result;
		}
	}
}
