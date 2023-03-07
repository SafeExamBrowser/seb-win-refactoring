/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Delegates
{
	/// <remarks>
	/// See https://docs.microsoft.com/de-de/windows/desktop/api/winuser/nc-winuser-hookproc
	/// </remarks>
	internal delegate IntPtr HookDelegate(int nCode, IntPtr wParam, IntPtr lParam);
}
