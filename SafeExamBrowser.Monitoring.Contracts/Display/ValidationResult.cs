/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Monitoring.Contracts.Display
{
	/// <summary>
	/// Provides the result of a display configuration validation.
	/// </summary>
	public class ValidationResult
	{
		/// <summary>
		/// Specifies the count of external displays detected.
		/// </summary>
		public int ExternalDisplays { get; set; }

		/// <summary>
		/// Specifies the count of internal displays detected.
		/// </summary>
		public int InternalDisplays { get; set; }

		/// <summary>
		/// Indicates whether the active display configuration is allowed.
		/// </summary>
		public bool IsAllowed { get; set; }
	}
}
