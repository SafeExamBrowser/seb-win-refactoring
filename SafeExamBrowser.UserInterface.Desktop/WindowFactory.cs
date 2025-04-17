/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading;
using System.Windows;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.UserInterface;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.UserInterface.Desktop.Windows;
using SafeExamBrowser.UserInterface.Shared;
using SplashScreen = SafeExamBrowser.UserInterface.Desktop.Windows.SplashScreen;

namespace SafeExamBrowser.UserInterface.Desktop
{
	internal class WindowFactory : Guardable
	{
		private readonly IText text;

		internal WindowFactory(IText text, IWindowGuard windowGuard = default) : base(windowGuard)
		{
			this.text = text;
		}

		internal IWindow CreateAboutWindow(AppConfig appConfig)
		{
			return Guard(new AboutWindow(appConfig, text));
		}

		internal IActionCenter CreateActionCenter()
		{
			return Guard(new ActionCenter());
		}

		internal IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings, bool isMainWindow, ILogger logger)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new BrowserWindow(control, settings, isMainWindow, text, logger)));
		}

		internal ICredentialsDialog CreateCredentialsDialog(CredentialsDialogPurpose purpose, string message, string title)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new CredentialsDialog(purpose, message, title, text)));
		}

		internal IExamSelectionDialog CreateExamSelectionDialog(IEnumerable<Exam> exams)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new ExamSelectionDialog(exams, text)));
		}

		internal ILockScreen CreateLockScreen(string message, string title, IEnumerable<LockScreenOption> options, LockScreenSettings settings)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new LockScreen(message, title, settings, text, options)));
		}

		internal IWindow CreateLogWindow(ILogger logger)
		{
			var window = default(LogWindow);
			var windowReadyEvent = new AutoResetEvent(false);
			var windowThread = new Thread(() =>
			{
				window = Guard(new LogWindow(logger, text));
				window.Closed += (o, args) => window.Dispatcher.InvokeShutdown();
				window.Show();

				windowReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			windowThread.SetApartmentState(ApartmentState.STA);
			windowThread.IsBackground = true;
			windowThread.Start();

			windowReadyEvent.WaitOne();

			return window;
		}

		internal IPasswordDialog CreatePasswordDialog(string message, string title)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new PasswordDialog(message, title, text)));
		}

		internal IPasswordDialog CreatePasswordDialog(TextKey message, TextKey title)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new PasswordDialog(text.Get(message), text.Get(title), text)));
		}

		internal IProctoringFinalizationDialog CreateProctoringFinalizationDialog()
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new ProctoringFinalizationDialog(text)));
		}

		internal IProctoringWindow CreateProctoringWindow(IProctoringControl control)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new ProctoringWindow(control)));
		}

		internal IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new RuntimeWindow(appConfig, text)));
		}

		internal IServerFailureDialog CreateServerFailureDialog(string info, bool showFallback)
		{
			return Application.Current.Dispatcher.Invoke(() => Guard(new ServerFailureDialog(info, showFallback, text)));
		}

		internal ISplashScreen CreateSplashScreen(AppConfig appConfig = null)
		{
			var window = default(SplashScreen);
			var windowReadyEvent = new AutoResetEvent(false);
			var windowThread = new Thread(() =>
			{
				window = Guard(new SplashScreen(text, appConfig));
				window.Closed += (o, args) => window.Dispatcher.InvokeShutdown();
				window.Show();

				windowReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			windowThread.SetApartmentState(ApartmentState.STA);
			windowThread.IsBackground = true;
			windowThread.Start();

			windowReadyEvent.WaitOne();

			return window;
		}

		internal ITaskbar CreateTaskbar(ILogger logger)
		{
			return Guard(new Taskbar(logger));
		}

		internal ITaskview CreateTaskview()
		{
			return Guard(new Taskview());
		}
	}
}
