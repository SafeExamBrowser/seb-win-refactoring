/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.WindowsApi.Types
{
	public class StickyKeysState : IStickyKeysState
	{
		internal StickyKeysFlags Flags { get; set; }

		public bool IsAvailable { get; internal set; }
		public bool IsEnabled { get; internal set; }
		public bool IsHotkeyActive { get; internal set; }
	}
}
