﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Keyboard.Events
{
	/// <summary>
	/// Indicates that the active keyboard layout has changed.
	/// </summary>
	public delegate void LayoutChangedEventHandler(IKeyboardLayout layout);
}
