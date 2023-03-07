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
	/// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms682612(v=vs.85).aspx
	/// </remarks>
	internal delegate bool EnumDesktopDelegate(string lpszDesktop, IntPtr lParam);
}
