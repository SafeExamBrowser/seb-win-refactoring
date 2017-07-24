/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Monitoring.Windows
{
	public class WindowMonitor : IWindowMonitor
	{
		private ILogger logger;

		public WindowMonitor(ILogger logger)
		{
			this.logger = logger;

			// TODO: Make operation for window monitor OR operation for all desktop initialization?!
			// ...
		}

		public void HideAllWindows()
		{
			logger.Info("Minimizing all open windows...");
			User32.MinimizeAllOpenWindows();
			logger.Info("Open windows successfully minimized.");
		}

		public void RestoreHiddenWindows()
		{
			throw new NotImplementedException();
		}

		public void StartMonitoringWindows()
		{
			throw new NotImplementedException();
		}

		public void StopMonitoringWindows()
		{
			throw new NotImplementedException();
		}
	}
}
