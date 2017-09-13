/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.SystemComponents;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	public delegate void KeyboardLayoutSelectedEventHandler(IKeyboardLayout layout);

	public interface ISystemKeyboardLayoutControl : ISystemControl
	{
		/// <summary>
		/// Event fired when the user selected a keyboard layout.
		/// </summary>
		event KeyboardLayoutSelectedEventHandler LayoutSelected;

		/// <summary>
		/// Adds the given layout to the list of selectable keyboard layouts.
		/// </summary>
		void Add(IKeyboardLayout layout);
	}
}
