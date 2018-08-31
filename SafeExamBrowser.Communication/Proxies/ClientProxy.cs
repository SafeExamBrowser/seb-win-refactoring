/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IClientProxy"/>, to be used for communication with the client application component.
	/// </summary>
	public class ClientProxy : BaseProxy, IClientProxy
	{
		public ClientProxy(string address, IProxyObjectFactory factory, ILogger logger) : base(address, factory, logger)
		{
		}

		public CommunicationResult InformReconfigurationDenied(string filePath)
		{
			try
			{
				var response = Send(new ReconfigurationDeniedMessage(filePath));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged reconfiguration denial.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge reconfiguration denial! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(InformReconfigurationDenied)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult InitiateShutdown()
		{
			try
			{
				var response = Send(SimpleMessagePurport.Shutdown);
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged shutdown request.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge shutdown request! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(InitiateShutdown)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult<AuthenticationResponse> RequestAuthentication()
		{
			try
			{
				var response = Send(SimpleMessagePurport.Authenticate);
				var success = response is AuthenticationResponse;

				if (success)
				{
					Logger.Debug("Received authentication response.");
				}
				else
				{
					Logger.Error($"Did not receive authentication response! Received: {ToString(response)}.");
				}

				return new CommunicationResult<AuthenticationResponse>(success, response as AuthenticationResponse);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RequestAuthentication)}'", e);

				return new CommunicationResult<AuthenticationResponse>(false, default(AuthenticationResponse));
			}
		}

		public CommunicationResult RequestPassword(PasswordRequestPurpose purpose, Guid requestId)
		{
			try
			{
				var response = Send(new PasswordRequestMessage(purpose, requestId));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged password request.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge password request! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RequestPassword)}'", e);

				return new CommunicationResult(false);
			}
		}
	}
}
