/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Logging
{
	/// <summary>
	/// Default implementation of <see cref="ILogContentFormatter"/>.
	/// </summary>
	public class DefaultLogFormatter : ILogContentFormatter
	{
		public string Format(ILogContent content)
		{
			if (content is ILogText text)
			{
				return text.Text;
			}

			if (content is ILogMessage message)
			{
				return FormatLogMessage(message);
			}

			throw new NotImplementedException($"The default formatter is not yet implemented for log content of type {content.GetType()}!");
		}

		private string FormatLogMessage(ILogMessage message)
		{
			var date = message.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
			var severity = message.Severity.ToString().ToUpper();
			var threadId = message.ThreadInfo.Id < 10 ? $"0{message.ThreadInfo.Id}" : message.ThreadInfo.Id.ToString();
			var threadName = message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty;
			var threadInfo = $"[{threadId}{threadName}]";

			return $"{date} {threadInfo} - {severity}: {message.Message}";
		}
	}
}
