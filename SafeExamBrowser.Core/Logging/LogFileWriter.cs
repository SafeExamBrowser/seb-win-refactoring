/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Text;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Logging
{
	public class LogFileWriter : ILogObserver
	{
		private static readonly object @lock = new object();
		private readonly string filePath;
		private readonly ILogContentFormatter formatter;

		public LogFileWriter(ILogContentFormatter formatter, ISettings settings)
		{
			if (!Directory.Exists(settings.LogFolderPath))
			{
				Directory.CreateDirectory(settings.LogFolderPath);
			}

			this.filePath = settings.ApplicationLogFile;
			this.formatter = formatter;
		}

		public void Notify(ILogContent content)
		{
			lock (@lock)
			{
				var raw = formatter.Format(content);

				using (var stream = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					stream.WriteLine(raw);
				}
			}
		}
	}
}
