/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface
{
	public interface ITaskbar
	{
		/// <summary>
		/// Adds the given application button to the taskbar.
		/// </summary>
		void AddButton(ITaskbarButton button);

		/// <summary>
		/// Adds the given notification button to the taskbar.
		/// </summary>
		/// <param name="button"></param>
		void AddNotification(ITaskbarNotification button);

		/// <summary>
		/// Moves the taskbar to the given location on the screen.
		/// </summary>
		void SetPosition(int x, int y);

		/// <summary>
		/// Sets the size of the taskbar.
		/// </summary>
		void SetSize(int width, int height);
	}
}
