/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts
{
	/// <summary>
	/// Defines the data format of an icon resource.
	/// </summary>
	public enum IconResourceType
	{
		/// <summary>
		/// The icon resource is a bitmap image (i.e. raster graphics).
		/// </summary>
		Bitmap,

		/// <summary>
		/// The icon resource is a file with embedded icon data (e.g. an executable).
		/// </summary>
		Embedded,

		/// <summary>
		/// The icon resource consists of XAML markup (i.e. vector graphics).
		/// </summary>
		Xaml
	}
}
