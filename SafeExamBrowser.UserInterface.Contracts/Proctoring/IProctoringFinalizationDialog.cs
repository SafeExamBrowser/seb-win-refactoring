/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.UserInterface.Contracts.Proctoring.Events;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.UserInterface.Contracts.Proctoring
{
	/// <summary>
	/// The dialog to display the status of the proctoring finalization.
	/// </summary>
	public interface IProctoringFinalizationDialog : IWindow
	{
		/// <summary>
		/// The quit password entered by the user.
		/// </summary>
		string QuitPassword { get; }

		/// <summary>
		/// Event fired when the cancellation of the remaining work has been requested.
		/// </summary>
		event CancellationRequestedEventHandler CancellationRequested;

		/// <summary>
		/// Updates the status of the finalization.
		/// </summary>
		void Update(RemainingWorkUpdatedEventArgs status);
	}
}
