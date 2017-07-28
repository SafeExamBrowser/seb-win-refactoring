/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using SafeExamBrowser.Contracts.Behaviour;

namespace SafeExamBrowser
{
	public class App : Application
	{
		private static readonly Mutex Mutex = new Mutex(true, "safe_exam_browser_single_instance_mutex");
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

			var success = instances.StartupController.TryInitializeApplication(instances.StartupOperations);

			if (success)
			{
				MainWindow = instances.Taskbar;
				MainWindow.Closing += (o, args) => ShutdownApplication();
				MainWindow.Show();
			}
			else
			{
				Shutdown();
			}
		}

		private void ShutdownApplication()
		{
			var operations = new Queue<IOperation>(instances.StartupOperations.Reverse());

			MainWindow.Hide();
			instances.ShutdownController.FinalizeApplication(operations);
		}
	}
}
