/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class ScreenChecker : IChecker
	{
		private readonly int minWidth;
		private readonly int minHeight;

		public string Name => "Monitor Screen Resolution";

		public ScreenChecker(int minWidth = 1024, int minHeight = 768)
		{
			this.minWidth = minWidth;
			this.minHeight = minHeight;
		}

		public CheckResult Check()
		{
			var bounds = Screen.PrimaryScreen.Bounds;
			int width = bounds.Width;
			int height = bounds.Height;
			bool passed = width >= minWidth && height >= minHeight;

			return new CheckResult
			{
				Category = "Screen",
				Title = Name,
				Status = passed ? CheckStatus.Passed : CheckStatus.Failed,
				ActualValue = $"{width} x {height}",
				RequiredValue = $"≥ {minWidth} x {minHeight}",
				Message = passed ? "Monitor screen resolution is sufficient." : $"Monitor screen resolution is too low (minimum {minWidth}x{minHeight}).",
				IsCritical = true
			};
		}
	}
}
