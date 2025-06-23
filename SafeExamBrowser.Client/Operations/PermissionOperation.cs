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
using SafeExamBrowser.SystemComponents.Contracts.Network;

namespace SafeExamBrowser.Client.Operations
{
	internal class PermissionOperation : ClientOperation
	{
		private readonly ILogger logger;
		private readonly INetworkAdapter networkAdapter;

		public override event StatusChangedEventHandler StatusChanged;

		public PermissionOperation(ClientContext context, ILogger logger, INetworkAdapter networkAdapter) : base(context)
		{
			this.logger = logger;
			this.networkAdapter = networkAdapter;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing permissions...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializePermissions);

			RequestNetworkAdapterAccess();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private void RequestNetworkAdapterAccess()
		{
			var granted = networkAdapter.RequestAccess();

			if (granted)
			{
				logger.Info("Permission to access the wireless networking functionality has been granted.");
			}
			else
			{
				logger.Warn("Permission to access the wireless networking functionality has not been granted! " +
							"If required, please grant the location permission manually under 'Privacy & Security' in the system settings.");
			}
		}
	}
}
