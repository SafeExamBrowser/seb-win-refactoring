/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Mobile.Windows;

namespace SafeExamBrowser.UserInterface.Mobile
{
	public class FileSystemDialogFactory : IFileSystemDialog
	{
		private IText text;

		public FileSystemDialogFactory(IText text)
		{
			this.text = text;
		}

		public FileSystemDialogResult Show(
			FileSystemElement element,
			FileSystemOperation operation,
			string initialPath = default(string),
			string message = null,
			string title = null,
			IWindow parent = null,
			bool restrictNavigation = false)
		{
			if (parent is Window window)
			{
				return window.Dispatcher.Invoke(() => new FileSystemDialog(element, operation, text, initialPath, message, title, parent, restrictNavigation).Show());
			}
			else
			{
				return new FileSystemDialog(element, operation, text, initialPath, message, title, restrictNavigation: restrictNavigation).Show();
			}
		}
	}
}
