/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// Defines the functionality of an exam selection dialog.
	/// </summary>
	public interface IExamSelectionDialog
	{
		/// <summary>
		/// Shows the dialog as topmost window. If a parent window is specified, the dialog is rendered modally for the given parent.
		/// </summary>
		ExamSelectionDialogResult Show(IWindow parent = null);
	}
}
