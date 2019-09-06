/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Security.AccessControl;
using System.Threading;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ServiceOperation : SessionOperation
	{
		private ILogger logger;
		private IRuntimeHost runtimeHost;
		private IServiceProxy service;
		private int timeout_ms;
		private IUserInfo userInfo;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public ServiceOperation(
			ILogger logger,
			IRuntimeHost runtimeHost,
			IServiceProxy service,
			SessionContext sessionContext,
			int timeout_ms,
			IUserInfo userInfo) : base(sessionContext)
		{
			this.logger = logger;
			this.runtimeHost = runtimeHost;
			this.service = service;
			this.timeout_ms = timeout_ms;
			this.userInfo = userInfo;
		}

		public override OperationResult Perform()
		{
			logger.Info($"Initializing service...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			var success = TryEstablishConnection();

			if (service.IsConnected)
			{
				success = TryStartSession();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Repeat()
		{
			logger.Info($"Initializing service...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			var success = false;

			if (service.IsConnected)
			{
				success = TryStopSession();
			}
			else
			{
				success = TryEstablishConnection();
			}

			if (success && service.IsConnected)
			{
				success = TryStartSession();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			logger.Info("Finalizing service...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServiceSession);

			var success = true;

			if (service.IsConnected)
			{
				if (Context.Current != null)
				{
					success &= TryStopSession(true);
				}

				success &= TryTerminateConnection();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		private bool TryEstablishConnection()
		{
			var mandatory = Context.Next.Settings.Service.Policy == ServicePolicy.Mandatory;
			var warn = Context.Next.Settings.Service.Policy == ServicePolicy.Warn;
			var connected = service.Connect();
			var success = connected || !mandatory;

			if (success)
			{
				service.Ignore = !connected;
				logger.Info($"The service is {(mandatory ? "mandatory" : "optional")} and {(connected ? "connected." : "not connected. All service-related operations will be ignored!")}");

				if (!connected && warn)
				{
					ActionRequired?.Invoke(new MessageEventArgs
					{
						Icon = MessageBoxIcon.Warning,
						Message = TextKey.MessageBox_ServiceUnavailableWarning,
						Title = TextKey.MessageBox_ServiceUnavailableWarningTitle
					});
				}
			}
			else
			{
				logger.Error("The service is mandatory but no connection could be established!");
				ActionRequired?.Invoke(new MessageEventArgs
				{
					Icon = MessageBoxIcon.Error,
					Message = TextKey.MessageBox_ServiceUnavailableError,
					Title = TextKey.MessageBox_ServiceUnavailableErrorTitle
				});
			}

			return success;
		}

		private bool TryTerminateConnection()
		{
			var disconnected = service.Disconnect();

			if (disconnected)
			{
				logger.Info("Successfully disconnected from service.");
			}
			else
			{
				logger.Error("Failed to disconnect from service!");
			}

			return disconnected;
		}

		private bool TryStartSession()
		{
			var configuration = new ServiceConfiguration
			{
				AppConfig = Context.Next.AppConfig,
				SessionId = Context.Next.SessionId,
				Settings = Context.Next.Settings,
				UserName = userInfo.GetUserName(),
				UserSid = userInfo.GetUserSid()
			};
			var started = false;

			logger.Info("Starting new service session...");

			var communication = service.StartSession(configuration);

			if (communication.Success)
			{
				started = TryWaitForServiceEvent(Context.Next.AppConfig.ServiceEventName);

				if (started)
				{
					logger.Info("Successfully started new service session.");
				}
				else
				{
					logger.Error($"Failed to start new service session within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				logger.Error("Failed to communicate session start command to service!");
			}

			return started;
		}

		private bool TryStopSession(bool isFinalSession = false)
		{
			var success = false;

			logger.Info("Stopping current service session...");

			var communication = service.StopSession(Context.Current.SessionId);

			if (communication.Success)
			{
				success = TryWaitForServiceEvent(Context.Current.AppConfig.ServiceEventName);

				if (success)
				{
					logger.Info("Successfully stopped service session.");
				}
				else
				{
					logger.Error($"Failed to stop service session within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				logger.Error("Failed to communicate session stop command to service!");
			}

			if (success && isFinalSession)
			{
				communication = service.RunSystemConfigurationUpdate();
				success = communication.Success;

				if (communication.Success)
				{
					logger.Info("Instructed service to perform system configuration update.");
				}
				else
				{
					logger.Error("Failed to communicate system configuration update command to service!");
				}
			}

			return success;
		}

		private bool TryWaitForServiceEvent(string eventName)
		{
			var serviceEvent = default(EventWaitHandle);
			var startTime = DateTime.Now;

			do
			{
				if (EventWaitHandle.TryOpenExisting(eventName, EventWaitHandleRights.Synchronize, out serviceEvent))
				{
					break;
				}
			} while (startTime.AddMilliseconds(timeout_ms) > DateTime.Now);

			if (serviceEvent != default(EventWaitHandle))
			{
				using (serviceEvent)
				{
					return serviceEvent.WaitOne(timeout_ms);
				}
			}

			return false;
		}
	}
}
