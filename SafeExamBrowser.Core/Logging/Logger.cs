/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Entities;

namespace SafeExamBrowser.Core.Logging
{
	public class Logger : ILogger
	{
		private static readonly object @lock = new object();
		private readonly IList<ILogMessage> log = new List<ILogMessage>();
		private readonly IList<ILogObserver> observers = new List<ILogObserver>();

		public void Error(string message)
		{
			Log(LogLevel.Error, message);
		}

		public void Info(string message)
		{
			Log(LogLevel.Info, message);
		}

		public void Warn(string message)
		{
			Log(LogLevel.Warn, message);
		}

		public IList<ILogMessage> GetLog()
		{
			lock (@lock)
			{
				return log.Select(m => m.Clone() as ILogMessage).ToList();
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

		private void Log(LogLevel severity, string message)
		{
			lock (@lock)
			{
				var entry = new LogMessage(DateTime.Now, severity, message);

				log.Add(entry);
				
				foreach (var observer in observers)
				{
					observer.Notify(entry);
				}
			}
		}
	}
}
