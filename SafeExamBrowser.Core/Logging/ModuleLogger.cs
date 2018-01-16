/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Logging
{
	public class ModuleLogger : ILogger
	{
		private ILogger logger;
		private Type module;

		/// <summary>
		/// Creates a wrapper around an <c>ILogger</c> that includes information
		/// about the specified module when logging messages with a severity.
		/// </summary>
		public ModuleLogger(ILogger logger, Type module)
		{
			this.logger = logger;
			this.module = module;
		}

		public void Error(string message)
		{
			logger.Error(AppendModuleInfo(message));
		}

		public void Error(string message, Exception exception)
		{
			logger.Error(AppendModuleInfo(message), exception);
		}

		public IList<ILogContent> GetLog()
		{
			return logger.GetLog();
		}

		public void Info(string message)
		{
			logger.Info(AppendModuleInfo(message));
		}

		public void Log(string message)
		{
			logger.Log(message);
		}

		public void Log(ILogContent content)
		{
			logger.Log(content);
		}

		public void Subscribe(ILogObserver observer)
		{
			logger.Subscribe(observer);
		}

		public void Unsubscribe(ILogObserver observer)
		{
			logger.Unsubscribe(observer);
		}

		public void Warn(string message)
		{
			logger.Warn(AppendModuleInfo(message));
		}

		private string AppendModuleInfo(string message)
		{
			return $"[{module.Name}] {message}";
		}
	}
}
