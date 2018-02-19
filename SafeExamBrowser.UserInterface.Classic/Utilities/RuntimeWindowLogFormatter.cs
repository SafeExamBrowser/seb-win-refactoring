/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.UserInterface.Classic.Utilities
{
	internal class RuntimeWindowLogFormatter : ILogContentFormatter
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

			throw new NotImplementedException($"The runtime window formatter is not yet implemented for log content of type {content.GetType()}!");
		}

		private string FormatLogMessage(ILogMessage message)
		{
			var time = message.DateTime.ToString("HH:mm:ss.fff");
			var severity = message.Severity.ToString().ToUpper();

			return $"{time} - {severity}: {message.Message}";
		}
	}
}
