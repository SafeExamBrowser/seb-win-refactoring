/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.Logging
{
	/// <summary>
	/// Default implementation of <see cref="ILogMessage"/>.
	/// </summary>
	public class LogMessage : ILogMessage
	{
		public DateTime DateTime { get; private set; }
		public LogLevel Severity { get; private set; }
		public string Message { get; private set; }
		public IThreadInfo ThreadInfo { get; private set; }

		public LogMessage(DateTime dateTime, LogLevel severity, string message, IThreadInfo threadInfo)
		{
			DateTime = dateTime;
			Severity = severity;
			Message = message;
			ThreadInfo = threadInfo ?? throw new ArgumentNullException(nameof(threadInfo));
		}

		public object Clone()
		{
			return new LogMessage(DateTime, Severity, Message, ThreadInfo.Clone() as IThreadInfo);
		}
	}
}
