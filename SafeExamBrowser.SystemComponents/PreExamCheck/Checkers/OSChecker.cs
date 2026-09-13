/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;
using OperatingSystem = SafeExamBrowser.SystemComponents.Contracts.OperatingSystem;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class OSChecker : IChecker
	{
		private readonly ISystemInfo systemInfo;

		public string Name => "Operating System (Windows)";

		public OSChecker(ISystemInfo systemInfo)
		{
			this.systemInfo = systemInfo;
		}

		public CheckResult Check()
		{
			var os = systemInfo.OperatingSystem;
			var osInfo = systemInfo.OperatingSystemInfo;
			var isSupported = os == OperatingSystem.Windows10 || os == OperatingSystem.Windows11;

			return new CheckResult
			{
				Category = "OS",
				Title = Name,
				Status = isSupported ? CheckStatus.Passed : CheckStatus.Failed,
				ActualValue = osInfo,
				RequiredValue = "Windows 10 (v1803+) / Windows 11",
				Message = isSupported ? "Operating system meets requirements." : "Windows 10 (version 1803 or higher) or Windows 11 is required.",
				IsCritical = true
			};
		}
	}
}
