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
using SafeExamBrowser.Lockdown.Contracts;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class Reset : ProcedureStep
	{
		public Reset(ProcedureContext context) : base(context)
		{
		}

		internal override ProcedureStepResult Execute()
		{
			InitializeConsole();

			var success = TryGetUserInfo(out var userName, out var sid);

			if (success)
			{
				ResetAll(userName, sid);
			}

			return ProcedureStepResult.Continue;
		}

		internal override ProcedureStep GetNextStep()
		{
			return new MainMenu(Context);
		}

		private bool TryGetUserInfo(out string userName, out string sid)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("IMPORTANT: Some configuration values are user specific. In order to reset these values, the user specified below needs to be logged in!");
			Console.ForegroundColor = ForegroundColor;
			Console.WriteLine();
			Console.WriteLine("Please enter the name of the user for which to reset all configuration values:");

			userName = ReadLine();

			StartProgressAnimation();
			var success = Context.UserInfo.TryGetSidForUser(userName, out sid);
			StopProgressAnimation();

			while (!success)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Could not find user '{userName}'!");
				Console.ForegroundColor = ForegroundColor;

				var tryAgain = new MenuOption { IsSelected = true, Text = "Try again" };
				var mainMenu = new MenuOption { Text = "Return to main menu" };

				ShowMenu(new List<MenuOption> { tryAgain, mainMenu });

				if (mainMenu.IsSelected)
				{
					break;
				}

				ClearLine(Console.CursorTop - 1);
				ClearLine(Console.CursorTop - 1);
				ClearLine(Console.CursorTop - 1);
				ClearLine(Console.CursorTop - 1);

				userName = ReadLine();
				success = Context.UserInfo.TryGetSidForUser(userName, out sid);
			}

			return success;
		}

		private void ResetAll(string userName, string sid)
		{
			var configurations = Context.ConfigurationFactory.CreateAll(Guid.NewGuid(), sid, userName);
			var failed = new List<IFeatureConfiguration>();

			Logger.Info($"Attempting to reset all configuration values for user '{userName}' with SID '{sid}'...");
			Console.WriteLine();
			Console.WriteLine("Initiating reset procedure...");

			foreach (var configuration in configurations)
			{
				var success = configuration.Reset();

				if (!success)
				{
					failed.Add(configuration);
				}

				ShowProgress(configurations.IndexOf(configuration) + 1, configurations.Count);
			}

			PerformUpdate();

			if (failed.Any())
			{
				HandleFailure(failed);
			}
			else
			{
				HandleSuccess();
			}

			Console.WriteLine();
			Console.WriteLine("Press any key to return to the main menu.");
			Console.ReadKey();
		}

		private void PerformUpdate()
		{
			Console.WriteLine();
			Console.WriteLine("Performing system configuration update, please wait...");
			StartProgressAnimation();
			Context.Update.Execute();
			StopProgressAnimation();
			Console.WriteLine("Update completed.");
		}

		private void HandleFailure(IList<IFeatureConfiguration> configurations)
		{
			Logger.Warn($"Failed to reset {configurations.Count} items!");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Failed to reset {configurations.Count} items!");

			foreach (var configuration in configurations)
			{
				Console.WriteLine($" - {configuration.GetType().Name}");
			}

			Console.ForegroundColor = ForegroundColor;
			Console.WriteLine();
			Console.WriteLine("Please consult the application log for more information.");
		}

		private void HandleSuccess()
		{
			Logger.Info("Successfully reset all changes!");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Successfully reset all changes!");
			Console.ForegroundColor = ForegroundColor;
		}
	}
}
