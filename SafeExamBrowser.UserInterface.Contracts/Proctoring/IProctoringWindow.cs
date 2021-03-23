/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.UserInterface.Contracts.Proctoring
{
	/// <summary>
	/// Defines the functionality of a proctoring window.
	/// </summary>
	public interface IProctoringWindow : IWindow
	{
		/// <summary>
		/// Sets the window title to the given value.
		/// </summary>
		void SetTitle(string title);

		/// <summary>
		/// Toggles the window visibility.
		/// </summary>
		void Toggle();
	}
}
