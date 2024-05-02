/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// Defines the functionality of a network dialog.
	/// </summary>
	public interface INetworkDialog : IWindow
	{
		/// <summary>
		/// Shows the dialog as topmost window. If a parent window is specified, the dialog is rendered modally for the given parent.
		/// </summary>
		NetworkDialogResult Show(IWindow parent = null);
	}
}
