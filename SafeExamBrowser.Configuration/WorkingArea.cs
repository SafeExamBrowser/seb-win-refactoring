/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.WindowsApi;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.Configuration
{
	public class WorkingArea : IWorkingArea
	{
		private ILogger logger;
		private RECT? initial;

		public WorkingArea(ILogger logger)
		{
			this.logger = logger;
		}

		public void InitializeFor(ITaskbar taskbar)
		{
			initial = User32.GetWorkingArea();

			LogWorkingArea("Saved initial working area", initial.Value);

			var area = new RECT
			{
				Left = 0,
				Top = 0,
				Right = Screen.PrimaryScreen.Bounds.Width,
				Bottom = Screen.PrimaryScreen.Bounds.Height - taskbar.GetAbsoluteHeight()
			};

			LogWorkingArea("Setting new working area", area);
			User32.SetWorkingArea(area);
			LogWorkingArea("Working area is now set to", User32.GetWorkingArea());
		}

		public void Reset()
		{
			if (initial.HasValue)
			{
				User32.SetWorkingArea(initial.Value);
				LogWorkingArea("Restored initial working area", initial.Value);
			}
		}

		private void LogWorkingArea(string message, RECT area)
		{
			logger.Info($"{message}: Left = {area.Left}, Top = {area.Top}, Right = {area.Right}, Bottom = {area.Bottom}.");
		}
	}
}
