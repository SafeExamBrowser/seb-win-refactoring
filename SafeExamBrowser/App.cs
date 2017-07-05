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
using SafeExamBrowser.UserInterface;

namespace SafeExamBrowser
{
	public class App : Application
	{
		private static Mutex mutex = new Mutex(true, "safe_exam_browser_single_instance_mutex");

		[STAThread]
		public static void Main()
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				StartApplication();
			}
			else
			{
				MessageBox.Show(Strings.Get(Key.MessageBox_SingleInstance), Strings.Get(Key.MessageBox_SingleInstanceTitle));
			}
		}

		private static void StartApplication()
		{
			var app = new App();
			var taskbar = new Taskbar();

			app.Run(taskbar);
		}
	}
}
