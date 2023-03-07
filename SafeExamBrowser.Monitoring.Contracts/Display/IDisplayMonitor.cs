/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Monitoring.Contracts.Display.Events;
using SafeExamBrowser.Settings.Monitoring;

namespace SafeExamBrowser.Monitoring.Contracts.Display
{
	/// <summary>
	/// Monitors the displays of the computer for changes and provides access to display-related functionality.
	/// </summary>
	public interface IDisplayMonitor
	{
		/// <summary>
		/// Event fired when the primary display or its settings have changed.
		/// </summary>
		event DisplayChangedEventHandler DisplayChanged;
		
		/// <summary>
		/// Sets the desktop working area to accommodate to the taskbar's height and removes the configured wallpaper (if possible).
		/// </summary>
		void InitializePrimaryDisplay(int taskbarHeight);

		/// <summary>
		/// Prevents the computer from entering sleep mode and turning its display(s) off.
		/// </summary>
		void PreventSleepMode();

		/// <summary>
		/// Resets the desktop working area and wallpaper to their previous (initial) state.
		/// </summary>
		void ResetPrimaryDisplay();

		/// <summary>
		/// Starts monitoring for display changes, i.e. display changes will trigger the <c>DisplaySettingsChanged</c> event.
		/// </summary>
		void StartMonitoringDisplayChanges();

		/// <summary>
		/// Stops monitoring for display changes.
		/// </summary>
		void StopMonitoringDisplayChanges();

		/// <summary>
		/// Validates the currently active display configuration according to the given settings.
		/// </summary>
		ValidationResult ValidateConfiguration(DisplaySettings settings);
	}
}
