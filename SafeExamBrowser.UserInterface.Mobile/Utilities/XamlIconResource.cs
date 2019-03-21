/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Core;

namespace SafeExamBrowser.UserInterface.Mobile.Utilities
{
	/// <summary>
	/// TODO: Move to shared library?
	/// </summary>
	internal class XamlIconResource : IIconResource
	{
		public Uri Uri { get; private set; }
		public bool IsBitmapResource => false;
		public bool IsXamlResource => true;

		public XamlIconResource(Uri uri)
		{
			Uri = uri;
		}
	}
}
