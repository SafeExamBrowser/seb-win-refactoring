/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Contracts.Events
{
	/// <summary>
	/// The key modifiers which can be detected by a keyboard hook.
	/// </summary>
	[Flags]
	public enum KeyModifier
	{
		None = 0,
		Alt = 0b1,
		Ctrl = 0b10
	}
}
