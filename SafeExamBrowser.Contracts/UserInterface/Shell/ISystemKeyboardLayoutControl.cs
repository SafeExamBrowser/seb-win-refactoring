/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.Contracts.UserInterface.Shell
{
	/// <summary>
	/// The control of the keyboard layout system component.
	/// </summary>
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
