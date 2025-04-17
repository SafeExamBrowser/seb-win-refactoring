/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
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
using SafeExamBrowser.Settings.UserInterface;
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

namespace SafeExamBrowser.UserInterface.Desktop
{
	public class UserInterfaceFactory : IUserInterfaceFactory
	{
		private readonly ControlFactory controlFactory;
		private readonly IText text;
		private readonly WindowFactory windowFactory;

		/// <remarks>
		/// The <see cref="IWindowGuard"/> is optional, as it is only used by the client application component.
		/// </remarks>
		public UserInterfaceFactory(IText text, IWindowGuard windowGuard = default)
		{
			this.controlFactory = new ControlFactory(text);
			this.text = text;
			this.windowFactory = new WindowFactory(text, windowGuard);

			InitializeFontAwesome();
		}

		public IWindow CreateAboutWindow(AppConfig appConfig)
		{
			return windowFactory.CreateAboutWindow(appConfig);
		}

		public IActionCenter CreateActionCenter()
		{
			return windowFactory.CreateActionCenter();
		}

		public IApplicationControl CreateApplicationControl(IApplication<IApplicationWindow> application, Location location)
		{
			return controlFactory.CreateApplicationControl(application, location);
		}

		public ISystemControl CreateAudioControl(IAudio audio, Location location)
		{
			return controlFactory.CreateAudioControl(audio, location);
		}

		public IBrowserWindow CreateBrowserWindow(IBrowserControl control, BrowserSettings settings, bool isMainWindow, ILogger logger)
		{
			return windowFactory.CreateBrowserWindow(control, settings, isMainWindow, logger);
		}

		public ICredentialsDialog CreateCredentialsDialog(CredentialsDialogPurpose purpose, string message, string title)
		{
			return windowFactory.CreateCredentialsDialog(purpose, message, title);
		}

		public IExamSelectionDialog CreateExamSelectionDialog(IEnumerable<Exam> exams)
		{
			return windowFactory.CreateExamSelectionDialog(exams);
		}

		public ISystemControl CreateKeyboardLayoutControl(IKeyboard keyboard, Location location)
		{
			return controlFactory.CreateKeyboardLayoutControl(keyboard, location);
		}

		public ILockScreen CreateLockScreen(string message, string title, IEnumerable<LockScreenOption> options, LockScreenSettings settings)
		{
			return windowFactory.CreateLockScreen(message, title, options, settings);
		}

		public IWindow CreateLogWindow(ILogger logger)
		{
			return windowFactory.CreateLogWindow(logger);
		}

		public ISystemControl CreateNetworkControl(INetworkAdapter adapter, Location location)
		{
			return controlFactory.CreateNetworkControl(adapter, location);
		}

		public INotificationControl CreateNotificationControl(INotification notification, Location location)
		{
			return controlFactory.CreateNotificationControl(notification, location);
		}

		public IPasswordDialog CreatePasswordDialog(string message, string title)
		{
			return windowFactory.CreatePasswordDialog(message, title);
		}

		public IPasswordDialog CreatePasswordDialog(TextKey message, TextKey title)
		{
			return windowFactory.CreatePasswordDialog(text.Get(message), text.Get(title));
		}

		public ISystemControl CreatePowerSupplyControl(IPowerSupply powerSupply, Location location)
		{
			return controlFactory.CreatePowerSupplyControl(powerSupply, location);
		}

		public IProctoringFinalizationDialog CreateProctoringFinalizationDialog()
		{
			return windowFactory.CreateProctoringFinalizationDialog();
		}

		public IProctoringWindow CreateProctoringWindow(IProctoringControl control)
		{
			return windowFactory.CreateProctoringWindow(control);
		}

		public INotificationControl CreateRaiseHandControl(IProctoringController controller, Location location, ProctoringSettings settings)
		{
			return controlFactory.CreateRaiseHandControl(controller, location, settings);
		}

		public IRuntimeWindow CreateRuntimeWindow(AppConfig appConfig)
		{
			return windowFactory.CreateRuntimeWindow(appConfig);
		}

		public IServerFailureDialog CreateServerFailureDialog(string info, bool showFallback)
		{
			return windowFactory.CreateServerFailureDialog(info, showFallback);
		}

		public ISplashScreen CreateSplashScreen(AppConfig appConfig = null)
		{
			return windowFactory.CreateSplashScreen(appConfig);
		}

		public ITaskbar CreateTaskbar(ILogger logger)
		{
			return windowFactory.CreateTaskbar(logger);
		}

		public ITaskview CreateTaskview()
		{
			return windowFactory.CreateTaskview();
		}

		private void InitializeFontAwesome()
		{
			// To be able to use FontAwesome in XAML icon resources, we need to make sure that the FontAwesome.WPF assembly is loaded into
			// the AppDomain before attempting to load an icon resource - thus the creation of an unused image below...
			ImageAwesome.CreateImageSource(FontAwesomeIcon.FontAwesome, Brushes.Black);
		}
	}
}
