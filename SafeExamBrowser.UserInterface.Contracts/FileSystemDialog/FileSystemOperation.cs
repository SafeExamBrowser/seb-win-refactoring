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
	/// Defines all operations supported by an <see cref="IFileSystemDialog"/>
	/// </summary>
	public enum FileSystemOperation
	{
		/// <summary>
		/// A dialog to open a file system element.
		/// </summary>
		Open,

		/// <summary>
		/// A dialog to save a file system element.
		/// </summary>
		Save
	}
}
