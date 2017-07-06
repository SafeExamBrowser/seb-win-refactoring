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
using SafeExamBrowser.Core.I18n;

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

				StartApplication(compositionRoot.Taskbar);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n\n" + e.StackTrace, Text.Instance.Get(Key.MessageBox_FatalErrorTitle));
			}
		}

		private static void StartApplication(Window taskbar)
		{
			if (NoInstanceRunning())
			{
				new App().Run(taskbar);
			}
			else
			{
				MessageBox.Show(Text.Instance.Get(Key.MessageBox_SingleInstance), Text.Instance.Get(Key.MessageBox_SingleInstanceTitle));
			}
		}

		private static bool NoInstanceRunning()
		{
			return mutex.WaitOne(TimeSpan.Zero, true);
		}
	}
}
