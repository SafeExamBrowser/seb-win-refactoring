/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;

namespace SafeExamBrowser.Browser.Wrapper
{
	internal static class Extensions
	{
		internal static FileSystemElement ToElement(this CefFileDialogMode mode)
		{
			switch (mode)
			{
				case CefFileDialogMode.OpenFolder:
					return FileSystemElement.Folder;
				default:
					return FileSystemElement.File;
			}
		}

		internal static FileSystemOperation ToOperation(this CefFileDialogMode mode)
		{
			switch (mode)
			{
				case CefFileDialogMode.Save:
					return FileSystemOperation.Save;
				default:
					return FileSystemOperation.Open;
			}
		}
	}
}
