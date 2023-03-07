/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.ResetUtility.Procedure;

namespace SafeExamBrowser.ResetUtility
{
	public class Program
	{
		public static void Main(string[] args)
		{
			new Program().Run();
		}

		public void Run()
		{
			var instances = new CompositionRoot();

			instances.BuildObjectGraph();
			instances.LogStartupInformation();
			instances.NativeMethods.TryDisableSystemMenu();

			for (var step = instances.InitialStep; ; step = step.GetNextStep())
			{
				var result = step.Execute();

				if (result == ProcedureStepResult.Terminate)
				{
					break;
				}
			}

			instances.LogShutdownInformation();
		}
	}
}
