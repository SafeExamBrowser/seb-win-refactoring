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
	/// Defines all possible image quantization algorithms for the screen proctoring.
	/// </summary>
	public enum ImageQuantization
	{
		/// <summary>
		/// Reduces an image to a black and white image with 1 bit per pixel.
		/// </summary>
		BlackAndWhite1bpp,

		/// <summary>
		/// Reduces an image to a colored image with 8 bits per pixel (256 colors).
		/// </summary>
		Color8bpp,

		/// <summary>
		/// Reduces an image to a colored image with 16 bits per pixel (5 bits per color and the remaining bit unused, thus 32'768 colors).
		/// </summary>
		Color16bpp,

		/// <summary>
		/// Reduces an image to a colored image with 24 bits per pixel (16'777'216 colors).
		/// </summary>
		Color24bpp,

		/// <summary>
		/// Reduces an image to a grayscale image with 2 bits per pixel (4 shades).
		/// </summary>
		Grayscale2bpp,

		/// <summary>
		/// Reduces an image to a grayscale image with 4 bits per pixel (16 shades).
		/// </summary>
		Grayscale4bpp,

		/// <summary>
		/// Reduces an image to a grayscale image with 8 bits per pixel (256 shades).
		/// </summary>
		Grayscale8bpp
	}
}
