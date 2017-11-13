/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using System.Windows;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.UserInterface.Classic.Controls;

namespace SafeExamBrowser.UserInterface.Classic
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		public IWindow CreateAboutWindow(ISettings settings, IText text)
		{
			return new AboutWindow(settings, text);
		}

		public IApplicationButton CreateApplicationButton(IApplicationInfo info)
		{
			return new ApplicationButton(info);
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, IBrowserSettings settings)
		{
			return new BrowserWindow(control, settings);
		}

		public IWindow CreateLogWindow(ILogger logger, IText text)
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

		public ISystemKeyboardLayoutControl CreateKeyboardLayoutControl()
		{
			return new KeyboardLayoutControl();
		}

		public ISystemPowerSupplyControl CreatePowerSupplyControl()
		{
			return new PowerSupplyControl();
		}

		public ISplashScreen CreateSplashScreen(ISettings settings, IText text)
		{
			SplashScreen splashScreen = null;
			var splashReadyEvent = new AutoResetEvent(false);
			var splashScreenThread = new Thread(() =>
			{
				splashScreen = new SplashScreen(settings, text);
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

		public void Show(string message, string title, MessageBoxAction action = MessageBoxAction.Confirm, MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			MessageBox.Show(message, title, ToButton(action), ToImage(icon));
		}

		private MessageBoxButton ToButton(MessageBoxAction action)
		{
			switch (action)
			{
				default:
					return MessageBoxButton.OK;
			}
		}

		private MessageBoxImage ToImage(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Warning:
					return MessageBoxImage.Warning;
				case MessageBoxIcon.Error:
					return MessageBoxImage.Error;
				default:
					return MessageBoxImage.Information;
			}
		}
	}
}
