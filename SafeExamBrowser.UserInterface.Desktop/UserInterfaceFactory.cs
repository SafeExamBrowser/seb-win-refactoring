/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;
using SafeExamBrowser.UserInterface.Desktop.Windows;
using SplashScreen = SafeExamBrowser.UserInterface.Desktop.Windows.SplashScreen;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		private readonly IText text;

		public UserInterfaceFactory(IText text)
		{
			this.text = text;

			InitializeFontAwesome();
		}

		public IWindow CreateAboutWindow(AppConfig appConfig)
		{
			return new AboutWindow(appConfig, text);
		}

		public IActionCenter CreateActionCenter()
		{
			return new ActionCenter();
		}

		public IApplicationControl CreateApplicationControl(IApplication application, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.ApplicationControl(application);
			}
			else
			{
				return new Controls.Taskbar.ApplicationControl(application);
			}
		}

		public ISystemControl CreateAudioControl(IAudio audio, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.AudioControl(audio, text);
			}
			else
			{
				return new Controls.Taskbar.AudioControl(audio, text);
			}
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings, bool isMainWindow, ILogger logger)
		{
			return Application.Current.Dispatcher.Invoke(() => new BrowserWindow(control, settings, isMainWindow, text, logger));
		}

		public IExamSelectionDialog CreateExamSelectionDialog(IEnumerable<Exam> exams)
		{
			return Application.Current.Dispatcher.Invoke(() => new ExamSelectionDialog(exams, text));
		}

		public ISystemControl CreateKeyboardLayoutControl(IKeyboard keyboard, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.KeyboardLayoutControl(keyboard, text);
			}
			else
			{
				return new Controls.Taskbar.KeyboardLayoutControl(keyboard, text);
			}
		}

		public ILockScreen CreateLockScreen(string message, string title, IEnumerable<LockScreenOption> options)
		{
			return Application.Current.Dispatcher.Invoke(() => new LockScreen(message, title, text, options));
		}

		public IWindow CreateLogWindow(ILogger logger)
		{
			var window = default(LogWindow);
			var windowReadyEvent = new AutoResetEvent(false);
			var windowThread = new Thread(() =>
			{
				window = new LogWindow(logger, text);
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

		public ISystemControl CreateNetworkControl(INetworkAdapter adapter, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.NetworkControl(adapter, text);
			}
			else
			{
				return new Controls.Taskbar.NetworkControl(adapter, text);
			}
		}

		public INotificationControl CreateNotificationControl(INotification notification, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.NotificationButton(notification);
			}
			else
			{
				return new Controls.Taskbar.NotificationButton(notification);
			}
		}

		public IPasswordDialog CreatePasswordDialog(string message, string title)
		{
			return Application.Current.Dispatcher.Invoke(() => new PasswordDialog(message, title, text));
		}

		public IPasswordDialog CreatePasswordDialog(TextKey message, TextKey title)
		{
			return Application.Current.Dispatcher.Invoke(() => new PasswordDialog(text.Get(message), text.Get(title), text));
		}

		public ISystemControl CreatePowerSupplyControl(IPowerSupply powerSupply, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.PowerSupplyControl(powerSupply, text);
			}
			else
			{
				return new Controls.Taskbar.PowerSupplyControl(powerSupply, text);
			}
		}

		public IProctoringWindow CreateProctoringWindow(IProctoringControl control)
		{
			return Application.Current.Dispatcher.Invoke(() => new ProctoringWindow(control));
		}

		public INotificationControl CreateRaiseHandControl(IProctoringController controller, Location location, ProctoringSettings settings)
		{
			if (location == Location.ActionCenter)
			{
				return new Controls.ActionCenter.RaiseHandControl(controller, settings, text);
			}
			else
			{
				return new Controls.Taskbar.RaiseHandControl(controller, settings, text);
			}
		}

		public IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig)
		{
			return Application.Current.Dispatcher.Invoke(() => new RuntimeWindow(appConfig, text));
		}

		public IServerFailureDialog CreateServerFailureDialog(string info, bool showFallback)
		{
			return Application.Current.Dispatcher.Invoke(() => new ServerFailureDialog(info, showFallback, text));
		}

		public ISplashScreen CreateSplashScreen(AppConfig appConfig = null)
		{
			var window = default(SplashScreen);
			var windowReadyEvent = new AutoResetEvent(false);
			var windowThread = new Thread(() =>
			{
				window = new SplashScreen(text, appConfig);
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

		public ITaskbar CreateTaskbar(ILogger logger)
		{
			return new Taskbar(logger);
		}

		public ITaskview CreateTaskview()
		{
			return new Taskview();
		}

		private void InitializeFontAwesome()
		{
			// To be able to use FontAwesome in XAML icon resources, we need to make sure that the FontAwesome.WPF assembly is loaded into
			// the AppDomain before attempting to load an icon resource - thus the creation of an unused image below...
			ImageAwesome.CreateImageSource(FontAwesomeIcon.FontAwesome, Brushes.Black);
		}
	}
}
