/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Browser.Contracts.Events;

namespace SafeExamBrowser.Browser.Contracts
{
	/// <summary>
	/// Controls the lifetime and functionality of the browser application.
	/// </summary>
	public interface IBrowserApplication : IApplication
	{
		/// <summary>
		/// Event fired when the browser application detects a download request for an application configuration file.
		/// </summary>
		event DownloadRequestedEventHandler ConfigurationDownloadRequested;

		/// <summary>
		/// Event fired when the browser application detects a session identifier of an LMS.
		/// </summary>
		event SessionIdentifierDetectedEventHandler SessionIdentifierDetected;

		/// <summary>
		/// Event fired when the browser application detects a request to terminate SEB.
		/// </summary>
		event TerminationRequestedEventHandler TerminationRequested;

		/// <summary>
		/// Event fired when the user tries to focus the taskbar.
		/// </summary>
		event LoseFocusRequestedEventHandler LoseFocusRequested;

		/// <summary>
		/// Transfers the focus to the browser application. If the parameter is <c>true</c>, the first focusable element in the browser window
		/// receives focus (passing forward of focus). Otherwise, the last element receives focus.
		/// </summary>
		void Focus(bool forward);
	}
}
