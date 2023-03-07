/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Wrapper;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class DialogHandler : IDialogHandler
	{
		internal event DialogRequestedEventHandler DialogRequested;

		public bool OnFileDialog(IWebBrowser webBrowser, IBrowser browser, CefFileDialogMode mode, string title, string defaultFilePath, List<string> acceptFilters, IFileDialogCallback callback)
		{
			var args = new DialogRequestedEventArgs
			{
				Element = mode.ToElement(),
				InitialPath = defaultFilePath,
				Operation = mode.ToOperation(),
				Title = title
			};

			Task.Run(() =>
			{
				DialogRequested?.Invoke(args);

				using (callback)
				{
					if (args.Success)
					{
						callback.Continue(new List<string> { args.FullPath });
					}
					else
					{
						callback.Cancel();
					}
				}
			});

			return true;
		}
	}
}
