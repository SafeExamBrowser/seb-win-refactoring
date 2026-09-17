/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;
using SafeExamBrowser.SystemComponents.PreExamCheck.Checkers;

namespace SafeExamBrowser.SystemComponents.PreExamCheck
{
	public class PreExamCheckService : IPreExamCheckService
	{
		private readonly ISystemInfo systemInfo;
		private readonly IEnumerable<IChecker> checkers;

		public PreExamCheckService(ISystemInfo systemInfo, IEnumerable<IChecker> checkers = null)
		{
			this.systemInfo = systemInfo;
			this.checkers = checkers ?? CreateDefaultCheckers(systemInfo);
		}

		public PreExamCheckReport RunAllChecks()
		{
			var report = new PreExamCheckReport
			{
				Timestamp = DateTime.Now,
				MachineName = systemInfo?.Name ?? Environment.MachineName
			};

			foreach (var checker in checkers)
			{
				try
				{
					var result = checker.Check();
					report.Results.Add(result);
				}
				catch (Exception ex)
				{
					report.Results.Add(new CheckResult
					{
						Category = "Error",
						Title = checker.Name,
						Status = CheckStatus.Failed,
						ActualValue = "Error occurred",
						RequiredValue = "-",
						Message = $"Error occurred while performing check: {ex.Message}",
						IsCritical = true
					});
				}
			}

			return report;
		}

		private static IEnumerable<IChecker> CreateDefaultCheckers(ISystemInfo systemInfo)
		{
			return new List<IChecker>
			{
				new OSChecker(systemInfo),
				new RAMChecker(),
				new DiskChecker(systemInfo),
				new InternetChecker(),
				new ScreenChecker(),
				new PowerChecker(systemInfo),
				new MachineNameChecker(systemInfo)
			};
		}
	}
}
