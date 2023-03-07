/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Reflection;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class Version : ProcedureStep
	{
		public Version(ProcedureContext context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			var executable = Assembly.GetExecutingAssembly();
			var build = executable.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
			var copyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			var version = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

			InitializeConsole();

			Console.ForegroundColor = ConsoleColor.DarkBlue;
			Console.WriteLine($"Safe Exam Browser, Version {version}");
			Console.WriteLine($"Build {build}");
			Console.WriteLine(copyright.Replace("©", "(c)"));
			Console.WriteLine();
			Console.ForegroundColor = ForegroundColor;
			Console.WriteLine("Press any key to return to the main menu.");
			Console.ReadKey();

			return ProcedureStepResult.Continue;
		}

		internal override ProcedureStep GetNextStep()
		{
			return new MainMenu(Context);
		}
	}
}
