/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.Contracts.UserInterface.Shell
{
	/// <summary>
	/// The taskbar is a user interface element via which the user can access and control various aspects of the application.
	/// </summary>
	public interface ITaskbar
	{
		/// <summary>
		/// Controls the visibility of the clock.
		/// </summary>
		bool ShowClock { set; }

		/// <summary>
		/// Event fired when the user clicked the quit button in the taskbar.
		/// </summary>
		event QuitButtonClickedEventHandler QuitButtonClicked;

		/// <summary>
		/// Adds the given application control to the taskbar.
		/// </summary>
		void AddApplicationControl(IApplicationControl control);

		/// <summary>
		/// Adds the given notification control to the taskbar.
		/// </summary>
		void AddNotificationControl(INotificationControl control);

		/// <summary>
		/// Adds the given system control to the taskbar.
		/// </summary>
		void AddSystemControl(ISystemControl control);

		/// <summary>
		/// Closes the taskbar.
		/// </summary>
		void Close();

		/// <summary>
		/// Returns the absolute height of the taskbar (i.e. in physical pixels).
		/// </summary>
		int GetAbsoluteHeight();

		/// <summary>
		/// Moves the taskbar to the bottom of and resizes it according to the current working area.
		/// </summary>
		void InitializeBounds();

		/// <summary>
		/// Shows the taskbar.
		/// </summary>
		void Show();
	}
}
