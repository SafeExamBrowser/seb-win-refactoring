/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Defines an icon resource, i.e. the path to and type of an icon.
	/// </summary>
	public interface IIconResource
	{
		/// <summary>
		/// The <see cref="System.Uri"/> pointing to the icon.
		/// </summary>
		Uri Uri { get; }

		/// <summary>
		/// Indicates whether the icon resource consists of a bitmap image (i.e. raster graphics).
		/// </summary>
		bool IsBitmapResource { get; }

		/// <summary>
		/// Indicates whether the icon resource consists of XAML markup (i.e. vector graphics).
		/// </summary>
		bool IsXamlResource { get; }
	}
}
