/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Input;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Data
{
	internal class KeyboardTrigger
	{
		public Key Key { get; internal set; }
		public KeyModifier Modifier { get; internal set; }
		public KeyState State { get; internal set; }
	}
}
