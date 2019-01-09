/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.SystemComponents;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar.Events
{
	/// <summary>
	/// Indicates that a particular <see cref="IKeyboardLayout"/> has been selected by the user.
	/// </summary>
	public delegate void KeyboardLayoutSelectedEventHandler(IKeyboardLayout layout);
}
