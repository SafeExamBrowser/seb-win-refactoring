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
using SafeExamBrowser.Contracts.I18n;

namespace SafeExamBrowser
{
	public class App : Application
	{
		private static readonly Mutex mutex = new Mutex(true, "safe_exam_browser_single_instance_mutex");

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
			var root = new CompositionRoot();

			root.BuildObjectGraph();

			root.Logger.Log(root.Settings.LogHeader);
			root.Logger.Log($"# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{Environment.NewLine}");

			if (NoInstanceRunning(root))
			{
				var app = new App();

				root.Logger.Info("No instance is running, initiating startup procedure.");

				app.Startup += (o, args) => root.StartupController.InitializeApplication(app.Shutdown);
				app.Exit += (o, args) => root.ShutdownController.FinalizeApplication();

				app.Run(root.Taskbar);
			}
			else
			{
				root.Logger.Info("Could not start because of an already running instance.");
				root.MessageBox.Show(root.Text.Get(Key.MessageBox_SingleInstance), root.Text.Get(Key.MessageBox_SingleInstanceTitle));
			}

			root.Logger.Log($"# Application terminating normally at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
		}

		private static bool NoInstanceRunning(CompositionRoot root)
		{
			return mutex.WaitOne(TimeSpan.Zero, true);
		}
	}
}
