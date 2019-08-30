/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// The control for a (third-party) application which can be loaded into the shell.
	/// </summary>
	public interface IApplicationControl
	{
		/// <summary>
		/// Event fired when the user clicked on the application control. If multiple instances of an application are active,
		/// the handler should only executed when the user selects one of the instances.
		/// </summary>
		event ApplicationControlClickedEventHandler Clicked;
	}
}
