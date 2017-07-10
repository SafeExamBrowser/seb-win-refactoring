/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Text;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Logging
{
	public class LogFileWriter : ILogObserver
	{
		private static readonly object @lock = new object();
		private readonly string filePath;

		public LogFileWriter(ISettings settings)
		{
			var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd HH\\hmm\\mss\\s")}.txt";

			if (!Directory.Exists(settings.LogFolderPath))
			{
				Directory.CreateDirectory(settings.LogFolderPath);
			}

			filePath = Path.Combine(settings.LogFolderPath, fileName);
		}

		public void Notify(ILogContent content)
		{
			if (content is ILogText)
			{
				WriteLogText(content as ILogText);
			}

			if (content is ILogMessage)
			{
				WriteLogMessage(content as ILogMessage);
			}
		}

		private void WriteLogText(ILogText text)
		{
			Write(text.Text);
		}

		private void WriteLogMessage(ILogMessage message)
		{
			var date = message.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
			var severity = message.Severity.ToString().ToUpper();
			var threadInfo = $"{message.ThreadInfo.Id}{(message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty)}";

			Write($"{date} [{threadInfo}] - {severity}: {message.Message}");
		}

		private void Write(string content)
		{
			lock (@lock)
			{
				using (var stream = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					stream.WriteLine(content);
				}
			}
		}
	}
}
