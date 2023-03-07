/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Client
{
	public class App : Application
	{
		private const int ILMCM_CHECKLAYOUTANDTIPENABLED = 0x00001;
		private const int ILMCM_LANGUAGEBAROFF = 0x00002;

		private static readonly Mutex Mutex = new Mutex(true, AppConfig.CLIENT_MUTEX_NAME);
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

			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			// We need to manually initialize a monitor in order to prevent Windows from automatically doing so and thus rendering an input lanuage
			// switch in the bottom right corner of the desktop. This must be done before any UI element is initialized or rendered on the screen.
			InitLocalMsCtfMonitor(ILMCM_CHECKLAYOUTANDTIPENABLED | ILMCM_LANGUAGEBAROFF);

			instances.BuildObjectGraph(Shutdown);
			instances.LogStartupInformation();

			var success = instances.ClientController.TryStart();

			if (!success)
			{
				Shutdown();
			}
		}

		public new void Shutdown()
		{
			void shutdown()
			{
				instances.ClientController.Terminate();
				instances.LogShutdownInformation();

				UninitLocalMsCtfMonitor();

				base.Shutdown();
			}

			Dispatcher.InvokeAsync(shutdown);
		}

		[DllImport("MsCtfMonitor.dll", SetLastError = true)]
		private static extern IntPtr InitLocalMsCtfMonitor(int dwFlags);

		[DllImport("MsCtfMonitor.dll", SetLastError = true)]
		private static extern IntPtr UninitLocalMsCtfMonitor();
	}
}
