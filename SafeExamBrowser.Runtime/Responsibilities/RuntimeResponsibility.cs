/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal abstract class RuntimeResponsibility : IResponsibility<RuntimeTask>
	{
		protected RuntimeContext Context { get; private set; }
		protected ILogger Logger { get; private set; }

		protected SessionConfiguration Session => Context.Current;
		protected bool SessionIsRunning => Session != default;

		internal RuntimeResponsibility(ILogger logger, RuntimeContext runtimeContext)
		{
			Logger = logger;
			Context = runtimeContext;
		}

		public abstract void Assume(RuntimeTask task);

		protected string AppendLogFilePaths(AppConfig appConfig, string message)
		{
			if (File.Exists(appConfig.BrowserLogFilePath))
			{
				message += $"{Environment.NewLine}{Environment.NewLine}{appConfig.BrowserLogFilePath}";
			}

			if (File.Exists(appConfig.ClientLogFilePath))
			{
				message += $"{Environment.NewLine}{Environment.NewLine}{appConfig.ClientLogFilePath}";
			}

			if (File.Exists(appConfig.RuntimeLogFilePath))
			{
				message += $"{Environment.NewLine}{Environment.NewLine}{appConfig.RuntimeLogFilePath}";
			}

			if (File.Exists(appConfig.ServiceLogFilePath))
			{
				message += $"{Environment.NewLine}{Environment.NewLine}{appConfig.ServiceLogFilePath}";
			}

			return message;
		}
	}
}
