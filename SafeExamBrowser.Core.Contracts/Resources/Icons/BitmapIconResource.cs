/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Core.Contracts.Resources.Icons
{
	/// <summary>
	/// Defines an icon resource which is a bitmap image (i.e. raster graphics).
	/// </summary>
	public class BitmapIconResource : IconResource
	{
		/// <summary>
		/// The <see cref="System.Uri"/> pointing to the image file.
		/// </summary>
		public Uri Uri { get; set; }
	}
}
