/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	/// <summary>
	/// An operation to handle the lifetime of an <see cref="ICommunicationHost"/>. The host is started during <see cref="Perform"/>,
	/// stopped and restarted during <see cref="Repeat"/> (if not running) and stopped during <see cref="Revert"/>.
	/// </summary>
	public class CommunicationOperation : IOperation
	{
		private ICommunicationHost host;
		private ILogger logger;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public CommunicationOperation(ICommunicationHost host, ILogger logger)
		{
			this.host = host;
			this.logger = logger;
		}

		public OperationResult Perform()
		{
			logger.Info("Starting communication host...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StartCommunicationHost);

			host.Start();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			if (!host.IsRunning)
			{
				logger.Info("Restarting communication host...");
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_RestartCommunicationHost);

				host.Stop();
				host.Start();
			}

			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Stopping communication host...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopCommunicationHost);

			host.Stop();
		}
	}
}
