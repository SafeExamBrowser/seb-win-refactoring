/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server
{
	public class ServerProxy : IServerProxy
	{
		public ServerResponse Connect(ServerSettings settings)
		{
			throw new NotImplementedException();
		}

		public ServerResponse Disconnect()
		{
			throw new NotImplementedException();
		}

		public ServerResponse<IEnumerable<Exam>> GetAvailableExams()
		{
			throw new NotImplementedException();
		}

		public ServerResponse<Uri> GetConfigurationFor(Exam exam)
		{
			throw new NotImplementedException();
		}

		public ServerResponse SendSessionInfo(string sessionId)
		{
			throw new NotImplementedException();
		}
	}
}
