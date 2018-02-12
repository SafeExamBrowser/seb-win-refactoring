/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal abstract class SessionSequenceOperation : IOperation
	{
		private bool sessionRunning;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IServiceProxy serviceProxy;
		private ISessionData sessionData;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public SessionSequenceOperation(IConfigurationRepository configuration, ILogger logger, IServiceProxy serviceProxy)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.serviceProxy = serviceProxy;
		}

		public abstract void Perform();
		public abstract void Repeat();
		public abstract void Revert();

		protected void StartSession()
		{
			logger.Info("Starting new session...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StartSession, true);

			sessionData = configuration.InitializeSessionData();
			serviceProxy.StartSession(sessionData.Id, configuration.CurrentSettings);

			// TODO:
			// - Create and connect to client
			// - Verify session integrity and start event handling -> in runtime controller?
			System.Threading.Thread.Sleep(5000);

			sessionRunning = true;
			logger.Info($"Successfully started new session with identifier '{sessionData.Id}'.");
		}

		protected void StopSession()
		{
			if (sessionRunning)
			{
				logger.Info($"Stopping session with identifier '{sessionData.Id}'...");
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopSession, true);

				serviceProxy.StopSession(sessionData.Id);

				// TODO:
				// - Terminate client (or does it terminate itself?)
				// - Stop event handling and verify session termination -> in runtime controller?
				System.Threading.Thread.Sleep(5000);

				sessionRunning = false;
				logger.Info($"Successfully stopped session with identifier '{sessionData.Id}'.");
			}
		}
	}
}
