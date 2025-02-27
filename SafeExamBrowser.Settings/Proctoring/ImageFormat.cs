/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Proctoring
{
	/// <summary>
	/// Defines all possible image formats for the screen proctoring.
	/// </summary>
	public enum ImageFormat
	{
		/// <summary>
		/// An image with the Windows Bitmap format.
		/// </summary>
		Bmp,

		/// <summary>
		/// An image with the Graphics Interchange Format format.
		/// </summary>
		Gif,

		/// <summary>
		/// An image with the Joint Photographic Experts Group format.
		/// </summary>
		Jpg,

		/// <summary>
		/// An image with the Portable Network Graphics format.
		/// </summary>
		Png
	}
}
