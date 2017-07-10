/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Windows;

namespace SafeExamBrowser
{
	public class App : Application
	{
		private static readonly Mutex mutex = new Mutex(true, "safe_exam_browser_single_instance_mutex");

		private CompositionRoot instances;

		[STAThread]
		public static void Main()
		{
			try
			{
				StartApplication();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Fatal Error");
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
				MessageBox.Show("You can only run one instance of SEB at a time.", "Startup Not Allowed");
			}
		}

		private static bool NoInstanceRunning()
		{
			return mutex.WaitOne(TimeSpan.Zero, true);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var initializationThread = new Thread(Initialize);

			instances = new CompositionRoot();
			instances.BuildObjectGraph();
			instances.SplashScreen.Show();

			MainWindow = instances.SplashScreen;

			initializationThread.SetApartmentState(ApartmentState.STA);
			initializationThread.Name = "Initialization Thread";
			initializationThread.Start();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			instances.ShutdownController.FinalizeApplication();

			base.OnExit(e);
		}

		private void Initialize()
		{
			var success = instances.StartupController.TryInitializeApplication();

			if (success)
			{
				instances.Taskbar.Dispatcher.Invoke(() =>
				{
					MainWindow = instances.Taskbar;
					MainWindow.Show();
				});
			}

			//instances.SplashScreen.Dispatcher.Invoke(instances.SplashScreen.Close);
		}
	}
}
