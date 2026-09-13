/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeExamBrowser.SystemComponents.Contracts.PreExamCheck
{
	public class PreExamCheckReport
	{
		public DateTime Timestamp { get; set; } = DateTime.Now;
		public string MachineName { get; set; }
		public List<CheckResult> Results { get; set; } = new List<CheckResult>();

		public bool PassedAllCritical => Results.Where(r => r.IsCritical).All(r => r.Status == CheckStatus.Passed || r.Status == CheckStatus.Warning);
		public bool HasWarnings => Results.Any(r => r.Status == CheckStatus.Warning);
		public bool HasFailures => Results.Any(r => r.Status == CheckStatus.Failed);
	}
}
