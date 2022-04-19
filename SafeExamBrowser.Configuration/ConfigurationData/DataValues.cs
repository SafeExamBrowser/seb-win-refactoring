/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Proxy;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.Proctoring;
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
			var executable = Assembly.GetEntryAssembly();
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
			settings.ActionCenter.ShowClock = true;
			settings.ActionCenter.ShowKeyboardLayout = true;
			settings.ActionCenter.ShowNetwork = false;

			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "AA_v3.exe", OriginalName = "AA_v3.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "AeroAdmin.exe", OriginalName = "AeroAdmin.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "beamyourscreen-host.exe", OriginalName = "beamyourscreen-host.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "CamPlay.exe", OriginalName = "CamPlay.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Camtasia.exe", OriginalName = "Camtasia.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "CamtasiaStudio.exe", OriginalName = "CamtasiaStudio.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Camtasia_Studio.exe", OriginalName = "Camtasia_Studio.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "CamRecorder.exe", OriginalName = "CamRecorder.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "CamtasiaUtl.exe", OriginalName = "CamtasiaUtl.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "chromoting.exe", OriginalName = "chromoting.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "CiscoCollabHost.exe", OriginalName = "CiscoCollabHost.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "CiscoWebExStart.exe", OriginalName = "CiscoWebExStart.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Discord.exe", OriginalName = "Discord.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Element.exe", OriginalName = "Element.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "g2mcomm.exe", OriginalName = "g2mcomm.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "g2mcomm.exe", OriginalName = "g2mcomm.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "g2mlauncher.exe", OriginalName = "g2mlauncher.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "g2mstart.exe", OriginalName = "g2mstart.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "GotoMeetingWinStore.exe", OriginalName = "GotoMeetingWinStore.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "join.me.exe", OriginalName = "join.me.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "join.me.sentinel.exe", OriginalName = "join.me.sentinel.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Mikogo-host.exe", OriginalName = "Mikogo-host.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "ptoneclk.exe", OriginalName = "ptoneclk.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "RemotePCDesktop.exe", OriginalName = "RemotePCDesktop.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "RPCService.exe", OriginalName = "RPCService.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "RPCSuite.exe", OriginalName = "RPCSuite.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Skype.exe", OriginalName = "Skype.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "SkypeApp.exe", OriginalName = "SkypeApp.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "SkypeHost.exe", OriginalName = "SkypeHost.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "slack.exe", OriginalName = "slack.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Teams.exe", OriginalName = "Teams.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "TeamViewer.exe", OriginalName = "TeamViewer.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Telegram.exe", OriginalName = "Telegram.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "vncserver.exe", OriginalName = "vncserver.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "vncviewer.exe", OriginalName = "vncviewer.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "vncserverui.exe", OriginalName = "vncserverui.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "webexmta.exe", OriginalName = "webexmta.exe" });
			settings.Applications.Blacklist.Add(new BlacklistApplication { ExecutableName = "Zoom.exe", OriginalName = "Zoom.exe" });

			settings.Browser.AdditionalWindow.AllowAddressBar = false;
			settings.Browser.AdditionalWindow.AllowBackwardNavigation = true;
			settings.Browser.AdditionalWindow.AllowDeveloperConsole = false;
			settings.Browser.AdditionalWindow.AllowForwardNavigation = true;
			settings.Browser.AdditionalWindow.AllowReloading = true;
			settings.Browser.AdditionalWindow.FullScreenMode = false;
			settings.Browser.AdditionalWindow.Position = WindowPosition.Right;
			settings.Browser.AdditionalWindow.RelativeHeight = 100;
			settings.Browser.AdditionalWindow.RelativeWidth = 50;
			settings.Browser.AdditionalWindow.ShowHomeButton = false;
			settings.Browser.AdditionalWindow.ShowReloadWarning = false;
			settings.Browser.AdditionalWindow.ShowToolbar = false;
			settings.Browser.AdditionalWindow.UrlPolicy = UrlPolicy.Never;
			settings.Browser.AllowConfigurationDownloads = true;
			settings.Browser.AllowCustomDownAndUploadLocation = false;
			settings.Browser.AllowDownloads = true;
			settings.Browser.AllowFind = true;
			settings.Browser.AllowPageZoom = true;
			settings.Browser.AllowPdfReader = true;
			settings.Browser.AllowPdfReaderToolbar = false;
			settings.Browser.AllowUploads = true;
			settings.Browser.DeleteCacheOnShutdown = true;
			settings.Browser.DeleteCookiesOnShutdown = true;
			settings.Browser.DeleteCookiesOnStartup = true;
			settings.Browser.EnableBrowser = true;
			settings.Browser.MainWindow.AllowAddressBar = false;
			settings.Browser.MainWindow.AllowBackwardNavigation = false;
			settings.Browser.MainWindow.AllowDeveloperConsole = false;
			settings.Browser.MainWindow.AllowForwardNavigation = false;
			settings.Browser.MainWindow.AllowReloading = true;
			settings.Browser.MainWindow.FullScreenMode = false;
			settings.Browser.MainWindow.RelativeHeight = 100;
			settings.Browser.MainWindow.RelativeWidth = 100;
			settings.Browser.MainWindow.ShowHomeButton = false;
			settings.Browser.MainWindow.ShowReloadWarning = true;
			settings.Browser.MainWindow.ShowToolbar = false;
			settings.Browser.MainWindow.UrlPolicy = UrlPolicy.Never;
			settings.Browser.PopupPolicy = PopupPolicy.Allow;
			settings.Browser.Proxy.Policy = ProxyPolicy.System;
			settings.Browser.ResetOnQuitUrl = false;
			settings.Browser.StartUrl = "https://www.safeexambrowser.org/start";
			settings.Browser.UseCustomUserAgent = false;
			settings.Browser.UseQueryParameter = false;
			settings.Browser.UseTemporaryDownAndUploadDirectory = false;

			settings.ConfigurationMode = ConfigurationMode.Exam;

			settings.Display.AllowedDisplays = 1;
			settings.Display.IgnoreError = false;
			settings.Display.InternalDisplayOnly = false;

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

			settings.Proctoring.Enabled = false;
			settings.Proctoring.ForceRaiseHandMessage = false;
			settings.Proctoring.JitsiMeet.AllowChat = false;
			settings.Proctoring.JitsiMeet.AllowClosedCaptions = false;
			settings.Proctoring.JitsiMeet.AllowRaiseHand = false;
			settings.Proctoring.JitsiMeet.AllowRecording = false;
			settings.Proctoring.JitsiMeet.AllowTileView = false;
			settings.Proctoring.JitsiMeet.AudioMuted = true;
			settings.Proctoring.JitsiMeet.AudioOnly = false;
			settings.Proctoring.JitsiMeet.Enabled = false;
			settings.Proctoring.JitsiMeet.ReceiveAudio = false;
			settings.Proctoring.JitsiMeet.ReceiveVideo = false;
			settings.Proctoring.JitsiMeet.SendAudio = true;
			settings.Proctoring.JitsiMeet.SendVideo = true;
			settings.Proctoring.JitsiMeet.ShowMeetingName = false;
			settings.Proctoring.JitsiMeet.VideoMuted = false;
			settings.Proctoring.ShowRaiseHandNotification = true;
			settings.Proctoring.ShowTaskbarNotification = true;
			settings.Proctoring.WindowVisibility = WindowVisibility.Hidden;
			settings.Proctoring.Zoom.AllowChat = false;
			settings.Proctoring.Zoom.AllowClosedCaptions = false;
			settings.Proctoring.Zoom.AllowRaiseHand = false;
			settings.Proctoring.Zoom.AudioMuted = true;
			settings.Proctoring.Zoom.Enabled = false;
			settings.Proctoring.Zoom.ReceiveAudio = false;
			settings.Proctoring.Zoom.ReceiveVideo = false;
			settings.Proctoring.Zoom.SendAudio = true;
			settings.Proctoring.Zoom.SendVideo = true;
			settings.Proctoring.Zoom.VideoMuted = false;

			settings.Security.AllowApplicationLogAccess = false;
			settings.Security.AllowTermination = true;
			settings.Security.AllowReconfiguration = false;
			settings.Security.KioskMode = KioskMode.CreateNewDesktop;
			settings.Security.VirtualMachinePolicy = VirtualMachinePolicy.Deny;

			settings.Server.PingInterval = 1000;
			settings.Server.RequestAttemptInterval = 2000;
			settings.Server.RequestAttempts = 5;
			settings.Server.RequestTimeout = 30000;
			settings.Server.PerformFallback = false;

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
			settings.Service.IgnoreService = true;
			settings.Service.Policy = ServicePolicy.Mandatory;
			settings.Service.SetVmwareConfiguration = false;

			settings.SessionMode = SessionMode.Normal;

			settings.Taskbar.EnableTaskbar = true;
			settings.Taskbar.ShowApplicationInfo = false;
			settings.Taskbar.ShowApplicationLog = false;
			settings.Taskbar.ShowClock = true;
			settings.Taskbar.ShowKeyboardLayout = true;
			settings.Taskbar.ShowNetwork = false;

			settings.UserInterfaceMode = UserInterfaceMode.Desktop;

			return settings;
		}
	}
}
