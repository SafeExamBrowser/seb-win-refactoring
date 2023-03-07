/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Communication.Contracts.Data;

namespace SafeExamBrowser.Communication.Contracts.Proxies
{
	/// <summary>
	/// Defines the functionality for a proxy to the communication host of the client application component.
	/// </summary>
	public interface IClientProxy : ICommunicationProxy
	{
		/// <summary>
		/// Informs the client that a reconfiguration was aborted.
		/// </summary>
		CommunicationResult InformReconfigurationAborted();

		/// <summary>
		/// Informs the client that the reconfiguration request for the specified file was denied.
		/// </summary>
		CommunicationResult InformReconfigurationDenied(string filePath);

		/// <summary>
		/// Instructs the client to initiate its shutdown procedure.
		/// </summary>
		CommunicationResult InitiateShutdown();

		/// <summary>
		/// Instructs the client to submit its authentication data.
		/// </summary>
		CommunicationResult<AuthenticationResponse> RequestAuthentication();

		/// <summary>
		/// Requests the client to render a server exam selection dialog and subsequently return the interaction result as separate message.
		/// </summary>
		CommunicationResult RequestExamSelection(IEnumerable<(string id, string lms, string name, string url)> exams, Guid requestId);

		/// <summary>
		/// Requests the client to render a password dialog and subsequently return the interaction result as separate message.
		/// </summary>
		CommunicationResult RequestPassword(PasswordRequestPurpose purpose, Guid requestId);

		/// <summary>
		/// Requests the client to render a server failure action dialog and subsequently return the interaction result as separate message.
		/// </summary>
		CommunicationResult RequestServerFailureAction(string message, bool showFallback, Guid requestId);

		/// <summary>
		/// Requests the client to render a message box and subsequently return the interaction result as separate message.
		/// </summary>
		CommunicationResult ShowMessage(string message, string title, int action, int icon, Guid requestId);
	}
}
