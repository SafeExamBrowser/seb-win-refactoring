/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Contracts
{
	/// <summary>
	/// Defines the communication options with a server.
	/// </summary>
	public interface IServerProxy
	{
		/// <summary>
		/// TODO: Return API as well or re-load in proxy instance of client?
		/// Attempts to initialize a connection to the server. If successful, returns a OAuth2 token as response value.
		/// </summary>
		ServerResponse<string> Connect();

		/// <summary>
		/// 
		/// </summary>
		ServerResponse Disconnect();

		/// <summary>
		/// 
		/// </summary>
		ServerResponse<IEnumerable<Exam>> GetAvailableExams();

		/// <summary>
		/// 
		/// </summary>
		ServerResponse<Uri> GetConfigurationFor(Exam exam);

		/// <summary>
		/// Initializes the server settings to be used for communication.
		/// </summary>
		void Initialize(ServerSettings settings);

		/// <summary>
		/// 
		/// </summary>
		ServerResponse SendSessionInfo(string sessionId);
	}
}
