/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Desktop
{
	internal class FolderDialog : IFolderDialog
	{
		private string message;

		internal FolderDialog(string message)
		{
			this.message = message;
		}

		public FolderDialogResult Show(IWindow parent = null)
		{
			var result = new FolderDialogResult();

			using (var dialog = new FolderBrowserDialog())
			{
				var dialogResult = DialogResult.None;

				dialog.Description = message;
				dialog.ShowNewFolderButton = false;

				if (parent is Window w)
				{
					dialogResult = dialog.ShowDialog(new Win32Window(w));
				}
				else
				{
					dialogResult = dialog.ShowDialog();
				}

				if (dialogResult == DialogResult.OK)
				{
					result.FolderPath = dialog.SelectedPath;
					result.Success = true;
				}
			}

			return result;
		}

		private class Win32Window : System.Windows.Forms.IWin32Window
		{
			private Window w;

			public Win32Window(Window w)
			{
				this.w = w;
			}

			public IntPtr Handle => w.Dispatcher.Invoke(() => new WindowInteropHelper(w).Handle);
		}
	}
}
