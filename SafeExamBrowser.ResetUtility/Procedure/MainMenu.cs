/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class MainMenu : ProcedureStep
	{
		private IList<Option> options;

		public MainMenu(Context context) : base(context)
		{
			options = new List<Option>
			{
				new Option { IsSelected = true, NextStep = new Restore(Context), Result = ProcedureStepResult.Continue, Text = "Restore system configuration via backup mechanism" },
				new Option { NextStep = new Reset(Context), Result = ProcedureStepResult.Continue, Text = "Reset system configuration to default values" },
				new Option { NextStep = new ShowVersion(Context), Result = ProcedureStepResult.Continue, Text = "Show version information" },
				new Option { NextStep = new ShowLog(Context), Result = ProcedureStepResult.Continue, Text = "Show application log" },
				new Option { Result = ProcedureStepResult.Terminate, Text = "Exit" }
			};
		}

		internal override ProcedureStepResult Execute()
		{
			PrintMenu();

			for (var key = Console.ReadKey(true).Key; key != ConsoleKey.Enter; key = Console.ReadKey(true).Key)
			{
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
				{
					SelectNextOption(key);
					PrintMenu();
				}
			}

			return options.First(o => o.IsSelected).Result;
		}

		internal override ProcedureStep GetNextStep()
		{
			return options.First(o => o.IsSelected).NextStep;
		}

		private void PrintMenu()
		{
			InitializeConsole();

			Console.WriteLine("Please choose one of the following options:");
			Console.WriteLine();

			foreach (var option in options)
			{
				Console.WriteLine(option.ToString());
			}

			Console.WriteLine();
			Console.WriteLine("Use the up/down arrow keys and enter to navigate the menu.");
		}

		private void SelectNextOption(ConsoleKey key)
		{
			var current = options.First(o => o.IsSelected);
			var currentIndex = options.IndexOf(current);
			var nextIndex = default(int);

			if (key == ConsoleKey.UpArrow)
			{
				nextIndex = --currentIndex < 0 ? options.Count - 1 : currentIndex;
			}

			if (key == ConsoleKey.DownArrow)
			{
				nextIndex = ++currentIndex == options.Count ? 0 : currentIndex;
			}

			var next = options.ElementAt(nextIndex);

			current.IsSelected = false;
			next.IsSelected = true;
		}

		private class Option
		{
			public bool IsSelected { get; set; }
			public ProcedureStep NextStep { get; set; }
			public ProcedureStepResult Result { get; set; }
			public string Text { get; set; }

			public override string ToString()
			{
				return $"[{(IsSelected ? "x" : " ")}] {Text}";
			}
		}
	}
}
