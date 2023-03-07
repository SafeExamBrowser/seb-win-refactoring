/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Proctoring.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Proctoring
{
	/// <summary>
	/// Defines the functionality of a proctoring control, i.e. a web view running a WebRTC-enabled web application, which normally is embedded in
	/// a <see cref="IProctoringWindow"/>.
	/// </summary>
	public interface IProctoringControl
	{
		/// <summary>
		/// Event fired when the full screen state changed.
		/// </summary>
		event FullScreenChangedEventHandler FullScreenChanged;
	}
}
