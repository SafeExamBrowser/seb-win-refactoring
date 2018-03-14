/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Contracts.UserInterface.Windows
{
	/// <summary>
	/// The main window of the runtime application component. It is controlled by the <see cref="Behaviour.IRuntimeController"/> and serves
	/// first of all as progress indicator for the user (e.g. during application startup &amp; shutdown).
	/// </summary>
	public interface IRuntimeWindow : ILogObserver, IProgressIndicator, IWindow
	{
		/// <summary>
		/// Determines whether the window will stay on top of other windows.
		/// </summary>
		bool TopMost { get; set; }

		/// <summary>
		/// Hides the progress bar.
		/// </summary>
		void HideProgressBar();

		/// <summary>
		/// Shows the progress bar.
		/// </summary>
		void ShowProgressBar();
	}
}
