/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class SessionInitializationOperation : IOperation
	{
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IRuntimeHost runtimeHost;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public SessionInitializationOperation(IConfigurationRepository configuration, ILogger logger, IRuntimeHost runtimeHost)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.runtimeHost = runtimeHost;
		}

		public OperationResult Perform()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public void Revert()
		{
			// Nothing to do here...
		}

		private void InitializeSessionConfiguration()
		{
			logger.Info("Initializing new session configuration...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeSession, true);

			configuration.InitializeSessionConfiguration();
			runtimeHost.StartupToken = configuration.CurrentSession.StartupToken;

			logger.Info($" -> Client-ID: {configuration.RuntimeInfo.ClientId}");
			logger.Info($" -> Runtime-ID: {configuration.RuntimeInfo.RuntimeId} (as reference, does not change)");
			logger.Info($" -> Session-ID: {configuration.CurrentSession.Id}");
		}
	}
}
