/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal abstract class ProcedureStep
	{
		private CancellationTokenSource progressAnimationToken;
		private Task progressAnimation;

		protected ProcedureContext Context { get; }
		protected ILogger Logger => Context.Logger;
		protected ConsoleColor BackgroundColor => ConsoleColor.White;
		protected ConsoleColor ForegroundColor => ConsoleColor.Black;

		internal ProcedureStep(ProcedureContext context)
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

		protected void ClearLine(int top)
		{
			Console.SetCursorPosition(0, top);
			Console.WriteLine(new String(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, top);
		}

		protected string ReadLine()
		{
			Console.Write("> ");
			Console.CursorVisible = true;

			var input = Console.ReadLine();

			Console.CursorVisible = false;

			return input;
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

		protected void StartProgressAnimation()
		{
			progressAnimationToken = new CancellationTokenSource();
			progressAnimation = Task.Run(new Action(ProgressAnimation));
		}

		protected void StopProgressAnimation()
		{
			progressAnimationToken?.Cancel();
			progressAnimation?.Wait();
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

		private void ProgressAnimation()
		{
			var length = 12;
			var max = Console.BufferWidth - 8;
			var min = 1;
			var left = 1;
			var operand = 1;

			Console.Write($"[{new String('■', length)}{new String('─', max - length)}]");

			while (!progressAnimationToken.IsCancellationRequested)
			{
				Console.SetCursorPosition(left, Console.CursorTop);
				Console.Write(new String('─', length));

				if (left + length > max)
				{
					operand = -1;
				}
				else if (left <= min)
				{
					operand = 1;
				}

				left += operand;

				Console.SetCursorPosition(left, Console.CursorTop);
				Console.Write(new String('■', length));

				Thread.Sleep(20);
			}

			Console.SetCursorPosition(0, Console.CursorTop);
			Console.WriteLine(new String(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, Console.CursorTop - 2);
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
