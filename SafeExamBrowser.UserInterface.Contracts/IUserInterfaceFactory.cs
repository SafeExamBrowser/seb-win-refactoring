/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Audio;
using SafeExamBrowser.SystemComponents.Contracts.Keyboard;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.UserInterface.Contracts
{
	/// <summary>
	/// The factory for user interface elements which cannot be instantiated at the composition root.
	/// </summary>
	public interface IUserInterfaceFactory
	{
		/// <summary>
		/// Creates a new about window displaying information about the currently running application version.
		/// </summary>
		IWindow CreateAboutWindow(AppConfig appConfig);

		/// <summary>
		/// Creates an application control for the specified application and location.
		/// </summary>
		IApplicationControl CreateApplicationControl(IApplication application, Location location);

		/// <summary>
		/// Creates a system control which allows to change the audio settings of the computer.
		/// </summary>
		ISystemControl CreateAudioControl(IAudio audio, Location location);

		/// <summary>
		/// Creates a new browser window loaded with the given browser control and settings.
		/// </summary>
		IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings, bool isMainWindow);

		/// <summary>
		/// Creates a system control which allows to change the keyboard layout of the computer.
		/// </summary>
		ISystemControl CreateKeyboardLayoutControl(IKeyboard keyboard, Location location);

		/// <summary>
		/// Creates a new log window which runs on its own thread.
		/// </summary>
		IWindow CreateLogWindow(ILogger logger);

		/// <summary>
		/// Creates a notification control for the specified location, initialized with the given notification information.
		/// </summary>
		INotificationControl CreateNotificationControl(INotificationInfo info, Location location);

		/// <summary>
		/// Creates a password dialog with the given message and title.
		/// </summary>
		IPasswordDialog CreatePasswordDialog(string message, string title);

		/// <summary>
		/// Creates a password dialog with the given message and title.
		/// </summary>
		IPasswordDialog CreatePasswordDialog(TextKey message, TextKey title);

		/// <summary>
		/// Creates a system control displaying the power supply status of the computer.
		/// </summary>
		ISystemControl CreatePowerSupplyControl(IPowerSupply powerSupply, Location location);

		/// <summary>
		/// Creates a new runtime window which runs on its own thread.
		/// </summary>
		/// <returns></returns>
		IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig);

		/// <summary>
		/// Creates a new splash screen which runs on its own thread.
		/// </summary>
		ISplashScreen CreateSplashScreen(AppConfig appConfig = null);

		/// <summary>
		/// Creates a system control which allows to change the wireless network connection of the computer.
		/// </summary>
		ISystemWirelessNetworkControl CreateWirelessNetworkControl(Location location);
	}
}
