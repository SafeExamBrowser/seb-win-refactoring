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
	internal class ShowLog : ProcedureStep
	{
		public ShowLog(Context context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			Initialize();

			foreach (var item in Logger.GetLog())
			{
				if (item is ILogMessage message)
				{
					PrintMessage(message);
				}

				if (item is ILogText text)
				{
					PrintText(text);
				}
			}

			Console.WriteLine();
			Console.WriteLine("Press any key to return to the main menu.");
			Console.ReadKey(true);

			return ProcedureStepResult.Continue;
		}

		internal override ProcedureStep GetNextStep()
		{
			return new MainMenu(Context);
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
					return ConsoleColor.Gray;
				case LogLevel.Error:
					return ConsoleColor.Red;
				case LogLevel.Warning:
					return ConsoleColor.DarkYellow;
				default:
					return ForegroundColor;
			}
		}
	}
}
