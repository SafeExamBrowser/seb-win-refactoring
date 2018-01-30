/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	public interface ISettings
	{
		/// <summary>
		/// The mode which determines the configuration behaviour.
		/// </summary>
		ConfigurationMode ConfigurationMode { get; }

		/// <summary>
		/// All browser-related settings.
		/// </summary>
		IBrowserSettings Browser { get; }

		/// <summary>
		/// All keyboard-related settings.
		/// </summary>
		IKeyboardSettings Keyboard { get; }

		/// <summary>
		/// The kiosk mode which determines how the computer is locked down.
		/// </summary>
		KioskMode KioskMode { get; }

		/// <summary>
		/// All mouse-related settings.
		/// </summary>
		IMouseSettings Mouse { get; }

		/// <summary>
		/// The active policy for the service component.
		/// </summary>
		ServicePolicy ServicePolicy { get; }

		/// <summary>
		/// All taskbar-related settings.
		/// </summary>
		ITaskbarSettings Taskbar { get; }
	}
}
