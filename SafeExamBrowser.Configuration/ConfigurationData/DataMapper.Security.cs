/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.Configuration.Contracts.Settings.Service;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapKioskMode(IDictionary<string, object> rawData, Settings settings)
		{
			var hasCreateNewDesktop = rawData.TryGetValue(Keys.Security.KioskModeCreateNewDesktop, out var createNewDesktop);
			var hasDisableExplorerShell = rawData.TryGetValue(Keys.Security.KioskModeDisableExplorerShell, out var disableExplorerShell);

			if (hasDisableExplorerShell && disableExplorerShell as bool? == true)
			{
				settings.KioskMode = KioskMode.DisableExplorerShell;
			}

			if (hasCreateNewDesktop && createNewDesktop as bool? == true)
			{
				settings.KioskMode = KioskMode.CreateNewDesktop;
			}

			if (hasCreateNewDesktop && hasDisableExplorerShell && createNewDesktop as bool? == false && disableExplorerShell as bool? == false)
			{
				settings.KioskMode = KioskMode.None;
			}
		}

		private void MapServicePolicy(Settings settings, object value)
		{
			const int WARN = 1;
			const int FORCE = 2;

			if (value is int policy)
			{
				settings.Service.Policy = policy == FORCE ? ServicePolicy.Mandatory : (policy == WARN ? ServicePolicy.Warn : ServicePolicy.Optional);
			}
		}
	}
}
