/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class ProcedureContext
	{
		internal IFeatureConfigurationFactory ConfigurationFactory { get; set; }
		internal Func<string, IFeatureConfigurationBackup> CreateBackup { get; set; }
		internal ILogger Logger { get; set; }
		internal IList<MainMenuOption> MainMenu { get; set; }
		internal ISystemConfigurationUpdate Update { get; set; }
		internal IUserInfo UserInfo { get; set; }
	}
}
