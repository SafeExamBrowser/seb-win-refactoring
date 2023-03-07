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
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class RemoteSessionOperation : SessionOperation
	{
		private readonly IRemoteSessionDetector detector;
		private readonly ILogger logger;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public RemoteSessionOperation(IRemoteSessionDetector detector, ILogger logger, SessionContext context) : base(context)
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
			logger.Info($"Validating remote session policy...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateRemoteSessionPolicy);

			if (Context.Next.Settings.Service.DisableRemoteConnections && detector.IsRemoteSession())
			{
				var args = new MessageEventArgs
				{
					Icon = MessageBoxIcon.Error,
					Message = TextKey.MessageBox_RemoteSessionNotAllowed,
					Title = TextKey.MessageBox_RemoteSessionNotAllowedTitle
				};

				logger.Error("Detected remote session while SEB is not allowed to be run in a remote session! Aborting...");
				ActionRequired?.Invoke(args);

				return OperationResult.Aborted;
			}

			return OperationResult.Success;
		}
	}
}
