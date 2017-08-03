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
		/// Updates the progress bar of the splash screen according to the specified amount.
		/// </summary>
		void Progress(int amount = 1);

		/// <summary>
		/// Regresses the progress bar of the splash screen according to the specified amount.
		/// </summary>
		void Regress(int amount = 1);

		/// <summary>
		/// Sets the style of the progress bar to indeterminate, i.e. <c>Progress</c> and
		/// <c>Regress</c> won't have any effect when called.
		/// </summary>
		void SetIndeterminate();

		/// <summary>
		/// Set the maximum of the splash screen's progress bar.
		/// </summary>
		void SetMaxProgress(int max);

		/// <summary>
		/// Updates the status text of the splash screen. If the busy flag is set,
		/// the splash screen will show an animation to indicate a long-running operation.
		/// </summary>
		void UpdateText(TextKey key, bool showBusyIndication = false);
	}
}
