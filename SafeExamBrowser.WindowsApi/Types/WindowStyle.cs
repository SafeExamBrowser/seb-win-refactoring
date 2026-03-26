/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.WindowsApi.Types
{
	internal class WindowStyle : IWindowStyle
	{
		private readonly WindowStyles styles;
		private readonly ExtendedWindowStyles extendedStyles;

		public bool IsDisabled => styles.HasFlag(WindowStyles.WS_DISABLED);
		public bool IsNotActivatable => extendedStyles.HasFlag(ExtendedWindowStyles.WS_EX_NOACTIVATE);
		public bool IsTopmost => extendedStyles.HasFlag(ExtendedWindowStyles.WS_EX_TOPMOST);
		public bool IsVisible => styles.HasFlag(WindowStyles.WS_VISIBLE);

		public WindowStyle(WindowStyles styles, ExtendedWindowStyles extendedStyles)
		{
			this.styles = styles;
			this.extendedStyles = extendedStyles;
		}

		public IEnumerable<string> GetRawStyles()
		{
			foreach (var style in Enum.GetValues(styles.GetType()).Cast<WindowStyles>())
			{
				if (styles.HasFlag(style))
				{
					yield return style.ToString();
				}
			}

			foreach (var style in Enum.GetValues(extendedStyles.GetType()).Cast<ExtendedWindowStyles>())
			{
				if (extendedStyles.HasFlag(style))
				{
					yield return style.ToString();
				}
			}
		}
	}
}
