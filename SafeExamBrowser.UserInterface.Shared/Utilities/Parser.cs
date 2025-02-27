/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Media;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class Parser
	{
		public static bool TryParseBrush(string hexColorCode, out Brush brush)
		{
			brush = default;

			try
			{
				brush = new BrushConverter().ConvertFromString(hexColorCode) as Brush;
			}
			catch
			{
			}

			return brush != default;
		}
	}
}
