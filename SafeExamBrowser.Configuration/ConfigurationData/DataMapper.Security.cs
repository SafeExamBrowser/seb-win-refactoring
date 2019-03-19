/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Settings;

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
	}
}
