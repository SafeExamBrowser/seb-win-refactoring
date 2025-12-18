/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts.Cryptography;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal class IntegrityResponsibility : BrowserResponsibility
	{
		private readonly IKeyGenerator keyGenerator;

		public IntegrityResponsibility(BrowserApplicationContext context, IKeyGenerator keyGenerator) : base(context)
		{
			this.keyGenerator = keyGenerator;
		}

		public override void Assume(BrowserTask task)
		{
			if (task == BrowserTask.InitializeIntegrity)
			{
				InitializeIntegrityKeys();
			}
		}

		private void InitializeIntegrityKeys()
		{
			Logger.Debug($"Browser Exam Key (BEK) transmission is {(Settings.SendBrowserExamKey ? "enabled" : "disabled")}.");
			Logger.Debug($"Configuration Key (CK) transmission is {(Settings.SendConfigurationKey ? "enabled" : "disabled")}.");

			if (Settings.CustomBrowserExamKey != default)
			{
				keyGenerator.UseCustomBrowserExamKey(Settings.CustomBrowserExamKey);
				Logger.Debug($"The browser application will be using a custom browser exam key.");
			}
			else
			{
				Logger.Debug($"The browser application will be using the default browser exam key.");
			}
		}
	}
}
