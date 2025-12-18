/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	internal class BrowserWindowContext
	{
		internal IBrowserControl Control { get; set; }
		internal IHashAlgorithm HashAlgorithm { get; set; }
		internal BrowserIconResource Icon { get; set; }
		internal int Id { get; set; }
		internal bool IsMainWindow { get; set; }
		internal ILifeSpanHandler LifeSpanHandler { get; set; }
		internal IModuleLogger Logger { get; set; }
		internal IMessageBox MessageBox { get; set; }
		internal BrowserSettings Settings { get; set; }
		internal string StartUrl { get; set; }
		internal IText Text { get; set; }
		internal string Title { get; set; }
		internal string Url { get; set; }
		internal IUserInterfaceFactory UserInterfaceFactory { get; set; }
		internal IBrowserWindow Window { get; set; }
	}
}
