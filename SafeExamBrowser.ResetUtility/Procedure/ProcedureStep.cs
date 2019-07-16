/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal abstract class ProcedureStep
	{
		protected Context Context { get; }
		protected ILogger Logger => Context.Logger;
		protected ConsoleColor BackgroundColor => ConsoleColor.White;
		protected ConsoleColor ForegroundColor => ConsoleColor.Black;

		internal ProcedureStep(Context context)
		{
			Context = context;
		}

		internal abstract ProcedureStepResult Execute();
		internal abstract ProcedureStep GetNextStep();

		protected void Initialize()
		{
			var title = "SEB Reset Utility";

			Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
			Console.SetWindowSize(Console.BufferWidth, Console.BufferHeight);
			Console.BackgroundColor = BackgroundColor;
			Console.ForegroundColor = ForegroundColor;
			Console.Clear();
			Console.BackgroundColor = ConsoleColor.Gray;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write(new String(' ', Console.BufferWidth));
			Console.Write(new String(' ', (int) Math.Floor((Console.BufferWidth - title.Length) / 2.0)));
			Console.Write(title);
			Console.Write(new String(' ', (int) Math.Ceiling((Console.BufferWidth - title.Length) / 2.0)));
			Console.Write(new String(' ', Console.BufferWidth));
			Console.BackgroundColor = BackgroundColor;
			Console.ForegroundColor = ForegroundColor;
			Console.SetCursorPosition(0, 4);
			Console.CursorVisible = false;
		}

		protected void ShowError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ForegroundColor = ForegroundColor;
			Console.ReadKey();
		}
	}
}
