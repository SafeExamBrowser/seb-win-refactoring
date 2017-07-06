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
using System.Threading;
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
			var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt";

			if (!Directory.Exists(settings.LogFolderPath))
			{
				Directory.CreateDirectory(settings.LogFolderPath);
			}

			filePath = Path.Combine(settings.LogFolderPath, fileName);
		}

		public void Notify(ILogMessage message)
		{
			lock (@lock)
			{
				using (var stream = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					var date = message.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
					var threadId = Thread.CurrentThread.ManagedThreadId;
					var severity = message.Severity.ToString().ToUpper();

					stream.WriteLine($"{date} [{threadId}] - {severity}: {message.Message}");
				}
			}
		}
	}
}
