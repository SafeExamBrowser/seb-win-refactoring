/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// Defines the functionality of a password dialog.
	/// </summary>
	public interface IPasswordDialog : IWindow
	{
		/// <summary>
		/// Shows the dialog as topmost window. If a parent window is specified, the dialog is rendered modally for the given parent.
		/// </summary>
		IPasswordDialogResult Show(IWindow parent = null);
	}
}
