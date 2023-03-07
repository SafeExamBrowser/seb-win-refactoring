/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Text;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Logging
{
	/// <summary>
	/// <see cref="ILogObserver"/> which immediately saves new log content to disk.
	/// </summary>
	public class LogFileWriter : ILogObserver
	{
		private readonly object @lock = new object();
		private readonly string filePath;
		private readonly ILogContentFormatter formatter;

		public LogFileWriter(ILogContentFormatter formatter, string filePath)
		{
			this.filePath = filePath;
			this.formatter = formatter;
		}

		public void Initialize()
		{
			var directory = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
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
