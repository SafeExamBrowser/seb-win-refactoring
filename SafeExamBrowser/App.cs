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

			ShowSplashScreen();
			Initialize();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			instances.ShutdownController.FinalizeApplication();

			base.OnExit(e);
		}

		private void Initialize()
		{
			instances.BuildObjectGraph();

			var success = instances.StartupController.TryInitializeApplication();

			if (success)
			{
				MainWindow = instances.Taskbar;
				MainWindow.Show();
			}
			else
			{
				Shutdown();
			}

			instances.SplashScreen?.Dispatcher.InvokeAsync(instances.SplashScreen.Close);
		}

		private void ShowSplashScreen()
		{
			var splashReadyEvent = new AutoResetEvent(false);
			var splashScreenThread = new Thread(() =>
			{
				instances.SplashScreen = new UserInterface.SplashScreen(instances.Settings);
				instances.SplashScreen.Closed += (o, args) => instances.SplashScreen.Dispatcher.InvokeShutdown();
				instances.SplashScreen.Show();

				splashReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			splashScreenThread.SetApartmentState(ApartmentState.STA);
			splashScreenThread.Name = "Splash Screen Thread";
			splashScreenThread.IsBackground = true;
			splashScreenThread.Start();

			splashReadyEvent.WaitOne();
		}
	}
}
