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
	/// 
	/// </summary>
	public interface IServerProxy
	{
		/// <summary>
		/// 
		/// </summary>
		ServerResponse Connect(ServerSettings settings);

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
		/// 
		/// </summary>
		ServerResponse SendSessionInfo(string sessionId);
	}
}
