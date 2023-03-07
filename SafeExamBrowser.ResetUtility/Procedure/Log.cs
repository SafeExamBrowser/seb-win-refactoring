/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class Log : ProcedureStep
	{
		public Log(ProcedureContext context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			var log = Logger.GetLog();
			var offset = 0;

			for (var key = default(ConsoleKey); key != ConsoleKey.Enter; key = Console.ReadKey(true).Key)
			{
				offset = UpdateOffset(key, offset, log.Count);

				InitializeConsole();
				PrintInstructions();
				PrintLogSection(log, offset);
			}

			return ProcedureStepResult.Continue;
		}

		internal override ProcedureStep GetNextStep()
		{
			return new MainMenu(Context);
		}

		private void PrintInstructions()
		{
			Console.WriteLine("Use the up/down arrow keys to scroll. Press enter to return to the main menu.");
			Console.WriteLine();
		}

		private void PrintLogSection(IList<ILogContent> log, int offset)
		{
			var count = 0;

			foreach (var item in log)
			{
				if (offset > log.IndexOf(item))
				{
					continue;
				}

				if (item is ILogMessage message)
				{
					PrintMessage(message);
				}

				if (item is ILogText text)
				{
					PrintText(text);
				}

				count++;

				if (Console.CursorTop >= Console.BufferHeight - 3)
				{
					break;
				}
			}

			Console.SetCursorPosition(0, Console.BufferHeight - 3);
			Console.WriteLine();
			Console.WriteLine($"Showing entries {offset + 1} - {offset + count} of total {log.Count}.");
		}

		private int UpdateOffset(ConsoleKey key, int offset, int total)
		{
			if (key == ConsoleKey.DownArrow)
			{
				offset = offset == total - 1 ? offset : offset + 1;
			}

			if (key == ConsoleKey.UpArrow)
			{
				offset = offset == 0 ? offset : offset - 1;
			}

			return offset;
		}

		private void PrintMessage(ILogMessage message)
		{
			var date = message.DateTime.ToString("HH:mm:ss.fff");
			var severity = message.Severity.ToString().ToUpper();
			var threadId = message.ThreadInfo.Id < 10 ? $"0{message.ThreadInfo.Id}" : message.ThreadInfo.Id.ToString();
			var threadName = message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty;
			var threadInfo = $"[{threadId}{threadName}]";

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write($"{date} {threadInfo} - ");
			Console.ForegroundColor = GetColorFor(message.Severity);
			Console.WriteLine($"{severity}: { message.Message}");
			Console.ForegroundColor = ForegroundColor;
		}

		private void PrintText(ILogText text)
		{
			var isHeader = text.Text.StartsWith("/* ");
			var isComment = text.Text.StartsWith("# ");

			if (isHeader || isComment)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
			}

			Console.WriteLine(text.Text);
			Console.ForegroundColor = ForegroundColor;
		}

		private ConsoleColor GetColorFor(LogLevel severity)
		{
			switch (severity)
			{
				case LogLevel.Debug:
					return ConsoleColor.DarkGray;
				case LogLevel.Error:
					return ConsoleColor.Red;
				case LogLevel.Warning:
					return ConsoleColor.DarkYellow;
				default:
					return ConsoleColor.DarkBlue;
			}
		}
	}
}
