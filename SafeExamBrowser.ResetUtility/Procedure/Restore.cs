/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Lockdown.Contracts;

namespace SafeExamBrowser.ResetUtility.Procedure
{
	internal class Restore : ProcedureStep
	{
		private ProcedureStep next;

		public Restore(ProcedureContext context) : base(context)
		{
			next = new MainMenu(Context);
		}

		internal override ProcedureStepResult Execute()
		{
			var filePath = $@"config\systemprofile\AppData\Local\{nameof(SafeExamBrowser)}\{AppConfig.BACKUP_FILE_NAME}";
			var x86FilePath = Environment.ExpandEnvironmentVariables($@"%WINDIR%\system32\{filePath}");
			var x64FilePath = Environment.ExpandEnvironmentVariables($@"%WINDIR%\SysWOW64\{filePath}");

			InitializeConsole();

			Logger.Info("Searching backup file...");
			Logger.Debug($"x86 path => {x86FilePath}");
			Logger.Debug($"x64 path => {x64FilePath}");
			Console.WriteLine("Searching backup file...");

			if (File.Exists(x86FilePath))
			{
				RestoreBackup(x86FilePath);
			}
			else if (File.Exists(x64FilePath))
			{
				RestoreBackup(x64FilePath);
			}
			else
			{
				HandleNoBackupFile();
			}

			return ProcedureStepResult.Continue;
		}

		internal override ProcedureStep GetNextStep()
		{
			return next;
		}

		private void RestoreBackup(string filePath)
		{
			var backup = Context.CreateBackup(filePath);
			var configurations = backup.GetAllConfigurations();
			var failed = new List<IFeatureConfiguration>();

			Logger.Info($"Found backup file '{filePath}' with {configurations.Count} items. Initiating restore procedure...");
			Console.WriteLine($"Found backup file with {configurations.Count} items.");
			Console.WriteLine();
			Console.WriteLine("Initiating restore procedure...");

			foreach (var configuration in configurations)
			{
				var success = configuration.Restore();

				if (success)
				{
					backup.Delete(configuration);
				}
				else
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
			Logger.Warn($"Failed to restore {configurations.Count} items!");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Failed to restore {configurations.Count} items!");

			foreach (var configuration in configurations)
			{
				Console.WriteLine($" - {configuration.GetType().Name}");
			}

			Console.ForegroundColor = ForegroundColor;
			Console.WriteLine();
			Console.WriteLine("Some configuration values may be user specific. In order to restore these values, the user who used SEB needs to be logged in.");
		}

		private void HandleSuccess()
		{
			Logger.Info("Successfully restored all changes!");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Successfully restored all changes!");
			Console.ForegroundColor = ForegroundColor;
		}

		private void HandleNoBackupFile()
		{
			var yes = new MenuOption { IsSelected = true, Text = "Yes" };
			var no = new MenuOption { Text = "No" };

			Logger.Warn("Could not find any backup file!");

			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("Could not find any backup file!");
			Console.ForegroundColor = ForegroundColor;
			Console.WriteLine();
			Console.WriteLine("Would you like to reset all configuration values possibly changed by SEB?");

			ShowMenu(new List<MenuOption> { yes, no });

			if (yes.IsSelected)
			{
				next = new Reset(Context);
			}

			Logger.Info($"The user chose {(yes.IsSelected ? "" : "not ")}to perform a reset.");
		}
	}
}
