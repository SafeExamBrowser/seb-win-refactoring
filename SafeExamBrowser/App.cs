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
		private static Mutex mutex = new Mutex(true, "safe_exam_browser_single_instance_mutex");

		[STAThread]
		public static void Main()
		{
			try
			{
				var compositionRoot = new CompositionRoot();

				compositionRoot.InitializeGlobalModules();
				compositionRoot.BuildObjectGraph();

				StartApplication(compositionRoot);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Fatal Error");
			}
		}

		private static void StartApplication(CompositionRoot compositionRoot)
		{
			compositionRoot.Logger.Info("Testing the log...");

			if (NoInstanceRunning())
			{
				new App().Run(compositionRoot.Taskbar);
			}
			else
			{
				var message = compositionRoot.Text.Get(Key.MessageBox_SingleInstance);
				var title = compositionRoot.Text.Get(Key.MessageBox_SingleInstanceTitle);

				MessageBox.Show(message, title);
			}
		}

		private static bool NoInstanceRunning()
		{
			return mutex.WaitOne(TimeSpan.Zero, true);
		}
	}
}
