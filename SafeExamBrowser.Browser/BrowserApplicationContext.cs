/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using CefSharp.WinForms;
using SafeExamBrowser.Browser.Responsibilities;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser
{
	internal class BrowserApplicationContext
	{
		internal AppConfig AppConfig { get; set; }
		internal CefSettings CefSettings { get; set; }
		internal IModuleLogger Logger { get; set; }
		internal IResponsibilityCollection<BrowserTask> Responsibilities { get; set; }
		internal BrowserSettings Settings { get; set; }
		internal IList<BrowserWindow> Windows { get; set; }

		internal BrowserApplicationContext()
		{
			Windows = new List<BrowserWindow>();
		}
	}
}
