/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.SystemComponents
{
	public class FileSystem : IFileSystem
	{
		public void CreateDirectory(string path)
		{
			Directory.CreateDirectory(path);
		}

		public void Delete(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}

			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}

		public void Save(string content, string path)
		{
			File.WriteAllText(path, content);
		}
	}
}
