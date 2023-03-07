/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown
{
	public class SystemConfigurationUpdate : ISystemConfigurationUpdate
	{
		private ILogger logger;

		public SystemConfigurationUpdate(ILogger logger)
		{
			this.logger = logger;
		}

		public void Execute()
		{
			try
			{
				logger.Info("Starting system configuration update...");

				var process = Process.Start(new ProcessStartInfo("cmd.exe", "/c \"gpupdate /force\"")
				{
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					UseShellExecute = false
				});

				logger.Info("Waiting for update to complete...");
				process.WaitForExit();

				var output = process.StandardOutput.ReadToEnd();
				var lines = output.Split(new [] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

				logger.Info($"Update has completed: {String.Join(" ", lines.Skip(1))}");
			}
			catch (Exception e)
			{
				logger.Error("Failed to update system configuration!", e);
			}
		}

		public void ExecuteAsync()
		{
			Task.Run(new Action(Execute));
		}
	}
}
