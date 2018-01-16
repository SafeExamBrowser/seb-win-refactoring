/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Logging
{
	public class Logger : ILogger
	{
		private static readonly object @lock = new object();
		private readonly IList<ILogContent> log = new List<ILogContent>();
		private readonly IList<ILogObserver> observers = new List<ILogObserver>();

		public void Info(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			Add(LogLevel.Info, message);
		}

		public void Warn(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			Add(LogLevel.Warning, message);
		}

		public void Error(string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			Add(LogLevel.Error, message);
		}

		public void Error(string message, Exception exception)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			var details = new StringBuilder();

			details.AppendLine();
			details.AppendLine($"   Exception Message: {exception.Message}");
			details.AppendLine($"   Exception Type: {exception.GetType()}");
			details.AppendLine();
			details.AppendLine(exception.StackTrace);

			Add(LogLevel.Error, message);
			Add(new LogText(details.ToString()));
		}

		public void Log(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			Add(new LogText(text));
		}

		public void Log(ILogContent content)
		{
			if (content == null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			Add(content.Clone() as ILogContent);
		}

		public IList<ILogContent> GetLog()
		{
			lock (@lock)
			{
				return log.Select(m => m.Clone() as ILogContent).ToList();
			}
		}

		public void Subscribe(ILogObserver observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			lock (@lock)
			{
				if (!observers.Contains(observer))
				{
					observers.Add(observer);
				}
			}
		}

		public void Unsubscribe(ILogObserver observer)
		{
			lock (@lock)
			{
				observers.Remove(observer);
			}
		}

		private void Add(LogLevel severity, string message)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var threadName = Thread.CurrentThread.Name;
			var threadInfo = new ThreadInfo(threadId, threadName);

			Add(new LogMessage(DateTime.Now, severity, message, threadInfo));
		}

		private void Add(ILogContent content)
		{
			lock (@lock)
			{
				log.Add(content);

				foreach (var observer in observers)
				{
					observer.Notify(content);
				}
			}
		}
	}
}
