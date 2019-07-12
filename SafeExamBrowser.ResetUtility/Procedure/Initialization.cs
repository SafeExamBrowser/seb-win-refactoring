/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class Initialization : ProcedureStep
	{
		public Initialization(AppContext context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			System.Threading.Thread.Sleep(5000);

			if (SebNotRunning() && IsAdmin())
			{
				return ProcedureStepResult.Continue;
			}
			else
			{
				return ProcedureStepResult.Terminate;
			}
		}

		internal override ProcedureStep GetNextStep()
		{
			return new MainMenu(Context);
		}

		private bool IsAdmin()
		{
			return false;
		}

		private bool SebNotRunning()
		{
			return false;
		}
	}
}
