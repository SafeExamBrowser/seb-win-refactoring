﻿/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;

namespace SafeExamBrowser.Browser.Wrapper.Events
{
	internal delegate void BeforeBrowseEventHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect, GenericEventArgs args);
}