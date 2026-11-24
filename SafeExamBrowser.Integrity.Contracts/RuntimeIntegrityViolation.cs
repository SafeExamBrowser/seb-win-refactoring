/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Integrity.Contracts
{
	/// <summary>
	/// Defines all possible runtime integrity violations.
	/// </summary>
	[Flags]
	public enum RuntimeIntegrityViolation
	{
		/// <summary>
		/// Inidcates that no violation has been detected.
		/// </summary>
		None = 0,

		/// <summary>
		/// Indicates that the runtime behavior is compromised.
		/// </summary>
		Behavior = 0b1,

		/// <summary>
		/// Indicates that the runtime components are compromised.
		/// </summary>
		Components = 0b10
	}
}
