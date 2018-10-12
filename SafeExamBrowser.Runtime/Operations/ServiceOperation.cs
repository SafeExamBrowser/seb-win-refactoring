/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ServiceOperation : SessionOperation
	{
		private bool connected, mandatory;
		private ILogger logger;
		private IServiceProxy service;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ServiceOperation(ILogger logger, IServiceProxy service, SessionContext sessionContext) : base(sessionContext)
		{
			this.logger = logger;
			this.service = service;
		}

		public override OperationResult Perform()
		{
			logger.Info($"Initializing service session...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			mandatory = Context.Next.Settings.ServicePolicy == ServicePolicy.Mandatory;
			connected = service.Connect();

			if (mandatory && !connected)
			{
				logger.Error("Failed to initialize a service session since the service is mandatory but not available!");

				return OperationResult.Failed;
			}

			service.Ignore = !connected;
			logger.Info($"The service is {(mandatory ? "mandatory" : "optional")} and {(connected ? "available." : "not available. All service-related operations will be ignored!")}");

			if (connected)
			{
				StartServiceSession();
			}

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			var result = Revert();

			if (result != OperationResult.Success)
			{
				return result;
			}

			return Perform();
		}

		public override OperationResult Revert()
		{
			logger.Info("Finalizing service session...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServiceSession);

			if (connected)
			{
				StopServiceSession();

				var success = service.Disconnect();

				if (success)
				{
					logger.Info("Successfully disconnected from the service.");
				}
				else
				{
					logger.Error("Failed to disconnect from the service!");
				}
			}

			return OperationResult.Success;
		}

		private void StartServiceSession()
		{
			service.StartSession(Context.Next.Id, Context.Next.Settings);
		}

		private void StopServiceSession()
		{
			service.StopSession(Context.Current.Id);
		}
	}
}
