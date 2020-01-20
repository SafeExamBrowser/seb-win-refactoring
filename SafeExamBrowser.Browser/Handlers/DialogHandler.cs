/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class DialogHandler : IDialogHandler
	{
		internal event DialogRequestedEventHandler DialogRequested;

		public bool OnFileDialog(IWebBrowser webBrowser, IBrowser browser, CefFileDialogMode mode, CefFileDialogFlags flags, string title, string defaultFilePath, List<string> acceptFilters, int selectedAcceptFilter, IFileDialogCallback callback)
		{
			var args = new DialogRequestedEventArgs
			{
				Element = ToElement(mode),
				InitialPath = defaultFilePath,
				Operation = ToOperation(mode),
				Title = title
			};

			Task.Run(() =>
			{
				DialogRequested?.Invoke(args);

				using (callback)
				{
					if (args.Success)
					{
						callback.Continue(selectedAcceptFilter, new List<string> { args.FullPath });
					}
					else
					{
						callback.Cancel();
					}
				}
			});

			return true;
		}

		private FileSystemElement ToElement(CefFileDialogMode mode)
		{
			switch (mode)
			{
				case CefFileDialogMode.OpenFolder:
					return FileSystemElement.Folder;
				default:
					return FileSystemElement.File;
			}
		}

		private FileSystemOperation ToOperation(CefFileDialogMode mode)
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
