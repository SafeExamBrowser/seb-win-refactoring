/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Core.Operations
{
	/// <summary>
	/// An operation to handle the lifetime of an <see cref="ICommunicationHost"/>. The host is started during <see cref="Perform"/>,
	/// stopped and restarted during <see cref="Repeat"/> (if not running) and stopped during <see cref="Revert"/>.
	/// </summary>
	public class CommunicationHostOperation : IRepeatableOperation
	{
		private ICommunicationHost host;
		private ILogger logger;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public CommunicationHostOperation(ICommunicationHost host, ILogger logger)
		{
			this.host = host;
			this.logger = logger;
		}

		public OperationResult Perform()
		{
			logger.Info("Starting communication host...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StartCommunicationHost);

			host.Start();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			if (!host.IsRunning)
			{
				logger.Info("Restarting communication host...");
				StatusChanged?.Invoke(TextKey.OperationStatus_RestartCommunicationHost);

				host.Stop();
				host.Start();
			}

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Stopping communication host...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopCommunicationHost);

			host.Stop();

			return OperationResult.Success;
		}
	}
}
