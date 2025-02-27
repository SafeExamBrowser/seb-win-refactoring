/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Imaging
{
	internal class ScreenShot : IDisposable
	{
		internal DateTime CaptureTime { get; set; }
		internal byte[] Data { get; set; }
		internal ImageFormat Format { get; set; }
		internal int Height { get; set; }
		internal int Width { get; set; }

		public void Dispose()
		{
			Data = default;
		}

		public override string ToString()
		{
			return $"captured: {CaptureTime}, format: {Format.ToString().ToUpper()}, resolution: {Width}x{Height}, size: {Data.Length / 1000:N0}kB";
		}
	}
}
