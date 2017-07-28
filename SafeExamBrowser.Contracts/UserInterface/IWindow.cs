/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface
{
	public delegate void WindowCloseHandler();

	public interface IWindow
	{
		/// <summary>
		/// Event fired when the window is closing.
		/// </summary>
		event WindowCloseHandler OnClose;

		/// <summary>
		/// Brings the window to the foreground.
		/// </summary>
		void BringToForeground();

		/// <summary>
		/// Closes the window.
		/// </summary>
		void Close();

		/// <summary>
		/// Shows the window to the user.
		/// </summary>
		void Show();
	}
}
