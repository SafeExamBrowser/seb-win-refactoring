/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.InteropServices;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.WindowsApi.Types
{
	/// <remarks>
	/// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd162897(v=vs.85).aspx.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		internal int Left;
		internal int Top;
		internal int Right;
		internal int Bottom;

		internal IBounds ToBounds()
		{
			return new Bounds
			{
				Left = Left,
				Top = Top,
				Right = Right,
				Bottom = Bottom
			};
		}
	}
}
