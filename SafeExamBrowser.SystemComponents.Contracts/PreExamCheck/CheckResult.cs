/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.PreExamCheck
{
	public class CheckResult
	{
		public string Category { get; set; }
		public string Title { get; set; }
		public CheckStatus Status { get; set; }
		public string ActualValue { get; set; }
		public string RequiredValue { get; set; }
		public string Message { get; set; }
		public bool IsCritical { get; set; } = true;
	}
}
