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

namespace SafeExamBrowser.Logging
{
	/// <summary>
	/// Wraps an implementation of <see cref="ILogger"/> in order to append information about the module from which messages are logged.
	/// </summary>
	public class ModuleLogger : ILogger
	{
		private ILogger logger;
		private string moduleInfo;

		public ModuleLogger(ILogger logger, string moduleInfo)
		{
			this.logger = logger;
			this.moduleInfo = moduleInfo;
		}

		public ModuleLogger(ILogger logger, Type module) : this(logger, module.Name)
		{
		}

		public void Debug(string message)
		{
			logger.Debug(AppendModuleInfo(message));
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
			return $"[{moduleInfo}] {message}";
		}
	}
}
