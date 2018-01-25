/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

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
			instances.LogStartupInformation();

			var success = instances.RuntimeController.TryInitializeApplication(instances.StartupOperations);

			if (success)
			{
				MainWindow = instances.RuntimeWindow;
				MainWindow.Closing += MainWindow_Closing;
				MainWindow.Show();
			}
			else
			{
				Shutdown();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			instances.LogShutdownInformation();

			base.OnExit(e);
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			MainWindow.Hide();
			instances.RuntimeController.FinalizeApplication();
		}
	}
}
