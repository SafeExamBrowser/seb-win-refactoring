/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class ServiceOperation : SessionOperation
	{
		private readonly IRuntimeHost runtimeHost;
		private readonly IServiceProxy serviceProxy;
		private readonly int timeout_ms;
		private readonly IUserInfo userInfo;

		private string serviceEventName;
		private Guid? sessionId;

		public override event StatusChangedEventHandler StatusChanged;

		public ServiceOperation(
			Dependencies dependencies,
			IRuntimeHost runtimeHost,
			IServiceProxy serviceProxy,
			int timeout_ms,
			IUserInfo userInfo) : base(dependencies)
		{
			this.runtimeHost = runtimeHost;
			this.serviceProxy = serviceProxy;
			this.timeout_ms = timeout_ms;
			this.userInfo = userInfo;
		}

		public override OperationResult Perform()
		{
			Logger.Info($"Initializing service...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			var success = IgnoreService() || TryInitializeConnection();

			if (success && serviceProxy.IsConnected)
			{
				success = TryStartSession();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Repeat()
		{
			Logger.Info($"Initializing service...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			var success = true;

			if (serviceProxy.IsConnected)
			{
				if (sessionId.HasValue)
				{
					success = TryStopSession();
				}

				if (success && IgnoreService())
				{
					success = TryTerminateConnection();
				}
			}
			else
			{
				success = IgnoreService() || TryInitializeConnection();
			}

			if (success && serviceProxy.IsConnected)
			{
				success = TryStartSession();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			Logger.Info("Finalizing service...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServiceSession);

			var success = true;

			if (serviceProxy.IsConnected)
			{
				if (sessionId.HasValue)
				{
					success = TryStopSession(true);
				}

				success &= TryTerminateConnection();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		private bool IgnoreService()
		{
			if (Context.Next.Settings.Service.IgnoreService)
			{
				Logger.Info("The service will be ignored for the next session.");

				return true;
			}

			return false;
		}

		private bool TryInitializeConnection()
		{
			var mandatory = Context.Next.Settings.Service.Policy == ServicePolicy.Mandatory;
			var warn = Context.Next.Settings.Service.Policy == ServicePolicy.Warn;
			var connected = serviceProxy.Connect();
			var success = connected || !mandatory;

			if (success)
			{
				Logger.Info($"The service is {(mandatory ? "mandatory" : "optional")} and {(connected ? "connected." : "not connected.")}");

				if (!connected && warn)
				{
					ShowMessageBox(TextKey.MessageBox_ServiceUnavailableWarning, TextKey.MessageBox_ServiceUnavailableWarningTitle, icon: MessageBoxIcon.Warning);
				}
			}
			else
			{
				Logger.Error("The service is mandatory but no connection could be established!");
				ShowMessageBox(TextKey.MessageBox_ServiceUnavailableError, TextKey.MessageBox_ServiceUnavailableErrorTitle, icon: MessageBoxIcon.Error);
			}

			return success;
		}

		private bool TryTerminateConnection()
		{
			var disconnected = serviceProxy.Disconnect();

			if (disconnected)
			{
				Logger.Info("Successfully disconnected from service.");
			}
			else
			{
				Logger.Error("Failed to disconnect from service!");
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

			Logger.Info("Starting new service session...");

			var communication = serviceProxy.StartSession(configuration);

			if (communication.Success)
			{
				started = TryWaitForServiceEvent(Context.Next.AppConfig.ServiceEventName);

				if (started)
				{
					sessionId = Context.Next.SessionId;
					serviceEventName = Context.Next.AppConfig.ServiceEventName;
					Logger.Info("Successfully started new service session.");
				}
				else
				{
					Logger.Error($"Failed to start new service session within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				Logger.Error("Failed to communicate session start command to service!");
			}

			return started;
		}

		private bool TryStopSession(bool isFinalSession = false)
		{
			var success = false;

			Logger.Info("Stopping current service session...");

			var communication = serviceProxy.StopSession(sessionId.Value);

			if (communication.Success)
			{
				success = TryWaitForServiceEvent(serviceEventName);

				if (success)
				{
					sessionId = default;
					serviceEventName = default;
					Logger.Info("Successfully stopped service session.");
				}
				else
				{
					Logger.Error($"Failed to stop service session within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				Logger.Error("Failed to communicate session stop command to service!");
			}

			if (success && isFinalSession)
			{
				communication = serviceProxy.RunSystemConfigurationUpdate();
				success = communication.Success;

				if (communication.Success)
				{
					Logger.Info("Instructed service to perform system configuration update.");
				}
				else
				{
					Logger.Error("Failed to communicate system configuration update command to service!");
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
