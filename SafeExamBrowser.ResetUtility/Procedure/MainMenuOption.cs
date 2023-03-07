/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class MainMenuOption : MenuOption
	{
		internal ProcedureStep NextStep { get; set; }
		internal ProcedureStepResult Result { get; set; }
	}
}
