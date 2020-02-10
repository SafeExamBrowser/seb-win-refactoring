/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Proxy;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.Settings.UserInterface;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal class DataValues
	{
		private const string DEFAULT_CONFIGURATION_NAME = "SebClientSettings.seb";
		private AppConfig appConfig;

		internal string GetAppDataFilePath()
		{
			return appConfig.AppDataFilePath;
		}

		internal AppConfig InitializeAppConfig()
		{
			var executable = Assembly.GetExecutingAssembly();
			var certificate = executable.Modules.First().GetSignerCertificate();
			var programBuild = FileVersionInfo.GetVersionInfo(executable.Location).FileVersion;
			var programCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			var programTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			var programVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			var appDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(SafeExamBrowser));
			var appDataRoamingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var programDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser));
			var startTime = DateTime.Now;
			var logFolder = Path.Combine(appDataLocalFolder, "Logs");
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			appConfig = new AppConfig();
			appConfig.AppDataFilePath = Path.Combine(appDataRoamingFolder, DEFAULT_CONFIGURATION_NAME);
			appConfig.ApplicationStartTime = startTime;
			appConfig.BrowserCachePath = Path.Combine(appDataLocalFolder, "Cache");
			appConfig.BrowserLogFilePath = Path.Combine(logFolder, $"{logFilePrefix}_Browser.log");
			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{AppConfig.BASE_ADDRESS}/client/{Guid.NewGuid()}";
			appConfig.ClientExecutablePath = Path.Combine(Path.GetDirectoryName(executable.Location), $"{nameof(SafeExamBrowser)}.Client.exe");
			appConfig.ClientLogFilePath = Path.Combine(logFolder, $"{logFilePrefix}_Client.log");
			appConfig.CodeSignatureHash = certificate?.GetCertHashString();
			appConfig.ConfigurationFileExtension = ".seb";
			appConfig.ConfigurationFileMimeType = "application/seb";
			appConfig.ProgramBuildVersion = programBuild;
			appConfig.ProgramCopyright = programCopyright;
			appConfig.ProgramDataFilePath = Path.Combine(programDataFolder, DEFAULT_CONFIGURATION_NAME);
			appConfig.ProgramTitle = programTitle;
			appConfig.ProgramInformationalVersion = programVersion;
			appConfig.RuntimeId = Guid.NewGuid();
			appConfig.RuntimeAddress = $"{AppConfig.BASE_ADDRESS}/runtime/{Guid.NewGuid()}";
			appConfig.RuntimeLogFilePath = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.log");
			appConfig.SebUriScheme = "seb";
			appConfig.SebUriSchemeSecure = "sebs";
			appConfig.ServiceAddress = $"{AppConfig.BASE_ADDRESS}/service";
			appConfig.ServiceEventName = $@"Global\{nameof(SafeExamBrowser)}-{Guid.NewGuid()}";
			appConfig.ServiceLogFilePath = Path.Combine(logFolder, $"{logFilePrefix}_Service.log");
			appConfig.TemporaryDirectory = Path.Combine(appDataLocalFolder, "Temp");

			return appConfig;
		}

		internal SessionConfiguration InitializeSessionConfiguration()
		{
			var configuration = new SessionConfiguration();

			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{AppConfig.BASE_ADDRESS}/client/{Guid.NewGuid()}";
			appConfig.ServiceEventName = $@"Global\{nameof(SafeExamBrowser)}-{Guid.NewGuid()}";

			configuration.AppConfig = appConfig.Clone();
			configuration.ClientAuthenticationToken = Guid.NewGuid();
			configuration.SessionId = Guid.NewGuid();

			return configuration;
		}

		internal AppSettings LoadDefaultSettings()
		{
			var settings = new AppSettings();

			settings.ActionCenter.EnableActionCenter = true;
			settings.ActionCenter.ShowApplicationInfo = true;
			settings.ActionCenter.ShowApplicationLog = false;
			settings.ActionCenter.ShowKeyboardLayout = true;
			settings.ActionCenter.ShowWirelessNetwork = false;
			settings.ActionCenter.ShowClock = true;

			settings.Browser.AdditionalWindow.AllowAddressBar = false;
			settings.Browser.AdditionalWindow.AllowBackwardNavigation = true;
			settings.Browser.AdditionalWindow.AllowDeveloperConsole = false;
			settings.Browser.AdditionalWindow.AllowForwardNavigation = true;
			settings.Browser.AdditionalWindow.AllowReloading = true;
			settings.Browser.AdditionalWindow.FullScreenMode = false;
			settings.Browser.AdditionalWindow.Position = WindowPosition.Right;
			settings.Browser.AdditionalWindow.RelativeHeight = 100;
			settings.Browser.AdditionalWindow.RelativeWidth = 50;
			settings.Browser.AdditionalWindow.ShowReloadWarning = false;
			settings.Browser.AllowConfigurationDownloads = true;
			settings.Browser.AllowCustomDownloadLocation = false;
			settings.Browser.AllowDownloads = true;
			settings.Browser.AllowPageZoom = true;
			settings.Browser.AllowPdfReader = true;
			settings.Browser.AllowPdfReaderToolbar = false;
			settings.Browser.AllowUploads = true;
			settings.Browser.MainWindow.AllowAddressBar = false;
			settings.Browser.MainWindow.AllowBackwardNavigation = false;
			settings.Browser.MainWindow.AllowDeveloperConsole = false;
			settings.Browser.MainWindow.AllowForwardNavigation = false;
			settings.Browser.MainWindow.AllowReloading = true;
			settings.Browser.MainWindow.FullScreenMode = false;
			settings.Browser.MainWindow.RelativeHeight = 100;
			settings.Browser.MainWindow.RelativeWidth = 100;
			settings.Browser.MainWindow.ShowReloadWarning = true;
			settings.Browser.PopupPolicy = PopupPolicy.Allow;
			settings.Browser.Proxy.Policy = ProxyPolicy.System;
			settings.Browser.StartUrl = "https://www.safeexambrowser.org/start";

			settings.Keyboard.AllowAltEsc = false;
			settings.Keyboard.AllowAltF4 = false;
			settings.Keyboard.AllowAltTab = true;
			settings.Keyboard.AllowCtrlEsc = false;
			settings.Keyboard.AllowEsc = true;
			settings.Keyboard.AllowF1 = true;
			settings.Keyboard.AllowF2 = true;
			settings.Keyboard.AllowF3 = true;
			settings.Keyboard.AllowF4 = true;
			settings.Keyboard.AllowF5 = true;
			settings.Keyboard.AllowF6 = true;
			settings.Keyboard.AllowF7 = true;
			settings.Keyboard.AllowF8 = true;
			settings.Keyboard.AllowF9 = true;
			settings.Keyboard.AllowF10 = true;
			settings.Keyboard.AllowF11 = true;
			settings.Keyboard.AllowF12 = true;
			settings.Keyboard.AllowPrintScreen = false;
			settings.Keyboard.AllowSystemKey = false;

			settings.LogLevel = LogLevel.Debug;

			settings.Mouse.AllowMiddleButton = false;
			settings.Mouse.AllowRightButton = true;

			settings.Security.AllowApplicationLogAccess = false;
			settings.Security.KioskMode = KioskMode.CreateNewDesktop;
			settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Deny;

			settings.Service.DisableChromeNotifications = true;
			settings.Service.DisableEaseOfAccessOptions = true;
			settings.Service.DisableNetworkOptions = true;
			settings.Service.DisablePasswordChange = true;
			settings.Service.DisablePowerOptions = true;
			settings.Service.DisableRemoteConnections = true;
			settings.Service.DisableSignout = true;
			settings.Service.DisableTaskManager = true;
			settings.Service.DisableUserLock = true;
			settings.Service.DisableUserSwitch = true;
			settings.Service.DisableVmwareOverlay = true;
			settings.Service.DisableWindowsUpdate = true;
			settings.Service.Policy = ServicePolicy.Mandatory;

			settings.Taskbar.EnableTaskbar = true;
			settings.Taskbar.ShowApplicationInfo = false;
			settings.Taskbar.ShowApplicationLog = false;
			settings.Taskbar.ShowKeyboardLayout = true;
			settings.Taskbar.ShowWirelessNetwork = false;
			settings.Taskbar.ShowClock = true;

			settings.UserInterfaceMode = UserInterfaceMode.Desktop;
			
			return settings;
		}
	}
}
