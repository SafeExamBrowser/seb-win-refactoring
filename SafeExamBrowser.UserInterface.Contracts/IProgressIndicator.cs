/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.I18n.Contracts;

namespace SafeExamBrowser.UserInterface.Contracts
{
	/// <summary>
	/// A progress indicator is a user interface element which displays the (completion) status of a procedure to the user.
	/// </summary>
	public interface IProgressIndicator
	{
		/// <summary>
		/// Increases the current progress value by 1.
		/// </summary>
		void Progress();

		/// <summary>
		/// Decreases the current progress value by 1.
		/// </summary>
		void Regress();

		/// <summary>
		/// Sets the style of the progress indicator to indeterminate (<c>Progress</c> and <c>Regress</c> won't have any effect when called).
		/// </summary>
		void SetIndeterminate();

		/// <summary>
		/// Sets the maximum progress value.
		/// </summary>
		void SetMaxValue(int max);

		/// <summary>
		/// Sets the current progress value.
		/// </summary>
		void SetValue(int value);

		/// <summary>
		/// Updates the status text. If the busy flag is set, an animation will be shown to indicate a long-running operation.
		/// </summary>
		void UpdateStatus(TextKey key, bool busyIndication = false);
	}
}
