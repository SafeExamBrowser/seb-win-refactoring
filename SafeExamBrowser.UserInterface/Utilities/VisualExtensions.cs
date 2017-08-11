/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SafeExamBrowser.UserInterface.Utilities
{
	internal static class VisualExtensions
	{
		/// <summary>
		/// WPF works with device-independent pixels. This method is required to
		/// transform such values to their absolute, device-specific pixel value.
		/// Source: https://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels
		/// </summary>
		internal static Vector TransformToPhysical(this Visual visual, double x, double y)
		{
			Matrix transformToDevice;
			var source = PresentationSource.FromVisual(visual);

			if (source != null)
			{
				transformToDevice = source.CompositionTarget.TransformToDevice;
			}
			else
			{
				using (var newSource = new HwndSource(new HwndSourceParameters()))
				{
					transformToDevice = newSource.CompositionTarget.TransformToDevice;
				}
			}

			return transformToDevice.Transform(new Vector(x, y));
		}
	}
}
