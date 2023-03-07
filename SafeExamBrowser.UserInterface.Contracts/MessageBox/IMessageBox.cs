/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.UserInterface.Contracts.MessageBox
{
	/// <summary>
	/// Defines the API for message boxes.
	/// </summary>
	public interface IMessageBox
	{
		/// <summary>
		/// Shows a message box according to the specified parameters and returns the result chosen by the user.
		/// </summary>
		MessageBoxResult Show(string message, string title, MessageBoxAction action = MessageBoxAction.Ok, MessageBoxIcon icon = MessageBoxIcon.Information, IWindow parent = null);

		/// <summary>
		/// Shows a message box according to the specified parameters and returns the result chosen by the user.
		/// </summary>
		MessageBoxResult Show(TextKey message, TextKey title, MessageBoxAction action = MessageBoxAction.Ok, MessageBoxIcon icon = MessageBoxIcon.Information, IWindow parent = null);
	}
}
