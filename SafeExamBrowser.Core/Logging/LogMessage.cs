/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Entities
{
	public class LogMessage : ILogMessage
	{
		public DateTime DateTime { get; private set; }
		public LogLevel Severity { get; private set; }
		public string Message { get; private set; }
		public int ThreadId { get; private set; }

		public LogMessage(DateTime dateTime, LogLevel severity, int threadId, string message)
		{
			DateTime = dateTime;
			Severity = severity;
			Message = message;
			ThreadId = threadId;
		}

		public object Clone()
		{
			return new LogMessage(DateTime, Severity, ThreadId, Message);
		}
	}
}
