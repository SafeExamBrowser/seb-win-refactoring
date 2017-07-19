/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.I18n;

namespace SafeExamBrowser.Contracts.UserInterface
{
	public interface ISplashScreen
	{
		/// <summary>
		/// Closes the splash screen on its own thread.
		/// </summary>
		void InvokeClose();

		/// <summary>
		/// Shows the splash screen on its own thread.
		/// </summary>
		void InvokeShow();

		/// <summary>
		/// Set the maximum of the splash screen's progress bar.
		/// </summary>
		void SetMaxProgress(int max);

		/// <summary>
		/// Starts an animation indicating the user that something is going on.
		/// </summary>
		void StartBusyIndication();

		/// <summary>
		/// Stops the busy animation, if it was running.
		/// </summary>
		void StopBusyIndication();

		/// <summary>
		/// Updates the progress bar of the splash screen according to the specified amount.
		/// </summary>
		void UpdateProgress(int amount = 1);

		/// <summary>
		/// Updates the status text of the splash screen.
		/// </summary>
		void UpdateText(Key key);
	}
}
