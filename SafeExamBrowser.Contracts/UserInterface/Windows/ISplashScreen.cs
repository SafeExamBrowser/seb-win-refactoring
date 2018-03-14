/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.UserInterface.Windows
{
	/// <summary>
	/// Defines the functionality of a splash screen.
	/// </summary>
	public interface ISplashScreen : IProgressIndicator, IWindow
	{
		/// <summary>
		/// The runtime information used to display version and copyright information. Can be updated during the execution of a procedure.
		/// </summary>
		RuntimeInfo RuntimeInfo { set; }
	}
}
