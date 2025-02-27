/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Proctoring.Contracts.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Proctoring
{
	/// <summary>
	/// The dialog to display the status of the proctoring finalization.
	/// </summary>
	public interface IProctoringFinalizationDialog
	{
		/// <summary>
		/// Shows the dialog as topmost window.
		/// </summary>
		void Show();

		/// <summary>
		/// Updates the status of the finalization.
		/// </summary>
		void Update(RemainingWorkUpdatedEventArgs status);
	}
}
