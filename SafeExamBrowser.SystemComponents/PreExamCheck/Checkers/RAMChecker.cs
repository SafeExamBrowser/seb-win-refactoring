/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Management;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class RAMChecker : IChecker
	{
		private readonly double minRamGb;

		public string Name => "RAM capacity";

		public RAMChecker(double minRamGb = 2.0)
		{
			this.minRamGb = minRamGb;
		}

		public CheckResult Check()
		{
			double totalRamGb = GetTotalMemoryInGb();
			bool passed = totalRamGb >= minRamGb;

			return new CheckResult
			{
				Category = "RAM",
				Title = Name,
				Status = passed ? CheckStatus.Passed : CheckStatus.Failed,
				ActualValue = $"{totalRamGb:F2} GB",
				RequiredValue = $"≥ {minRamGb:F2} GB",
				Message = passed ? "RAM capacity meets requirements." : $"Insufficient RAM capacity (minimum {minRamGb:F1} GB required).",
				IsCritical = true
			};
		}

		private double GetTotalMemoryInGb()
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
				using (var results = searcher.Get())
				{
					foreach (var result in results)
					{
						var bytes = Convert.ToDouble(result["TotalPhysicalMemory"]);
						return bytes / (1024 * 1024 * 1024);
					}
				}
			}
			catch
			{
			}

			return 4.0;
		}
	}
}
