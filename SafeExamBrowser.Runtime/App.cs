/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using SafeExamBrowser.Contracts.Behaviour;

namespace SafeExamBrowser.Runtime
{
	public class App : Application
	{
		private static readonly Mutex Mutex = new Mutex(true, "safe_exam_browser_runtime_mutex");
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
				MessageBox.Show("You can only run one instance of SEB at a time.", "Startup Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
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
			LogStartupInformation();

			var success = instances.StartupController.TryInitializeApplication(instances.StartupOperations);

			if (success)
			{
				// TODO: Probably needs new window to display status of running application...
				//MainWindow = instances.SplashScreen;
				//MainWindow.Closing += MainWindow_Closing;
				//MainWindow.Show();
			}
			else
			{
				Shutdown();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			instances.Logger?.Log($"{Environment.NewLine}# Application terminated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

			base.OnExit(e);
		}

		private void LogStartupInformation()
		{
			var runtimeInfo = instances.RuntimeInfo;
			var logger = instances.Logger;
			var titleLine = $"/* {runtimeInfo.ProgramTitle}, Version {runtimeInfo.ProgramVersion}{Environment.NewLine}";
			var copyrightLine = $"/* {runtimeInfo.ProgramCopyright}{Environment.NewLine}";
			var emptyLine = $"/* {Environment.NewLine}";
			var githubLine = $"/* Please visit https://github.com/SafeExamBrowser for more information.";

			logger.Log($"{titleLine}{copyrightLine}{emptyLine}{githubLine}");
			logger.Log(string.Empty);
			logger.Log($"# Application started at {runtimeInfo.ApplicationStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			logger.Log($"# Running on {instances.SystemInfo.OperatingSystemInfo}");
			logger.Log(string.Empty);
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			var operations = new Queue<IOperation>(instances.StartupOperations.Reverse());

			MainWindow.Hide();
			instances.ShutdownController.FinalizeApplication(operations);
		}
	}
}
