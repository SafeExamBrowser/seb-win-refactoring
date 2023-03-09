/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.UserInterface.Contracts.FileSystemDialog
{
	public interface IFileSystemDialog
	{
		/// <summary>
		/// Creates a dialog according to the given parameters and shows it to the user.
		/// </summary>
		FileSystemDialogResult Show(
			FileSystemElement element,
			FileSystemOperation operation,
			string initialPath = default,
			string message = default,
			string title = default,
			IWindow parent = default,
			bool restrictNavigation = false,
			bool showElementPath = true);
	}
}
