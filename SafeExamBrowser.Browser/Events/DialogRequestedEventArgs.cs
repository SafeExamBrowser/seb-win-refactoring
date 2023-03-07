/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;

namespace SafeExamBrowser.Browser.Events
{
	internal class DialogRequestedEventArgs
	{
		internal FileSystemElement Element { get; set; }
		internal string InitialPath { get; set; }
		internal FileSystemOperation Operation { get; set; }
		internal string FullPath { get; set; }
		internal bool Success { get; set; }
		internal string Title { get; set; }
	}
}
