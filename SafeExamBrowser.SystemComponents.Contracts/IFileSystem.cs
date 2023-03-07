/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts
{
	/// <summary>
	/// Provides access to file system operations.
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		/// Creates all directories and subdirectories defined by the given path.
		/// </summary>
		void CreateDirectory(string path);

		/// <summary>
		/// Deletes the item at the given path, if it exists. Directories will be completely deleted, including all subdirectories and files.
		/// </summary>
		void Delete(string path);

		/// <summary>
		/// Saves the given content as a file under the specified path. If the file doesn't yet exist, it will be created, otherwise overwritten.
		/// </summary>
		void Save(string content, string path);
	}
}
