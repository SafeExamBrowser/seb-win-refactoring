/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.UserInterface.Windows10.Controls;
using MessageBoxResult = SafeExamBrowser.Contracts.UserInterface.MessageBoxResult;

namespace SafeExamBrowser.UserInterface.Windows10
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		private IText text;

		public UserInterfaceFactory(IText text)
		{
			this.text = text;
		}

		public IWindow CreateAboutWindow(RuntimeInfo runtimeInfo)
		{
			return new AboutWindow(runtimeInfo, text);
		}

		public IApplicationButton CreateApplicationButton(IApplicationInfo info)
		{
			return new ApplicationButton(info);
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings)
		{
			return new BrowserWindow(control, settings);
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

		public ISystemKeyboardLayoutControl CreateKeyboardLayoutControl()
		{
			// TODO
			throw new System.NotImplementedException();
		}

		public ISystemPowerSupplyControl CreatePowerSupplyControl()
		{
			return new PowerSupplyControl();
		}

		public IRuntimeWindow CreateRuntimeWindow(RuntimeInfo runtimeInfo)
		{
			// TODO
			throw new System.NotImplementedException();
		}

		public ISplashScreen CreateSplashScreen(RuntimeInfo runtimeInfo)
		{
			SplashScreen splashScreen = null;
			var splashReadyEvent = new AutoResetEvent(false);
			var splashScreenThread = new Thread(() =>
			{
				splashScreen = new SplashScreen(runtimeInfo, text);
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
			// TODO
			throw new System.NotImplementedException();
		}

		public MessageBoxResult Show(string message, string title, MessageBoxAction action = MessageBoxAction.Confirm, MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			// The last two parameters are an unfortunate necessity, since e.g. splash screens are displayed topmost while running in their
			// own thread / dispatcher, and would thus conceal the message box...
			var result = MessageBox.Show(message, title, ToButton(action), ToImage(icon), System.Windows.MessageBoxResult.None, MessageBoxOptions.ServiceNotification);

			return ToResult(result);
		}

		public MessageBoxResult Show(TextKey message, TextKey title, MessageBoxAction action = MessageBoxAction.Confirm, MessageBoxIcon icon = MessageBoxIcon.Information)
		{
			return Show(text.Get(message), text.Get(title), action, icon);
		}

		private MessageBoxButton ToButton(MessageBoxAction action)
		{
			switch (action)
			{
				case MessageBoxAction.YesNo:
					return MessageBoxButton.YesNo;
				default:
					return MessageBoxButton.OK;
			}
		}

		private MessageBoxImage ToImage(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Error:
					return MessageBoxImage.Error;
				case MessageBoxIcon.Question:
					return MessageBoxImage.Question;
				case MessageBoxIcon.Warning:
					return MessageBoxImage.Warning;
				default:
					return MessageBoxImage.Information;
			}
		}

		private MessageBoxResult ToResult(System.Windows.MessageBoxResult result)
		{
			switch (result)
			{
				case System.Windows.MessageBoxResult.Cancel:
					return MessageBoxResult.Cancel;
				case System.Windows.MessageBoxResult.No:
					return MessageBoxResult.No;
				case System.Windows.MessageBoxResult.OK:
					return MessageBoxResult.Ok;
				case System.Windows.MessageBoxResult.Yes:
					return MessageBoxResult.Yes;
				default:
					return MessageBoxResult.None;
			}
		}
	}
}
