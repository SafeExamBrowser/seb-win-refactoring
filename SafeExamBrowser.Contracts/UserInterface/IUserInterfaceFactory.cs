/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Contracts.UserInterface
{
	public interface IUserInterfaceFactory : IMessageBox
	{
		/// <summary>
		/// Creates a new about window displaying information about the currently running application version.
		/// </summary>
		IWindow CreateAboutWindow(ISettings settings, IText text);

		/// <summary>
		/// Creates a taskbar button, initialized with the given application information.
		/// </summary>
		IApplicationButton CreateApplicationButton(IApplicationInfo info);

		/// <summary>
		/// Creates a new browser window loaded with the given browser control and settings.
		/// </summary>
		IBrowserWindow CreateBrowserWindow(IBrowserControl control, IBrowserSettings settings);

		/// <summary>
		/// Creates a new log window which runs on its own thread.
		/// </summary>
		IWindow CreateLogWindow(ILogger logger, IText text);

		/// <summary>
		/// Creates a taskbar notification, initialized with the given notification information.
		/// </summary>
		INotificationButton CreateNotification(INotificationInfo info);

		/// <summary>
		/// Creates a system control which allows to change the keyboard layout of the computer.
		/// </summary>
		ISystemKeyboardLayoutControl CreateKeyboardLayoutControl();

		/// <summary>
		/// Creates a system control displaying the power supply status of the computer.
		/// </summary>
		ISystemPowerSupplyControl CreatePowerSupplyControl();

		/// <summary>
		/// Creates a new splash screen which runs on its own thread.
		/// </summary>
		ISplashScreen CreateSplashScreen(ISettings settings, IText text);
	}
}
