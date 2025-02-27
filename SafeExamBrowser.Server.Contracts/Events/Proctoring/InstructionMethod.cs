/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Contracts.Events.Proctoring
{
	/// <summary>
	/// Defines all possible methods for a proctoring instruction.
	/// </summary>
	public enum InstructionMethod
	{
		/// <summary>
		/// Instructs to start proctoring resp. join a proctoring event or session.
		/// </summary>
		Join,

		/// <summary>
		/// Instructs to stop proctoring resp. leave a proctoring event or session.
		/// </summary>
		Leave
	}
}
