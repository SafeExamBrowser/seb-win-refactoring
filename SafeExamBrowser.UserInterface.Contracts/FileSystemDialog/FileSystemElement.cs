/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.FileSystemDialog
{
	/// <summary>
	/// Defines all elements supported by an <see cref="IFileSystemDialog"/>
	/// </summary>
	public enum FileSystemElement
	{
		/// <summary>
		/// A dialog to perform an operation with a file.
		/// </summary>
		File,

		/// <summary>
		/// A dialog to perform an operation with a folder.
		/// </summary>
		Folder
	}
}
