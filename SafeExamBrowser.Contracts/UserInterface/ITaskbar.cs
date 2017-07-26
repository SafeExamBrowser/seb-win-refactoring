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
		/// Returns the absolute height of the taskbar (i.e. in physical pixels).
		/// </summary>
		int GetAbsoluteHeight();

		/// <summary>
		/// Moves the taskbar to the bottom of and resizes it according to the current working area.
		/// </summary>
		void InitializeBounds();
	}
}
