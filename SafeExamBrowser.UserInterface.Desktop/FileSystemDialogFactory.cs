/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Desktop.Windows;
using SafeExamBrowser.UserInterface.Shared;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public class FileSystemDialogFactory : Guardable, IFileSystemDialog
	{
		private readonly ISystemInfo systemInfo;
		private readonly IText text;

		public FileSystemDialogFactory(ISystemInfo systemInfo, IText text, IWindowGuard windowGuard) : base(windowGuard)
		{
			this.systemInfo = systemInfo;
			this.text = text;
		}

		public FileSystemDialogResult Show(
			FileSystemElement element,
			FileSystemOperation operation,
			string initialPath = default,
			string message = default,
			string title = default,
			IWindow parent = default,
			bool restrictNavigation = false,
			bool showElementPath = true)
		{
			if (parent is Window window)
			{
				return window.Dispatcher.Invoke(() =>
				{
					var dialog = Guard(new FileSystemDialog(element, operation, systemInfo, text, initialPath, message, title, parent, restrictNavigation, showElementPath));
					var result = dialog.Show();

					return result;
				});
			}
			else
			{
				var dialog = Guard(new FileSystemDialog(element, operation, systemInfo, text, initialPath, message, title, restrictNavigation: restrictNavigation, showElementPath: showElementPath));
				var result = dialog.Show();

				return result;
			}
		}
	}
}
