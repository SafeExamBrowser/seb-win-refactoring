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
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Contracts.WindowsApi.Types;

namespace SafeExamBrowser.Configuration
{
	public class WorkingArea : IWorkingArea
	{
		private ILogger logger;
		private INativeMethods nativeMethods;
		private RECT? originalWorkingArea;

		public WorkingArea(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public void InitializeFor(ITaskbar taskbar)
		{
			originalWorkingArea = nativeMethods.GetWorkingArea();

			LogWorkingArea("Saved original working area", originalWorkingArea.Value);

			var area = new RECT
			{
				Left = 0,
				Top = 0,
				Right = Screen.PrimaryScreen.Bounds.Width,
				Bottom = Screen.PrimaryScreen.Bounds.Height - taskbar.GetAbsoluteHeight()
			};

			LogWorkingArea("Trying to set new working area", area);
			nativeMethods.SetWorkingArea(area);
			LogWorkingArea("Working area is now set to", nativeMethods.GetWorkingArea());
		}

		public void Reset()
		{
			if (originalWorkingArea.HasValue)
			{
				nativeMethods.SetWorkingArea(originalWorkingArea.Value);
				LogWorkingArea("Restored original working area", originalWorkingArea.Value);
			}
		}

		private void LogWorkingArea(string message, RECT area)
		{
			logger.Info($"{message}: Left = {area.Left}, Top = {area.Top}, Right = {area.Right}, Bottom = {area.Bottom}.");
		}
	}
}
