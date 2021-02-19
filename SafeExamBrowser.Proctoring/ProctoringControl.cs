/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.Web.WebView2.Wpf;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;

namespace SafeExamBrowser.Proctoring
{
	internal class ProctoringControl : WebView2, IProctoringControl
	{
		internal ProctoringControl()
		{
			Source = new Uri("https://www.microsoft.com");
		}
	}
}
