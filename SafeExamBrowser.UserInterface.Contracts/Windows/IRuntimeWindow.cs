/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// The main window of the runtime application component. It is controlled by the <see cref="Behaviour.IRuntimeController"/> and serves
	/// first of all as progress indicator for the user (e.g. during application startup &amp; shutdown).
	/// </summary>
	public interface IRuntimeWindow : ILogObserver, IProgressIndicator, IWindow
	{
		/// <summary>
		/// Determines whether the application log is visible.
		/// </summary>
		bool ShowLog { set; }

		/// <summary>
		/// Determines whether the progress bar is visible.
		/// </summary>
		bool ShowProgressBar { set; }

		/// <summary>
		/// Determines whether the window will stay on top of other windows.
		/// </summary>
		bool TopMost { set; }
	}
}
