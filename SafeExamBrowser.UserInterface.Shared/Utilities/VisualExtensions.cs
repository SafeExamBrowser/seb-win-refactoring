/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	/// <summary>
	/// WPF works with device-independent pixels. These methods are required to transform
	/// such values to their absolute, device-specific pixel values and vice versa.
	/// 
	/// Source: https://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels
	/// </summary>
	public static class VisualExtensions
	{
		public static Vector TransformToPhysical(this Visual visual, double x, double y)
		{
			var matrix = default(Matrix);
			var source = PresentationSource.FromVisual(visual);

			if (source != null)
			{
				matrix = source.CompositionTarget.TransformToDevice;
			}
			else
			{
				using (var newSource = new HwndSource(new HwndSourceParameters()))
				{
					matrix = newSource.CompositionTarget.TransformToDevice;
				}
			}

			return matrix.Transform(new Vector(x, y));
		}

		public static Vector TransformFromPhysical(this Visual visual, double x, double y)
		{
			var matrix = default(Matrix);
			var source = PresentationSource.FromVisual(visual);

			if (source != null)
			{
				matrix = source.CompositionTarget.TransformFromDevice;
			}
			else
			{
				using (var newSource = new HwndSource(new HwndSourceParameters()))
				{
					matrix = newSource.CompositionTarget.TransformFromDevice;
				}
			}

			return matrix.Transform(new Vector(x, y));
		}
	}
}
