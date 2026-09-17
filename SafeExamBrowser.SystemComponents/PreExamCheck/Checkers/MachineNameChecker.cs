/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class MachineNameChecker : IChecker
	{
		private readonly ISystemInfo systemInfo;

		public string Name => "Computer Name";

		public MachineNameChecker(ISystemInfo systemInfo)
		{
			this.systemInfo = systemInfo;
		}

		public CheckResult Check()
		{
			string name = systemInfo.Name ?? Environment.MachineName;
			bool passed = !string.IsNullOrWhiteSpace(name);

			return new CheckResult
			{
				Category = "System",
				Title = Name,
				Status = passed ? CheckStatus.Passed : CheckStatus.Warning,
				ActualValue = name,
				RequiredValue = "Valid Computer Name",
				Message = passed ? "Valid computer name." : "Unable to retrieve computer name.",
				IsCritical = false
			};
		}
	}
}
