/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IRuntimeProxy"/>, to be used for communication with the runtime application component.
	/// </summary>
	public class RuntimeProxy : BaseProxy, IRuntimeProxy
	{
		public RuntimeProxy(string address, IProxyObjectFactory factory, ILogger logger, Interlocutor owner) : base(address, factory, logger, owner)
		{
		}

		public CommunicationResult<ConfigurationResponse> GetConfiguration()
		{
			try
			{
				var response = Send(SimpleMessagePurport.ConfigurationNeeded);
				var success = response is ConfigurationResponse;

				if (success)
				{
					Logger.Debug("Received configuration response.");
				}
				else
				{
					Logger.Error($"Did not retrieve configuration response! Received: {ToString(response)}.");
				}

				return new CommunicationResult<ConfigurationResponse>(success, response as ConfigurationResponse);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(GetConfiguration)}'", e);

				return new CommunicationResult<ConfigurationResponse>(false, default(ConfigurationResponse));
			}
		}

		public CommunicationResult InformClientReady()
		{
			try
			{
				var response = Send(SimpleMessagePurport.ClientIsReady);
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Runtime acknowledged that the client is ready.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge that the client is ready! Response: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(InformClientReady)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult RequestReconfiguration(string filePath, string url)
		{
			try
			{
				var response = Send(new ReconfigurationMessage(filePath, url));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Runtime acknowledged reconfiguration request.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge reconfiguration request! Response: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RequestReconfiguration)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult RequestShutdown()
		{
			try
			{
				var response = Send(SimpleMessagePurport.RequestShutdown);
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Runtime acknowledged shutdown request.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge shutdown request! Response: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RequestShutdown)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult SubmitExamSelectionResult(Guid requestId, bool success, string selectedExamId = null)
		{
			try
			{
				var response = Send(new ExamSelectionReplyMessage(requestId, success, selectedExamId));
				var acknowledged = IsAcknowledged(response);

				if (acknowledged)
				{
					Logger.Debug("Runtime acknowledged server exam selection transmission.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge server exam selection transmission! Response: {ToString(response)}.");
				}

				return new CommunicationResult(acknowledged);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(SubmitExamSelectionResult)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult SubmitMessageBoxResult(Guid requestId, int result)
		{
			try
			{
				var response = Send(new MessageBoxReplyMessage(requestId, result));
				var acknowledged = IsAcknowledged(response);

				if (acknowledged)
				{
					Logger.Debug("Runtime acknowledged message box result transmission.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge message box result transmission! Response: {ToString(response)}.");
				}

				return new CommunicationResult(acknowledged);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(SubmitMessageBoxResult)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult SubmitPassword(Guid requestId, bool success, string password = null)
		{
			try
			{
				var response = Send(new PasswordReplyMessage(requestId, success, password));
				var acknowledged = IsAcknowledged(response);

				if (acknowledged)
				{
					Logger.Debug("Runtime acknowledged password transmission.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge password transmission! Response: {ToString(response)}.");
				}

				return new CommunicationResult(acknowledged);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(SubmitPassword)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult SubmitServerFailureActionResult(Guid requestId, bool abort, bool fallback, bool retry)
		{
			try
			{
				var response = Send(new ServerFailureActionReplyMessage(abort, fallback, retry, requestId));
				var acknowledged = IsAcknowledged(response);

				if (acknowledged)
				{
					Logger.Debug("Runtime acknowledged server failure action transmission.");
				}
				else
				{
					Logger.Error($"Runtime did not acknowledge server failure action transmission! Response: {ToString(response)}.");
				}

				return new CommunicationResult(acknowledged);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(SubmitServerFailureActionResult)}'", e);

				return new CommunicationResult(false);
			}
		}
	}
}
