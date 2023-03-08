/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
		/// Event fired when the proxy receives a confirmation for a raise hand notification.
		/// </summary>
		event ServerEventHandler HandConfirmed;

		/// <summary>
		/// Event fired when the proxy receives a confirmation for a lock screen notification.
		/// </summary>
		event ServerEventHandler LockScreenConfirmed;

		/// <summary>
		/// Event fired when the proxy receives a lock screen instruction.
		/// </summary>
		event LockScreenRequestedEventHandler LockScreenRequested;

		/// <summary>
		/// Event fired when the proxy receives new proctoring configuration values.
		/// </summary>
		event ProctoringConfigurationReceivedEventHandler ProctoringConfigurationReceived;

		/// <summary>
		/// Event fired when the proxy receives a proctoring instruction.
		/// </summary>
		event ProctoringInstructionReceivedEventHandler ProctoringInstructionReceived;

		/// <summary>
		/// Event fired when the proxy detects an instruction to terminate SEB.
		/// </summary>
		event TerminationRequestedEventHandler TerminationRequested;

		/// <summary>
		/// Sends a lock screen confirm notification to the server.
		/// </summary>
		ServerResponse ConfirmLockScreen();

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
		ServerResponse<IEnumerable<Exam>> GetAvailableExams(string examId = default);

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
		/// Sends a lock screen notification to the server.
		/// </summary>
		ServerResponse LockScreen(string message = default);

		/// <summary>
		/// Sends a lower hand notification to the server.
		/// </summary>
		ServerResponse LowerHand();

		/// <summary>
		/// Sends a raise hand notification to the server.
		/// </summary>
		ServerResponse RaiseHand(string message = default);

		/// <summary>
		/// Sends the selected exam to the server. Optionally returns a custom browser exam key to be used for the active session.
		/// </summary>
		ServerResponse<string> SendSelectedExam(Exam exam);

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
