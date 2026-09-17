/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class DiskChecker : IChecker
	{
		private readonly ISystemInfo systemInfo;
		private readonly double minFreeGb;

		public string Name => "Free hard drive space";

		public DiskChecker(ISystemInfo systemInfo, double minFreeGb = 2.0)
		{
			this.systemInfo = systemInfo;
			this.minFreeGb = minFreeGb;
		}

		public CheckResult Check()
		{
			double freeGb = 0;
			string driveName = "C:";

			try
			{
				var systemDrive = systemInfo.GetDrives()
					.FirstOrDefault(d => d.IsReady && string.Equals(d.Name, "C:\\", StringComparison.OrdinalIgnoreCase)) 
					?? systemInfo.GetDrives().FirstOrDefault(d => d.IsReady);

				if (systemDrive != null)
				{
					driveName = systemDrive.Name;
					freeGb = (double)systemDrive.AvailableFreeSpace / (1024 * 1024 * 1024);
				}
			}
			catch
			{
			}

			bool passed = freeGb >= minFreeGb;

			return new CheckResult
			{
				Category = "Disk",
				Title = Name,
				Status = passed ? CheckStatus.Passed : CheckStatus.Failed,
				ActualValue = $"{freeGb:F2} GB ({driveName})",
				RequiredValue = $"≥ {minFreeGb:F2} GB",
				Message = passed ? "Sufficient free hard drive space available." : $"Hard drive {driveName} is running low on space, at least {minFreeGb:F1} GB is required.",
				IsCritical = true
			};
		}
	}
}
