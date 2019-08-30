/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.Browser
{
	public class BrowserIconResource : IIconResource
	{
		public Uri Uri { get; private set; }
		public bool IsBitmapResource => true;
		public bool IsXamlResource => false;

		public BrowserIconResource(string uri = null)
		{
			Uri = new Uri(uri ?? "pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/SafeExamBrowser.ico");
		}
	}
}
