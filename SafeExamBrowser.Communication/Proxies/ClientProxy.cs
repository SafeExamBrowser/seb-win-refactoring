/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IClientProxy"/>, to be used for communication with the client application component.
	/// </summary>
	public class ClientProxy : BaseProxy, IClientProxy
	{
		public ClientProxy(string address, IProxyObjectFactory factory, ILogger logger, Interlocutor owner) : base(address, factory, logger, owner)
		{
		}

		public CommunicationResult InformReconfigurationAborted()
		{
			try
			{
				var response = Send(new SimpleMessage(SimpleMessagePurport.ReconfigurationAborted));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged reconfiguration abortion.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge reconfiguration abortion! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(InformReconfigurationAborted)}'", e);

				return new CommunicationResult(false);
			}
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

		public CommunicationResult RequestExamSelection(IEnumerable<(string id, string lms, string name, string url)> exams, Guid requestId)
		{
			try
			{
				var response = Send(new ExamSelectionRequestMessage(exams, requestId));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged server exam selection request.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge server exam selection request! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RequestExamSelection)}'", e);

				return new CommunicationResult(false);
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

		public CommunicationResult RequestServerFailureAction(string message, bool showFallback, Guid requestId)
		{
			try
			{
				var response = Send(new ServerFailureActionRequestMessage(message, showFallback, requestId));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged server failure action request.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge server failure action request! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RequestServerFailureAction)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult ShowMessage(string message, string title, int action, int icon, Guid requestId)
		{
			try
			{
				var response = Send(new MessageBoxRequestMessage(action, icon, message, requestId, title));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Client acknowledged message box request.");
				}
				else
				{
					Logger.Error($"Client did not acknowledge message box request! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(ShowMessage)}'", e);

				return new CommunicationResult(false);
			}
		}
	}
}
