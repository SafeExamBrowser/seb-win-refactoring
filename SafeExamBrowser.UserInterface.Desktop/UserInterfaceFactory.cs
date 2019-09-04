/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Desktop.Controls;

namespace SafeExamBrowser.UserInterface.Desktop
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		private IText text;

		public UserInterfaceFactory(IText text)
		{
			this.text = text;

			InitializeFontAwesome();
		}

		public IWindow CreateAboutWindow(AppConfig appConfig)
		{
			return new AboutWindow(appConfig, text);
		}

		public IApplicationControl CreateApplicationControl(IApplication application, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new ActionCenterApplicationControl(application);
			}
			else
			{
				return new TaskbarApplicationControl(application);
			}
		}

		public ISystemControl CreateAudioControl(IAudio audio, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new ActionCenterAudioControl(audio, text);
			}
			else
			{
				return new TaskbarAudioControl(audio, text);
			}
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings, bool isMainWindow)
		{
			return new BrowserWindow(control, settings, isMainWindow, text);
		}

		public ISystemControl CreateKeyboardLayoutControl(IKeyboard keyboard, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new ActionCenterKeyboardLayoutControl(keyboard, text);
			}
			else
			{
				return new TaskbarKeyboardLayoutControl(keyboard, text);
			}
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
			logWindowThread.IsBackground = true;
			logWindowThread.Start();

			logWindowReadyEvent.WaitOne();

			return logWindow;
		}

		public INotificationControl CreateNotificationControl(INotificationController controller, INotificationInfo info, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new ActionCenterNotificationButton(controller, info);
			}
			else
			{
				return new TaskbarNotificationButton(controller, info);
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
				return new ActionCenterPowerSupplyControl(powerSupply, text);
			}
			else
			{
				return new TaskbarPowerSupplyControl(powerSupply, text);
			}
		}

		public IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig)
		{
			return Application.Current.Dispatcher.Invoke(() => new RuntimeWindow(appConfig, text));
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

		public ISystemControl CreateWirelessNetworkControl(IWirelessAdapter wirelessAdapter, Location location)
		{
			if (location == Location.ActionCenter)
			{
				return new ActionCenterWirelessNetworkControl(wirelessAdapter, text);
			}
			else
			{
				return new TaskbarWirelessNetworkControl(wirelessAdapter, text);
			}
		}

		private void InitializeFontAwesome()
		{
			// To be able to use FontAwesome in XAML icon resources, we need to make sure that the FontAwesome.WPF assembly is loaded into
			// the AppDomain before attempting to load an icon resource - thus the creation of an unused image below...
			ImageAwesome.CreateImageSource(FontAwesomeIcon.FontAwesome, Brushes.Black);
		}
	}
}
