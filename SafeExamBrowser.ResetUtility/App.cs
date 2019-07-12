/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SafeExamBrowser.ResetUtility.Procedure;

namespace SafeExamBrowser.ResetUtility
{
	public class App : Application
	{
		private static readonly Mutex Mutex = new Mutex(true, "safe_exam_browser_reset_mutex");
		private CompositionRoot instances = new CompositionRoot();

		[STAThread]
		public static void Main()
		{
			try
			{
				StartApplication();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				Mutex.Close();
			}
		}

		private static void StartApplication()
		{
			if (NoInstanceRunning())
			{
				new App().Run();
			}
			else
			{
				MessageBox.Show("You can only run one instance of the Reset Utility at a time.", "Startup Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private static bool NoInstanceRunning()
		{
			return Mutex.WaitOne(TimeSpan.Zero, true);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			instances.BuildObjectGraph();
			instances.LogStartupInformation();

			MainWindow = instances.MainWindow;
			MainWindow.Show();

			Task.Run(new Action(RunApplication));
		}

		private void RunApplication()
		{
			for (var step = instances.InitialStep; ; step = step.GetNextStep())
			{
				var result = step.Execute();

				if (result == ProcedureStepResult.Terminate)
				{
					break;
				}
			}

			Dispatcher.Invoke(Shutdown);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			instances.LogShutdownInformation();

			base.OnExit(e);
		}
	}
}
