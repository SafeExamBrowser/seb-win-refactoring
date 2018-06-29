/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Browser;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.UserInterface.Windows;
using SafeExamBrowser.UserInterface.Classic.Controls;
using SafeExamBrowser.UserInterface.Classic.Utilities;

namespace SafeExamBrowser.UserInterface.Classic
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		private IText text;

		public UserInterfaceFactory(IText text)
		{
			this.text = text;
		}

		public IWindow CreateAboutWindow(AppConfig appConfig)
		{
			return new AboutWindow(appConfig, text);
		}

		public IApplicationButton CreateApplicationButton(IApplicationInfo info)
		{
			return new ApplicationButton(info);
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings)
		{
			return new BrowserWindow(control, settings);
		}

		public ISystemKeyboardLayoutControl CreateKeyboardLayoutControl()
		{
			return new KeyboardLayoutControl();
		}

		public IWindow CreateLogWindow(ILogger logger)
		{
			LogWindow logWindow = null;
			var logWindowReadyEvent = new AutoResetEvent(false);
			var logWindowThread = new Thread(() =>
			{
				logWindow = new LogWindow(logger, text);
				logWindow.Closed += (o, args) => logWindow.Dispatcher.InvokeShutdown();
				logWindow.Show();

				logWindowReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			logWindowThread.SetApartmentState(ApartmentState.STA);
			logWindowThread.Name = nameof(LogWindow);
			logWindowThread.IsBackground = true;
			logWindowThread.Start();

			logWindowReadyEvent.WaitOne();

			return logWindow;
		}

		public INotificationButton CreateNotification(INotificationInfo info)
		{
			return new NotificationButton(info);
		}

		public IPasswordDialog CreatePasswordDialog(string message, string title)
		{
			throw new System.NotImplementedException();
		}

		public ISystemPowerSupplyControl CreatePowerSupplyControl()
		{
			return new PowerSupplyControl();
		}

		public IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig)
		{
			RuntimeWindow runtimeWindow = null;
			var windowReadyEvent = new AutoResetEvent(false);
			var runtimeWindowThread = new Thread(() =>
			{
				runtimeWindow = new RuntimeWindow(appConfig, new RuntimeWindowLogFormatter(), text);
				runtimeWindow.Closed += (o, args) => runtimeWindow.Dispatcher.InvokeShutdown();

				windowReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			runtimeWindowThread.SetApartmentState(ApartmentState.STA);
			runtimeWindowThread.Name = nameof(RuntimeWindow);
			runtimeWindowThread.IsBackground = true;
			runtimeWindowThread.Start();

			windowReadyEvent.WaitOne();

			return runtimeWindow;
		}

		public ISplashScreen CreateSplashScreen(AppConfig appConfig = null)
		{
			SplashScreen splashScreen = null;
			var splashReadyEvent = new AutoResetEvent(false);
			var splashScreenThread = new Thread(() =>
			{
				splashScreen = new SplashScreen(text, appConfig);
				splashScreen.Closed += (o, args) => splashScreen.Dispatcher.InvokeShutdown();
				splashScreen.Show();

				splashReadyEvent.Set();

				System.Windows.Threading.Dispatcher.Run();
			});

			splashScreenThread.SetApartmentState(ApartmentState.STA);
			splashScreenThread.Name = nameof(SplashScreen);
			splashScreenThread.IsBackground = true;
			splashScreenThread.Start();

			splashReadyEvent.WaitOne();

			return splashScreen;
		}

		public ISystemWirelessNetworkControl CreateWirelessNetworkControl()
		{
			return new WirelessNetworkControl();
		}
	}
}
