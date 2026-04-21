/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Monitoring.Contracts.Applications;

namespace SafeExamBrowser.Monitoring.Applications
{
	internal class Window : IWindow
	{
		internal bool IsOverlay { get; set; }

		public IntPtr Handle { get; set; }
		public string Title { get; set; }

		public override string ToString()
		{
			return $"'{Title}' ({Handle}{(IsOverlay ? ", Overlay" : "")})";
		}
	}
}
