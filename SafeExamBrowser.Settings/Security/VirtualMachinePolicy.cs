/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Security
{
	/// <summary>
	/// Defines all policies with respect to running SEB in a virtual machine.
	/// </summary>
	public enum VirtualMachinePolicy
	{
		/// <summary>
		/// SEB is allowed to be run in a virtual machine.
		/// </summary>
		Allow,

		/// <summary>
		/// SEB is not allowed to be run in a virtual machine.
		/// </summary>
		Deny
	}
}
