/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard.Events;

namespace SafeExamBrowser.SystemComponents.Contracts.Keyboard
{
	/// <summary>
	/// Defines the functionality of the keyboard system component.
	/// </summary>
	public interface IKeyboard : ISystemComponent
	{
		/// <summary>
		/// Fired when the active keyboard layout changed.
		/// </summary>
		event LayoutChangedEventHandler LayoutChanged;

		/// <summary>
		/// Activates the keyboard layout with the given identifier.
		/// </summary>
		void ActivateLayout(Guid id);

		/// <summary>
		/// Gets all currently available keyboard layouts.
		/// </summary>
		IEnumerable<IKeyboardLayout> GetLayouts();
	}
}
