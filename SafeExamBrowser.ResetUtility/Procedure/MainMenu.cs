/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class MainMenu : ProcedureStep
	{
		public MainMenu(ProcedureContext context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			InitializeConsole();
			ShowMenu(Context.MainMenu.Cast<MenuOption>().ToList(), true);

			return Context.MainMenu.First(o => o.IsSelected).Result;
		}

		internal override ProcedureStep GetNextStep()
		{
			return Context.MainMenu.First(o => o.IsSelected).NextStep;
		}
	}
}
