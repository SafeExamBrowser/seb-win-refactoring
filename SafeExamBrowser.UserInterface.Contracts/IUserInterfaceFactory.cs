/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
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
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

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
		/// Creates a new action center.
		/// </summary>
		IActionCenter CreateActionCenter();

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
		IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings, bool isMainWindow, ILogger logger);

		/// <summary>
		/// Creates an exam selection dialog for the given exams.
		/// </summary>
		IExamSelectionDialog CreateExamSelectionDialog(IEnumerable<Exam> exams);

		/// <summary>
		/// Creates a system control which allows to change the keyboard layout of the computer.
		/// </summary>
		ISystemControl CreateKeyboardLayoutControl(IKeyboard keyboard, Location location);

		/// <summary>
		/// Creates a lock screen with the given message, title and options.
		/// </summary>
		ILockScreen CreateLockScreen(string message, string title, IEnumerable<LockScreenOption> options);

		/// <summary>
		/// Creates a new log window which runs on its own thread.
		/// </summary>
		IWindow CreateLogWindow(ILogger logger);

		/// <summary>
		/// Creates a system control which allows to view and/or change the network connection of the computer.
		/// </summary>
		ISystemControl CreateNetworkControl(INetworkAdapter adapter, Location location);

		/// <summary>
		/// Creates a notification control for the given notification, initialized for the specified location.
		/// </summary>
		INotificationControl CreateNotificationControl(INotification notification, Location location);

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
		/// Creates a new proctoring window loaded with the given proctoring control.
		/// </summary>
		IProctoringWindow CreateProctoringWindow(IProctoringControl control);

		/// <summary>
		/// Creates a new notification control for the raise hand functionality of a remote proctoring session.
		/// </summary>
		INotificationControl CreateRaiseHandControl(IProctoringController controller, Location location, ProctoringSettings settings);

		/// <summary>
		/// Creates a new runtime window which runs on its own thread.
		/// </summary>
		IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig);

		/// <summary>
		/// Creates a new server failure dialog with the given parameters.
		/// </summary>
		IServerFailureDialog CreateServerFailureDialog(string info, bool showFallback);

		/// <summary>
		/// Creates a new splash screen which runs on its own thread.
		/// </summary>
		ISplashScreen CreateSplashScreen(AppConfig appConfig = null);

		/// <summary>
		/// Creates a new taskbar.
		/// </summary>
		ITaskbar CreateTaskbar(ILogger logger);

		/// <summary>
		/// Creates a new taskview.
		/// </summary>
		ITaskview CreateTaskview();
	}
}
