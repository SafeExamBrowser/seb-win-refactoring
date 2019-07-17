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

		protected void InitializeConsole()
		{
			var title = "SEB Reset Utility";
			var height = Console.LargestWindowHeight > 40 ? 40 : Console.LargestWindowHeight;
			var width = Console.LargestWindowWidth > 160 ? 160 : Console.LargestWindowWidth;

			Console.SetBufferSize(width, height);
			Console.SetWindowSize(width, height);
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

		protected void ShowMenu(IList<MenuOption> options, bool showInstructions = false)
		{
			var left = Console.CursorLeft;
			var top = Console.CursorTop;

			PrintMenu(options, left, top, showInstructions);

			for (var key = Console.ReadKey(true).Key; key != ConsoleKey.Enter; key = Console.ReadKey(true).Key)
			{
				if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
				{
					SelectNextOption(key, options);
					PrintMenu(options, left, top, showInstructions);
				}
			}
		}

		protected void ShowProgress(int current, int total)
		{
			var scale = Console.BufferWidth - 8.0;
			var progress = Math.Floor(current * scale / total);
			var remaining = Math.Ceiling((total - current) * scale / total);

			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write($"[{new String('■', (int) progress)}{new String('─', (int) remaining)}] {current * 100 / total}%");
		}

		private void PrintMenu(IList<MenuOption> options, int left, int top, bool showInstructions)
		{
			Console.SetCursorPosition(left, top);

			if (showInstructions)
			{
				Console.WriteLine("Please choose one of the following options:");
				Console.WriteLine();
			}

			foreach (var option in options)
			{
				Console.WriteLine(option.ToString());
			}

			if (showInstructions)
			{
				Console.WriteLine();
				Console.WriteLine("Use the up/down arrow keys and enter to navigate the menu.");
			}
		}

		private void SelectNextOption(ConsoleKey key, IList<MenuOption> options)
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
	}
}
