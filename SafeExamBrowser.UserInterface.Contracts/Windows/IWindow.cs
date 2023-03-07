/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Windows.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// Defines the functionality of a window.
	/// </summary>
	public interface IWindow
	{
		/// <summary>
		/// Event fired when the window has been closed;
		/// </summary>
		event WindowClosedEventHandler Closed;

		/// <summary>
		/// Event fired when the window is closing.
		/// </summary>
		event WindowClosingEventHandler Closing;

		/// <summary>
		/// Brings the window to the foreground.
		/// </summary>
		void BringToForeground();

		/// <summary>
		/// Closes the window.
		/// </summary>
		void Close();

		/// <summary>
		/// Hides the window.
		/// </summary>
		void Hide();

		/// <summary>
		/// Shows the window to the user.
		/// </summary>
		void Show();
	}
}
