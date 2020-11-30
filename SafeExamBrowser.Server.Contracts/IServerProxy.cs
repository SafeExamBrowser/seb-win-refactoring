/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Server.Contracts.Events;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Contracts
{
	/// <summary>
	/// Defines the communication options with a server.
	/// </summary>
	public interface IServerProxy
	{
		/// <summary>
		/// Event fired when the server detects an instruction to terminate SEB.
		/// </summary>
		event TerminationRequestedEventHandler TerminationRequested; 

		/// <summary>
		/// Attempts to initialize a connection with the server.
		/// </summary>
		ServerResponse Connect();

		/// <summary>
		/// Terminates a connection with the server.
		/// </summary>
		ServerResponse Disconnect();

		/// <summary>
		/// Retrieves a list of all currently available exams, or a list containing the specified exam.
		/// </summary>
		ServerResponse<IEnumerable<Exam>> GetAvailableExams(string examId = default(string));

		/// <summary>
		/// Retrieves the URI of the configuration file for the given exam.
		/// </summary>
		ServerResponse<Uri> GetConfigurationFor(Exam exam);

		/// <summary>
		/// Retrieves the information required to establish a connection with the server.
		/// </summary>
		ConnectionInfo GetConnectionInfo();

		/// <summary>
		/// Initializes the server settings to be used for communication.
		/// </summary>
		void Initialize(ServerSettings settings);

		/// <summary>
		/// Initializes the configuration and server settings to be used for communication.
		/// </summary>
		void Initialize(string api, string connectionToken, string examId, string oauth2Token, ServerSettings settings);

		/// <summary>
		/// Sends the given user session identifier of a LMS and thus establishes a connection with the server.
		/// </summary>
		ServerResponse SendSessionIdentifier(string identifier);

		/// <summary>
		/// Starts sending ping and log data to the server.
		/// </summary>
		void StartConnectivity();

		/// <summary>
		/// Stops sending ping and log data to the server.
		/// </summary>
		void StopConnectivity();
	}
}
