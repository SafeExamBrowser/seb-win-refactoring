/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal abstract class BrowserResponsibility : IResponsibility<BrowserTask>
	{
		protected BrowserApplicationContext Context { get; }

		protected AppConfig AppConfig => Context.AppConfig;
		protected IModuleLogger Logger => Context.Logger;
		protected BrowserSettings Settings => Context.Settings;

		internal BrowserResponsibility(BrowserApplicationContext context)
		{
			Context = context;
		}

		public abstract void Assume(BrowserTask task);
	}
}