//
//  SEBSettings.cs
//  SafeExamBrowser
//
//  Copyright (c) 2010-2020 Viktor Tomas, Dirk Bauer, Daniel R. Schneider, Pascal Wyss,
//  ETH Zurich, Educational Development and Technology (LET),
//  based on the original idea of Safe Exam Browser
//  by Stefan Schneider, University of Giessen
//  Project concept: Thomas Piendl, Daniel R. Schneider,
//  Dirk Bauer, Kai Reuter, Tobias Halbherr, Karsten Burger, Marco Lehre,
//  Brigitte Schmucki, Oliver Rahs. French localization: Nicolas Dunand
//
//  ``The contents of this file are subject to the Mozilla Public License
//  Version 1.1 (the "License"); you may not use this file except in
//  compliance with the License. You may obtain a copy of the License at
//  http://www.mozilla.org/MPL/
//
//  Software distributed under the License is distributed on an "AS IS"
//  basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//  License for the specific language governing rights and limitations
//  under the License.
//
//  The Original Code is Safe Exam Browser for Windows.
//
//  The Initial Developers of the Original Code are Viktor Tomas, 
//  Dirk Bauer, Daniel R. Schneider, Pascal Wyss.
//  Portions created by Viktor Tomas, Dirk Bauer, Daniel R. Schneider, Pascal Wyss
//  are Copyright (c) 2010-2020 Viktor Tomas, Dirk Bauer, Daniel R. Schneider, 
//  Pascal Wyss, ETH Zurich, Educational Development and Technology (LET), 
//  based on the original idea of Safe Exam Browser
//  by Stefan Schneider, University of Giessen. All Rights Reserved.
//
//  Contributor(s): ______________________________________.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using SebWindowsConfig.Utilities;
using DictObj = System.Collections.Generic.Dictionary<string, object>;
using KeyValue = System.Collections.Generic.KeyValuePair<string, object>;
using ListObj = System.Collections.Generic.List<object>;

namespace SebWindowsConfig
{
	public class SEBSettings
	{

		// **************************
		// Constants for SEB settings
		// **************************

		// The default SEB configuration file
		public const String DefaultSebConfigXml = "SebClient.xml";
		public const String DefaultSebConfigSeb = "SebClient.seb";

		// Operating systems
		const int IntOSX = 0;
		const int IntWin = 1;

		// Some key/value pairs are not stored in the sebSettings Plist structures,
		// so they must be separately stored in arrays
		public const int ValCryptoIdentity = 1;
		public const int ValMainBrowserWindowWidth = 2;
		public const int ValMainBrowserWindowHeight = 3;
		public const int ValNewBrowserWindowByLinkWidth = 4;
		public const int ValNewBrowserWindowByLinkHeight = 5;
		public const int ValTaskBarHeight = 6;
		public const int ValMinMacOSVersion = 7;
		public const int ValAllowedDisplaysMaxNumber = 8;
		public const int ValNum = 8;

		// Keys not belonging to any group
		public const String KeyOriginatorVersion = "originatorVersion";

		// Group "General"
		public const String KeyStartURL = "startURL";
		public const String KeyStartResource = "startResource";
		public const String KeySebServerURL = "sebServerURL";
		public const String KeyHashedAdminPassword = "hashedAdminPassword";
		public const String KeyAllowQuit = "allowQuit";
		public const String KeyIgnoreExitKeys = "ignoreExitKeys";
		public const String KeyHashedQuitPassword = "hashedQuitPassword";
		public const String KeyExitKey1 = "exitKey1";
		public const String KeyExitKey2 = "exitKey2";
		public const String KeyExitKey3 = "exitKey3";
		public const String KeySebMode = "sebMode";
		public const String KeyBrowserMessagingSocket = "browserMessagingSocket";
		public const String KeyBrowserMessagingPingTime = "browserMessagingPingTime";
		public const String KeyBrowserMessagingSocketEnabled = "browserMessagingSocketEnabled";

		// Group "Config File"
		public const String KeySebConfigPurpose = "sebConfigPurpose";
		public const String KeyAllowPreferencesWindow = "allowPreferencesWindow";
		public const String KeyCryptoIdentity = "cryptoIdentity";
		public const String KeyUseAsymmetricOnlyEncryption = "useAsymmetricOnlyEncryption";

		// Group "User Interface"
		public const String KeyBrowserViewMode = "browserViewMode";
		public const String KeyBrowserWindowAllowAddressBar = "browserWindowAllowAddressBar";
		public const String KeyNewBrowserWindowAllowAddressBar = "newBrowserWindowAllowAddressBar";
		public const String KeyMainBrowserWindowWidth = "mainBrowserWindowWidth";
		public const String KeyMainBrowserWindowHeight = "mainBrowserWindowHeight";
		public const String KeyMainBrowserWindowPositioning = "mainBrowserWindowPositioning";
		public const String KeyEnableBrowserWindowToolbar = "enableBrowserWindowToolbar";
		public const String KeyHideBrowserWindowToolbar = "hideBrowserWindowToolbar";
		public const String KeyShowMenuBar = "showMenuBar";
		public const String KeyShowTaskBar = "showTaskBar";
		public const String KeyShowSideMenu = "showSideMenu";
		public const String KeyTaskBarHeight = "taskBarHeight";
		public const String KeyTouchOptimized = "touchOptimized";
		public const String KeyEnableZoomText = "enableZoomText";
		public const String KeyEnableZoomPage = "enableZoomPage";
		public const String KeyZoomMode = "zoomMode";
		public const String KeyShowReloadButton = "showReloadButton";
		public const String KeyShowTime = "showTime";
		public const String KeyShowInputLanguage = "showInputLanguage";
		public const String KeyAllowDictionaryLookup = "allowDictionaryLookup";
		public const String KeyEnableTouchExit = "enableTouchExit";
		public const String KeyOskBehavior = "oskBehavior";
		public const String KeyAllowDeveloperConsole = "allowDeveloperConsole";

		public const string KeyAllowSpellCheck = "allowSpellCheck";
		public const string KeyAllowSpellCheckDictionary = "allowSpellCheckDictionary";
		public const string KeyAdditionalDictionaries = "additionalDictionaries";
		public const string KeyAdditionalDictionaryData = "dictionaryData";
		public const string KeyAdditionalDictionaryFormat = "dictionaryFormat";
		public const string KeyAdditionalDictionaryLocale = "localeName";

		public const string KeyAudioMute = "audioMute";
		public const string KeyAudioControlEnabled = "audioControlEnabled";
		public const string KeyAudioVolumeLevel = "audioVolumeLevel";
		public const string KeyAudioSetVolumeLevel = "audioSetVolumeLevel";

		//Touch optimized settings
		public const String KeyBrowserScreenKeyboard = "browserScreenKeyboard";

		// Group "Browser"
		public const String KeyNewBrowserWindowByLinkPolicy = "newBrowserWindowByLinkPolicy";
		public const String KeyNewBrowserWindowByScriptPolicy = "newBrowserWindowByScriptPolicy";
		public const String KeyNewBrowserWindowByLinkBlockForeign = "newBrowserWindowByLinkBlockForeign";
		public const String KeyNewBrowserWindowByScriptBlockForeign = "newBrowserWindowByScriptBlockForeign";
		public const String KeyNewBrowserWindowByLinkWidth = "newBrowserWindowByLinkWidth";
		public const String KeyNewBrowserWindowByLinkHeight = "newBrowserWindowByLinkHeight";
		public const String KeyNewBrowserWindowByLinkPositioning = "newBrowserWindowByLinkPositioning";
		public const String KeyNewBrowserWindowUrlPolicy = "newBrowserWindowShowURL";
		public const String KeyMainBrowserWindowUrlPolicy = "browserWindowShowURL";
		public const String KeyEnablePlugIns = "enablePlugIns";
		public const String KeyEnableJava = "enableJava";
		public const String KeyEnableJavaScript = "enableJavaScript";
		public const String KeyBlockPopUpWindows = "blockPopUpWindows";
		public const String KeyAllowVideoCapture = "allowVideoCapture";
		public const String KeyAllowAudioCapture = "allowAudioCapture";
		public const String KeyAllowBrowsingBackForward = "allowBrowsingBackForward";
		public const String KeyNewBrowserWindowNavigation = "newBrowserWindowNavigation";
		public const String KeyRemoveBrowserProfile = "removeBrowserProfile";
		public const String KeyDisableLocalStorage = "removeLocalStorage";
		public const String KeyEnableSebBrowser = "enableSebBrowser";
		public const String KeyBrowserWindowAllowReload = "browserWindowAllowReload";
		public const String KeyNewBrowserWindowAllowReload = "newBrowserWindowAllowReload";
		public const String KeyShowReloadWarning = "showReloadWarning";
		public const String KeyNewBrowserWindowShowReloadWarning = "newBrowserWindowShowReloadWarning";
		public const String KeyBrowserUserAgentDesktopMode = "browserUserAgentWinDesktopMode";
		public const String KeyBrowserUserAgentDesktopModeCustom = "browserUserAgentWinDesktopModeCustom";
		public const String KeyBrowserUserAgentTouchMode = "browserUserAgentWinTouchMode";
		public const String KeyBrowserUserAgentTouchModeIPad = "browserUserAgentWinTouchModeIPad";
		public const String KeyBrowserUserAgentTouchModeCustom = "browserUserAgentWinTouchModeCustom";
		public const String KeyBrowserUserAgent = "browserUserAgent";
		public const String KeyBrowserUserAgentMac = "browserUserAgentMac";
		public const String KeyBrowserUserAgentMacCustom = "browserUserAgentMacCustom";
		public const String KeyBrowserWindowTitleSuffix = "browserWindowTitleSuffix";
		public const String KeyAllowPDFReaderToolbar = "allowPDFReaderToolbar";
		public const String KeyAllowFind = "allowFind";
		public const String KeyAllowPrint = "allowPrint";

		// Group "DownUploads"
		public const String KeyAllowDownUploads = "allowDownUploads";
		public const String KeyAllowCustomDownUploadLocation = "allowCustomDownUploadLocation";
		public const String KeyDownloadDirectoryOSX = "downloadDirectoryOSX";
		public const String KeyDownloadDirectoryWin = "downloadDirectoryWin";
		public const String KeyOpenDownloads = "openDownloads";
		public const String KeyChooseFileToUploadPolicy = "chooseFileToUploadPolicy";
		public const String KeyDownloadPDFFiles = "downloadPDFFiles";
		public const String KeyAllowPDFPlugIn = "allowPDFPlugIn";
		public const String KeyDownloadAndOpenSebConfig = "downloadAndOpenSebConfig";
		public const String KeyBackgroundOpenSEBConfig = "backgroundOpenSEBConfig";
		public const String KeyUseTemporaryDownUploadDirectory = "useTemporaryDownUploadDirectory";
		public const String KeyShowFileSystemElementPath = "browserShowFileSystemElementPath";

		// Group "Exam"
		public const String KeyExamKeySalt = "examKeySalt";
		public const String KeyExamSessionClearCookiesOnEnd = "examSessionClearCookiesOnEnd";
		public const String KeyExamSessionClearCookiesOnStart = "examSessionClearCookiesOnStart";
		public const String KeyBrowserExamKey = "browserExamKey";
		public const String KeyBrowserURLSalt = "browserURLSalt";
		public const String KeySendBrowserExamKey = "sendBrowserExamKey";
		public const String KeyQuitURL = "quitURL";
		public const String KeyQuitURLConfirm = "quitURLConfirm";
		public const String KeyRestartExamText = "restartExamText";
		public const String KeyRestartExamURL = "restartExamURL";
		public const String KeyRestartExamUseStartURL = "restartExamUseStartURL";
		public const String KeyRestartExamPasswordProtected = "restartExamPasswordProtected";
		public const String KeyAllowReconfiguration = "examSessionReconfigureAllow";
		public const String KeyReconfigurationUrl = "examSessionReconfigureConfigURL";
		public const String KeyResetOnQuitUrl = "quitURLRestart";
		public const String KeyUseStartUrlQuery = "startURLAppendQueryParameter";

		// Group Additional Resources
		public const String KeyAdditionalResources = "additionalResources";
		public const String KeyAdditionalResourcesActive = "active";
		public const String KeyAdditionalResourcesAutoOpen = "autoOpen";
		public const String KeyAdditionalResourcesIdentifier = "identifier";
		public const String KeyAdditionalResourcesTitle = "title";
		public const String KeyAdditionalResourcesUrl = "url";
		public const String KeyAdditionalResourcesURLFilterRules = KeyURLFilterRules;
		public const String KeyAdditionalResourcesResourceData = "resourceData";
		public const String KeyAdditionalResourcesResourceDataLauncher = "resourceDataLauncher";
		public const String KeyAdditionalResourcesResourceDataFilename = "resourceDataFilename";
		public const String KeyAdditionalResourcesResourceIcons = "resourceIcons";
		public const String KeyAdditionalResourcesResourceIconsFormat = "format";
		public const String KeyAdditionalResourcesResourceIconsResolution = "resolution";
		public const String KeyAdditionalResourcesResourceIconsIconData = "iconData";
		public const String KeyAdditionalResourcesLinkUrl = "linkURL";
		public const String KeyAdditionalResourcesRefererFilter = "refererFilter";
		public const String KeyAdditionalResourcesResetSession = "resetSession";
		public const String KeyAdditionalResourcesKey = "key";
		public const String KeyAdditionalResourcesModifiers = "modifiers";
		public const String KeyAdditionalResourcesConfirm = "confirm";
		public const String KeyAdditionalResourcesConfirmText = "confirmText";
		public const String KeyAdditionalResourcesShowButton = "showButton";

		// Group "Applications"
		public const String KeyMonitorProcesses = "monitorProcesses";

		// Group "Applications - Permitted  Processes"
		public const String KeyPermittedProcesses = "permittedProcesses";
		public const String KeyAllowSwitchToApplications = "allowSwitchToApplications";
		public const String KeyAllowFlashFullscreen = "allowFlashFullscreen";

		// Group "Applications - Prohibited Processes"
		public const String KeyProhibitedProcesses = "prohibitedProcesses";

		public const String KeyActive = "active";
		public const String KeyAutostart = "autostart";
		public const String KeyIconInTaskbar = "iconInTaskbar";
		public const String KeyRunInBackground = "runInBackground";
		public const String KeyAllowUser = "allowUserToChooseApp";
		public const String KeyCurrentUser = "currentUser";
		public const String KeyStrongKill = "strongKill";
		public const String KeyOS = "os";
		public const String KeyTitle = "title";
		public const String KeyDescription = "description";
		public const String KeyExecutable = "executable";
		public const String KeyOriginalName = "originalName";
		public const String KeyPath = "path";
		public const String KeyIdentifier = "identifier";
		public const String KeyUser = "user";
		public const String KeyArguments = "arguments";
		public const String KeyArgument = "argument";
		public const String KeyWindowHandlingProcess = "windowHandlingProcess";

		// Group "Network"
		public const String KeyEnableURLFilter = "enableURLFilter";
		public const String KeyEnableURLContentFilter = "enableURLContentFilter";

		// New "Network" - Filter
		public const String KeyURLFilterEnable = "URLFilterEnable";
		public const String KeyURLFilterEnableContentFilter = "URLFilterEnableContentFilter";
		public const String KeyURLFilterRules = "URLFilterRules";
		public const String KeyURLFilterRuleAction = "action";
		public const String KeyURLFilterRuleActive = "active";
		public const String KeyURLFilterRuleExpression = "expression";
		public const String KeyURLFilterRuleRegex = "regex";

		//Group "Network" - URL Filter XULRunner keys
		public const String KeyUrlFilterBlacklist = "blacklistURLFilter";
		public const String KeyUrlFilterWhitelist = "whitelistURLFilter";
		public const String KeyUrlFilterTrustedContent = "urlFilterTrustedContent";
		public const String KeyUrlFilterRulesAsRegex = "urlFilterRegex";

		// Group "Network - Certificates"
		//public const String KeyEmbedSSLServerCertificate = "EmbedSSLServerCertificate";
		//public const String KeyEmbedIdentity             = "EmbedIdentity";
		public const String KeyEmbeddedCertificates = "embeddedCertificates";
		public const String KeyCertificateData = "certificateData";
		public const String KeyCertificateDataBase64 = "certificateDataBase64";
		public const String KeyCertificateDataWin = "certificateDataWin";
		public const String KeyType = "type";
		public const String KeyName = "name";
		public const String KeyPinEmbeddedCertificates = "pinEmbeddedCertificates";

		// Group "Network - Proxies"
		public const String KeyProxySettingsPolicy = "proxySettingsPolicy";

		public const String KeyProxies = "proxies";
		public const String KeyExceptionsList = "ExceptionsList";
		public const String KeyExcludeSimpleHostnames = "ExcludeSimpleHostnames";
		public const String KeyFTPPassive = "FTPPassive";

		public const String KeyAutoDiscoveryEnabled = "AutoDiscoveryEnabled";
		public const String KeyAutoConfigurationEnabled = "AutoConfigurationEnabled";
		public const String KeyAutoConfigurationJavaScript = "AutoConfigurationJavaScript";
		public const String KeyAutoConfigurationURL = "AutoConfigurationURL";

		public const String KeyAutoDiscovery = "";
		public const String KeyAutoConfiguration = "";
		public const String KeyHTTP = "HTTP";
		public const String KeyHTTPS = "HTTPS";
		public const String KeyFTP = "FTP";
		public const String KeySOCKS = "SOCKS";
		public const String KeyRTSP = "RTSP";

		public const String KeyEnable = "Enable";
		public const String KeyPort = "Port";
		public const String KeyHost = "Proxy";
		public const String KeyRequires = "RequiresPassword";
		public const String KeyUsername = "Username";
		public const String KeyPassword = "Password";

		public const String KeyHTTPEnable = "HTTPEnable";
		public const String KeyHTTPPort = "HTTPPort";
		public const String KeyHTTPHost = "HTTPProxy";
		public const String KeyHTTPRequires = "HTTPRequiresPassword";
		public const String KeyHTTPUsername = "HTTPUsername";
		public const String KeyHTTPPassword = "HTTPPassword";

		public const String KeyHTTPSEnable = "HTTPSEnable";
		public const String KeyHTTPSPort = "HTTPSPort";
		public const String KeyHTTPSHost = "HTTPSProxy";
		public const String KeyHTTPSRequires = "HTTPSRequiresPassword";
		public const String KeyHTTPSUsername = "HTTPSUsername";
		public const String KeyHTTPSPassword = "HTTPSPassword";

		public const String KeyFTPEnable = "FTPEnable";
		public const String KeyFTPPort = "FTPPort";
		public const String KeyFTPHost = "FTPProxy";
		public const String KeyFTPRequires = "FTPRequiresPassword";
		public const String KeyFTPUsername = "FTPUsername";
		public const String KeyFTPPassword = "FTPPassword";

		public const String KeySOCKSEnable = "SOCKSEnable";
		public const String KeySOCKSPort = "SOCKSPort";
		public const String KeySOCKSHost = "SOCKSProxy";
		public const String KeySOCKSRequires = "SOCKSRequiresPassword";
		public const String KeySOCKSUsername = "SOCKSUsername";
		public const String KeySOCKSPassword = "SOCKSPassword";

		public const String KeyRTSPEnable = "RTSPEnable";
		public const String KeyRTSPPort = "RTSPPort";
		public const String KeyRTSPHost = "RTSPProxy";
		public const String KeyRTSPRequires = "RTSPRequiresPassword";
		public const String KeyRTSPUsername = "RTSPUsername";
		public const String KeyRTSPPassword = "RTSPPassword";

		// Tab "Security"
		public const String KeySebServicePolicy = "sebServicePolicy";
		public const String KeySebServiceIgnore = "sebServiceIgnore";
		public const String KeyAllowVirtualMachine = "allowVirtualMachine";
		public const String KeyAllowScreenSharing = "allowScreenSharing";
		public const String KeyEnablePrivateClipboard = "enablePrivateClipboard";

		public const String KeyCreateNewDesktop = "createNewDesktop";
		public const String KeyKillExplorerShell = "killExplorerShell";
		public const String KeyAllowUserSwitching = "allowUserSwitching";
		public const String KeyEnableLogging = "enableLogging";
		public const String KeyAllowApplicationLog = "allowApplicationLog";
		public const String KeyShowApplicationLogButton = "showApplicationLogButton";
		public const String KeyLogDirectoryOSX = "logDirectoryOSX";
		public const String KeyLogDirectoryWin = "logDirectoryWin";
		public const String KeyAllowWLAN = "allowWlan";
		public const String KeyLockOnMessageSocketClose = "lockOnMessageSocketClose";
		public const String KeyAllowChromeNotifications = "enableChromeNotifications";
		public const String KeyAllowWindowsUpdate = "enableWindowsUpdate";
		// Group "macOS specific settings"
		public const String KeyMinMacOSVersion = "minMacOSVersion";
		public const String KeyEnableAppSwitcherCheck = "enableAppSwitcherCheck";
		public const String KeyForceAppFolderInstall = "forceAppFolderInstall";
		public const String KeyAllowUserAppFolderInstall = "allowUserAppFolderInstall";
		public const String KeyAllowSiri = "allowSiri";
		public const String KeyAllowDictation = "allowDictation";
		public const String KeyDetectStoppedProcess = "detectStoppedProcess";
		public const String KeyAllowDisplayMirroring = "allowDisplayMirroring";
		public const String KeyAllowedDisplaysMaxNumber = "allowedDisplaysMaxNumber";
		public const String KeyAllowedDisplayBuiltin = "allowedDisplayBuiltin";
		public const String KeyAllowedDisplayBuiltinEnforce = "allowedDisplayBuiltinEnforce";
		public const String KeyAllowedDisplayIgnoreFailure = "allowedDisplaysIgnoreFailure";

		// Group "Registry"

		// Group "Inside SEB"
		public const String KeyInsideSebEnableSwitchUser = "insideSebEnableSwitchUser";
		public const String KeyInsideSebEnableLockThisComputer = "insideSebEnableLockThisComputer";
		public const String KeyInsideSebEnableChangeAPassword = "insideSebEnableChangeAPassword";
		public const String KeyInsideSebEnableStartTaskManager = "insideSebEnableStartTaskManager";
		public const String KeyInsideSebEnableLogOff = "insideSebEnableLogOff";
		public const String KeyInsideSebEnableShutDown = "insideSebEnableShutDown";
		public const String KeyInsideSebEnableEaseOfAccess = "insideSebEnableEaseOfAccess";
		public const String KeyInsideSebEnableVmWareClientShade = "insideSebEnableVmWareClientShade";
		public const String KeyInsideSebEnableNetworkConnectionSelector = "insideSebEnableNetworkConnectionSelector";
		public const String KeySetVmwareConfiguration = "setVmwareConfiguration";
		public const String KeyEnableFindPrinter = "enableFindPrinter";

		// Group "Hooked Keys"
		public const String KeyHookKeys = "hookKeys";

		// Group "Special Keys"
		public const String KeyEnableEsc = "enableEsc";
		public const String KeyEnablePrintScreen = "enablePrintScreen";
		public const String KeyEnableCtrlEsc = "enableCtrlEsc";
		public const String KeyEnableAltEsc = "enableAltEsc";
		public const String KeyEnableAltTab = "enableAltTab";
		public const String KeyEnableAltF4 = "enableAltF4";
		public const String KeyEnableStartMenu = "enableStartMenu";
		public const String KeyEnableMiddleMouse = "enableMiddleMouse";
		public const String KeyEnableRightMouse = "enableRightMouse";
		public const String KeyEnableAltMouseWheel = "enableAltMouseWheel";

		// Group "Function Keys"
		public const String KeyEnableF1 = "enableF1";
		public const String KeyEnableF2 = "enableF2";
		public const String KeyEnableF3 = "enableF3";
		public const String KeyEnableF4 = "enableF4";
		public const String KeyEnableF5 = "enableF5";
		public const String KeyEnableF6 = "enableF6";
		public const String KeyEnableF7 = "enableF7";
		public const String KeyEnableF8 = "enableF8";
		public const String KeyEnableF9 = "enableF9";
		public const String KeyEnableF10 = "enableF10";
		public const String KeyEnableF11 = "enableF11";
		public const String KeyEnableF12 = "enableF12";

		public enum sebConfigPurposes
		{
			sebConfigPurposeStartingExam, sebConfigPurposeConfiguringClient
		}

		public enum operatingSystems
		{
			operatingSystemOSX, operatingSystemWin
		}

		public enum DictionaryFormat
		{
			Mozilla = 0
		}

		// *********************************
		// Global Variables for SEB settings
		// *********************************

		// Some settings are not stored in Plists but in Arrays
		public static String[] strArrayDefault = new String[ValNum + 1];
		public static String[] strArrayCurrent = new String[ValNum + 1];

		public static int[] intArrayDefault = new int[ValNum + 1];
		public static int[] intArrayCurrent = new int[ValNum + 1];

		// Class SEBSettings contains all settings
		// and is used for importing/exporting the settings
		// from/to a human-readable .xml and an encrypted.seb file format.
		public static DictObj settingsDefault = new DictObj();
		public static DictObj settingsCurrent = new DictObj();
		public static DictObj settingsCurrentOriginal = new DictObj();

		public static int permittedProcessIndex;
		public static ListObj permittedProcessList = new ListObj();
		public static DictObj permittedProcessData = new DictObj();
		public static DictObj permittedProcessDataDefault = new DictObj();

		public static int permittedArgumentIndex;
		public static ListObj permittedArgumentList = new ListObj();
		public static DictObj permittedArgumentData = new DictObj();
		public static DictObj permittedArgumentDataDefault = new DictObj();
		public static DictObj permittedArgumentDataXulRunner1 = new DictObj();
		public static DictObj permittedArgumentDataXulRunner2 = new DictObj();
		public static ListObj permittedArgumentListXulRunner = new ListObj();

		public static ListObj additionalResourcesList = new ListObj();
		public static DictObj additionalResourcesData = new DictObj();
		public static DictObj additionalResourcesDataDefault = new DictObj();

		public static int prohibitedProcessIndex;
		public static ListObj prohibitedProcessList = new ListObj();
		public static DictObj prohibitedProcessData = new DictObj();
		public static DictObj prohibitedProcessDataDefault = new DictObj();
		private static List<string> prohibitedProcessesDefault;
		private static List<string> prohibitedProcessesDefaultStrict;

		public static int urlFilterRuleIndex;
		public static ListObj urlFilterRuleList = new ListObj();
		public static DictObj urlFilterRuleData = new DictObj();
		public static DictObj urlFilterRuleDataDefault = new DictObj();
		public static DictObj urlFilterRuleDataStorage = new DictObj();

		public static int urlFilterActionIndex;
		public static ListObj urlFilterActionList = new ListObj();
		public static ListObj urlFilterActionListDefault = new ListObj();
		public static ListObj urlFilterActionListStorage = new ListObj();
		public static DictObj urlFilterActionData = new DictObj();
		public static DictObj urlFilterActionDataDefault = new DictObj();
		public static DictObj urlFilterActionDataStorage = new DictObj();

		public static int embeddedCertificateIndex;
		public static ListObj embeddedCertificateList = new ListObj();
		public static DictObj embeddedCertificateData = new DictObj();
		public static DictObj embeddedCertificateDataDefault = new DictObj();

		public static DictObj proxiesData = new DictObj();
		public static DictObj proxiesDataDefault = new DictObj();

		public static int proxyProtocolIndex;

		public static int bypassedProxyIndex;
		public static ListObj bypassedProxyList = new ListObj();
		public static String bypassedProxyData = "";
		public static String bypassedProxyDataDefault = "";

		public static object SebWindowsConfigForm { get; private set; }


		// ************************
		// Methods for SEB settings
		// ************************


		// ********************************************************************
		// Set all the default values for the Plist structure "settingsDefault"
		// ********************************************************************
		public static void CreateDefaultAndCurrentSettingsFromScratch()
		{
			// Destroy all default lists and dictionaries
			SEBSettings.settingsDefault = new DictObj();
			SEBSettings.settingsCurrent = new DictObj();

			SEBSettings.permittedProcessList = new ListObj();
			SEBSettings.permittedProcessData = new DictObj();
			SEBSettings.permittedProcessDataDefault = new DictObj();

			SEBSettings.permittedArgumentList = new ListObj();
			SEBSettings.permittedArgumentData = new DictObj();
			SEBSettings.permittedArgumentDataDefault = new DictObj();
			SEBSettings.permittedArgumentDataXulRunner1 = new DictObj();
			SEBSettings.permittedArgumentDataXulRunner2 = new DictObj();
			SEBSettings.permittedArgumentListXulRunner = new ListObj();

			SEBSettings.prohibitedProcessList = new ListObj();
			SEBSettings.prohibitedProcessData = new DictObj();
			SEBSettings.prohibitedProcessDataDefault = new DictObj();

			SEBSettings.additionalResourcesList = new ListObj();
			SEBSettings.additionalResourcesData = new DictObj();
			SEBSettings.additionalResourcesDataDefault = new DictObj();

			SEBSettings.urlFilterRuleList = new ListObj();
			SEBSettings.urlFilterRuleData = new DictObj();
			SEBSettings.urlFilterRuleDataDefault = new DictObj();
			SEBSettings.urlFilterRuleDataStorage = new DictObj();

			SEBSettings.urlFilterActionList = new ListObj();
			SEBSettings.urlFilterActionListDefault = new ListObj();
			SEBSettings.urlFilterActionListStorage = new ListObj();
			SEBSettings.urlFilterActionData = new DictObj();
			SEBSettings.urlFilterActionDataDefault = new DictObj();
			SEBSettings.urlFilterActionDataStorage = new DictObj();

			SEBSettings.embeddedCertificateList = new ListObj();
			SEBSettings.embeddedCertificateData = new DictObj();
			SEBSettings.embeddedCertificateDataDefault = new DictObj();

			SEBSettings.proxiesData = new DictObj();
			SEBSettings.proxiesDataDefault = new DictObj();

			SEBSettings.bypassedProxyList = new ListObj();
			SEBSettings.bypassedProxyData = "";
			SEBSettings.bypassedProxyDataDefault = "";


			// Initialise the global arrays
			for (int value = 1; value <= ValNum; value++)
			{
				SEBSettings.intArrayDefault[value] = 0;
				SEBSettings.intArrayCurrent[value] = 0;

				SEBSettings.strArrayDefault[value] = "";
				SEBSettings.strArrayCurrent[value] = "";
			}

			// Initialise the default settings Plist
			SEBSettings.settingsDefault.Clear();

			// Default settings for keys not belonging to any group
			SEBSettings.settingsDefault.Add(SEBSettings.KeyOriginatorVersion, "SEB_Win_2.1.1");

			// Default settings for group "General"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyStartURL, "https://safeexambrowser.org/start");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyStartResource, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeySebServerURL, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyHashedAdminPassword, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowQuit, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyIgnoreExitKeys, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyHashedQuitPassword, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyExitKey1, 2);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyExitKey2, 10);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyExitKey3, 5);
			SEBSettings.settingsDefault.Add(SEBSettings.KeySebMode, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserMessagingSocket, "ws://localhost:8706");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserMessagingPingTime, 120000);

			// Default settings for group "Config File"
			SEBSettings.settingsDefault.Add(SEBSettings.KeySebConfigPurpose, 1);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowPreferencesWindow, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUseAsymmetricOnlyEncryption, false);

			// CryptoIdentity is stored additionally
			SEBSettings.intArrayDefault[SEBSettings.ValCryptoIdentity] = 0;
			SEBSettings.strArrayDefault[SEBSettings.ValCryptoIdentity] = "";

			// Default settings for group "User Interface"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserViewMode, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserWindowAllowAddressBar, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowAllowAddressBar, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyMainBrowserWindowWidth, "100%");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyMainBrowserWindowHeight, "100%");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyMainBrowserWindowPositioning, 1);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableBrowserWindowToolbar, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyHideBrowserWindowToolbar, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowMenuBar, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowTaskBar, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowSideMenu, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyTaskBarHeight, 40);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyTouchOptimized, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableZoomText, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableZoomPage, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyZoomMode, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowSpellCheck, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowDictionaryLookup, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowSpellCheckDictionary, new ListObj());
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAdditionalDictionaries, new ListObj());
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowReloadButton, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowTime, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowInputLanguage, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableTouchExit, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyOskBehavior, 2);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAudioControlEnabled, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAudioMute, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAudioVolumeLevel, 25);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAudioSetVolumeLevel, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowDeveloperConsole, false);

			//Touch Settings
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserScreenKeyboard, false);

			// MainBrowserWindow Width and Height is stored additionally
			SEBSettings.intArrayDefault[SEBSettings.ValMainBrowserWindowWidth] = 2;
			SEBSettings.intArrayDefault[SEBSettings.ValMainBrowserWindowHeight] = 2;
			SEBSettings.strArrayDefault[SEBSettings.ValMainBrowserWindowWidth] = "100%";
			SEBSettings.strArrayDefault[SEBSettings.ValMainBrowserWindowHeight] = "100%";

			// Default settings for group "Browser"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByLinkPolicy, 2);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByScriptPolicy, 2);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByLinkBlockForeign, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByScriptBlockForeign, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByLinkWidth, "1000");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByLinkHeight, "100%");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowByLinkPositioning, 2);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowUrlPolicy, 0);

			SEBSettings.settingsDefault.Add(SEBSettings.KeyMainBrowserWindowUrlPolicy, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnablePlugIns, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableJava, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableJavaScript, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBlockPopUpWindows, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowVideoCapture, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowAudioCapture, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowBrowsingBackForward, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowNavigation, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyRemoveBrowserProfile, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyDisableLocalStorage, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableSebBrowser, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserWindowAllowReload, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowAllowReload, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowReloadWarning, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyNewBrowserWindowShowReloadWarning, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentDesktopMode, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentDesktopModeCustom, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentTouchMode, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentTouchModeIPad, SEBClientInfo.BROWSER_USERAGENT_TOUCH_IPAD);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentTouchModeCustom, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgent, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentMac, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserUserAgentMacCustom, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserWindowTitleSuffix, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowPDFReaderToolbar, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowFind, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowPrint, false);
			// NewBrowserWindow Width and Height is stored additionally
			SEBSettings.intArrayDefault[SEBSettings.ValNewBrowserWindowByLinkWidth] = 4;
			SEBSettings.intArrayDefault[SEBSettings.ValNewBrowserWindowByLinkHeight] = 2;
			SEBSettings.strArrayDefault[SEBSettings.ValNewBrowserWindowByLinkWidth] = "1000";
			SEBSettings.strArrayDefault[SEBSettings.ValNewBrowserWindowByLinkHeight] = "100%";

			// Default settings for group "DownUploads"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowDownUploads, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowCustomDownUploadLocation, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyDownloadDirectoryOSX, "~/Downloads");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyDownloadDirectoryWin, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyOpenDownloads, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyChooseFileToUploadPolicy, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyDownloadPDFFiles, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowPDFPlugIn, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyDownloadAndOpenSebConfig, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBackgroundOpenSEBConfig, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUseTemporaryDownUploadDirectory, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowFileSystemElementPath, true);

			// Default settings for group "Exam"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyExamKeySalt, new Byte[] { });
			SEBSettings.settingsDefault.Add(SEBSettings.KeyExamSessionClearCookiesOnEnd, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyExamSessionClearCookiesOnStart, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserExamKey, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyBrowserURLSalt, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeySendBrowserExamKey, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyQuitURL, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyQuitURLConfirm, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyRestartExamURL, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyRestartExamUseStartURL, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyRestartExamText, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyRestartExamPasswordProtected, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowReconfiguration, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyReconfigurationUrl, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyResetOnQuitUrl, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUseStartUrlQuery, false);

			// Default settings for group "Additional Resources"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAdditionalResources, new ListObj());

			// Default settings for group "Applications"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyMonitorProcesses, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowSwitchToApplications, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowFlashFullscreen, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyPermittedProcesses, new ListObj());
			SEBSettings.settingsDefault.Add(SEBSettings.KeyProhibitedProcesses, new ListObj());

			// Default settings for permitted argument data
			SEBSettings.permittedArgumentDataDefault.Clear();
			SEBSettings.permittedArgumentDataDefault.Add(SEBSettings.KeyActive, true);
			SEBSettings.permittedArgumentDataDefault.Add(SEBSettings.KeyArgument, "");

			// Define the XulRunner arguments
			//SEBSettings.permittedArgumentDataXulRunner1.Clear();
			//SEBSettings.permittedArgumentDataXulRunner1.Add(SEBSettings.KeyActive, true);
			//SEBSettings.permittedArgumentDataXulRunner1.Add(SEBSettings.KeyArgument, "-app \"..\\xul_seb\\seb.ini\"");

			//SEBSettings.permittedArgumentDataXulRunner2.Clear();
			//SEBSettings.permittedArgumentDataXulRunner2.Add(SEBSettings.KeyActive, true);
			//SEBSettings.permittedArgumentDataXulRunner2.Add(SEBSettings.KeyArgument, "-profile \"%LOCALAPPDATA%\\ETH Zuerich\\xul_seb\\Profiles\"");

			// Create the XulRunner argument list with the XulRunner arguments
			SEBSettings.permittedArgumentListXulRunner.Clear();
			SEBSettings.permittedArgumentListXulRunner.Add(SEBSettings.permittedArgumentDataDefault);
			//SEBSettings.permittedArgumentListXulRunner.Add(SEBSettings.permittedArgumentDataXulRunner1);
			//SEBSettings.permittedArgumentListXulRunner.Add(SEBSettings.permittedArgumentDataXulRunner2);

			// Default settings for permitted process data
			SEBSettings.permittedProcessDataDefault.Clear();
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyActive, true);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyAutostart, true);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyIconInTaskbar, true);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyRunInBackground, false);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyAllowUser, false);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyStrongKill, false);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyOS, IntWin);
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyTitle, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyDescription, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyExecutable, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyOriginalName, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyPath, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyIdentifier, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyWindowHandlingProcess, "");
			SEBSettings.permittedProcessDataDefault.Add(SEBSettings.KeyArguments, new ListObj());

			// Default settings for prohibited process data
			SEBSettings.prohibitedProcessDataDefault.Clear();
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyActive, true);
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyCurrentUser, true);
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyStrongKill, false);
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyOS, IntWin);
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyExecutable, "");
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyOriginalName, "");
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyDescription, "");
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyIdentifier, "");
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyWindowHandlingProcess, "");
			SEBSettings.prohibitedProcessDataDefault.Add(SEBSettings.KeyUser, "");

			SEBSettings.prohibitedProcessesDefault = new List<string>
			{
				"Chrome.exe",
				"Chromium.exe",
				"Vivaldi.exe",
				"Opera.exe",
				"browser.exe",
				"slimjet.exe",
				"UCBrowser.exe",
				"CamRecorder.exe",
				"Firefox.exe"
			};
			SEBSettings.prohibitedProcessesDefaultStrict = new List<string>
			{
				"Skype.exe",
				"SkypeApp.exe",
				"SkypeHost.exe",
				"g2mcomm.exe",
				"GotoMeetingWinStore.exe",
				"TeamViewer.exe",
				"vncserver.exe",
				"vncviewer.exe",
				"vncserverui.exe",
				"chromoting.exe",
				"Mikogo-host.exe",
				"AeroAdmin.exe",
				"beamyourscreen-host.exe",
				"RemotePCDesktop.exe",
				"RPCService.exe",
				"RPCSuite.exe",
				"Discord.exe",
				"Camtasia.exe",
				"CamtasiaStudio.exe",
				"Camtasia_Studio.exe",
				"CamPlay.exe",
				"CamRecorder.exe",
				"CamtasiaUtl.exe",
				"slack.exe",
				"Element.exe",
				"Zoom.exe",
				"Telegram.exe",
				"g2mcomm.exe",
				"g2mlauncher.exe",
				"g2mstart.exe",
				"join.me.exe",
				"join.me.sentinel.exe",
				"Teams.exe",
				"webexmta.exe",
				"ptoneclk.exe",
				"AA_v3.exe",
				"CiscoCollabHost.exe",
				"CiscoWebExStart.exe",
				"remoting_host.exe"
			};

			// Default settings for group "Network - Filter"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableURLFilter, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableURLContentFilter, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyURLFilterRules, new ListObj());

			//// Create a default action
			//SEBSettings.urlFilterActionDataDefault.Clear();
			//SEBSettings.urlFilterActionDataDefault.Add(SEBSettings.KeyActive    , true);
			//SEBSettings.urlFilterActionDataDefault.Add(SEBSettings.KeyRegex     , false);
			//SEBSettings.urlFilterActionDataDefault.Add(SEBSettings.KeyExpression, "*");
			//SEBSettings.urlFilterActionDataDefault.Add(SEBSettings.KeyAction    , 0);

			//// Create a default action list with one entry (the default action)
			//SEBSettings.urlFilterActionListDefault.Clear();
			//SEBSettings.urlFilterActionListDefault.Add(SEBSettings.urlFilterActionDataDefault);

			//// Create a default rule with this default action list.
			//// This default rule is used for the "Insert Rule" operation:
			//// when a new rule is created, it initially contains one action.
			//SEBSettings.urlFilterRuleDataDefault.Clear();
			//SEBSettings.urlFilterRuleDataDefault.Add(SEBSettings.KeyActive     , true);
			//SEBSettings.urlFilterRuleDataDefault.Add(SEBSettings.KeyExpression , "Rule");
			//SEBSettings.urlFilterRuleDataDefault.Add(SEBSettings.KeyRuleActions, SEBSettings.urlFilterActionListDefault);

			//// Initialise the stored action
			//SEBSettings.urlFilterActionDataStorage.Clear();
			//SEBSettings.urlFilterActionDataStorage.Add(SEBSettings.KeyActive    , true);
			//SEBSettings.urlFilterActionDataStorage.Add(SEBSettings.KeyRegex     , false);
			//SEBSettings.urlFilterActionDataStorage.Add(SEBSettings.KeyExpression, "*");
			//SEBSettings.urlFilterActionDataStorage.Add(SEBSettings.KeyAction    , 0);

			//// Initialise the stored action list with no entry
			//SEBSettings.urlFilterActionListStorage.Clear();

			//// Initialise the stored rule
			//SEBSettings.urlFilterRuleDataStorage.Clear();
			//SEBSettings.urlFilterRuleDataStorage.Add(SEBSettings.KeyActive     , true);
			//SEBSettings.urlFilterRuleDataStorage.Add(SEBSettings.KeyExpression , "Rule");
			//SEBSettings.urlFilterRuleDataStorage.Add(SEBSettings.KeyRuleActions, SEBSettings.urlFilterActionListStorage);

			// Default settings for group "Network - Filter"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyURLFilterEnable, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyURLFilterEnableContentFilter, false);
			//SEBSettings.settingsDefault.Add(SEBSettings.KeyURLFilterRules, new ListObj());

			//Group "Network" - URL Filter XULRunner keys
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUrlFilterBlacklist, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUrlFilterWhitelist, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUrlFilterTrustedContent, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyUrlFilterRulesAsRegex, false);

			// Default settings for group "Network - Certificates"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEmbeddedCertificates, new ListObj());

			SEBSettings.embeddedCertificateDataDefault.Clear();
			SEBSettings.embeddedCertificateDataDefault.Add(SEBSettings.KeyCertificateData, new Byte[0]);
			SEBSettings.embeddedCertificateDataDefault.Add(SEBSettings.KeyCertificateDataBase64, "");
			SEBSettings.embeddedCertificateDataDefault.Add(SEBSettings.KeyCertificateDataWin, "");
			SEBSettings.embeddedCertificateDataDefault.Add(SEBSettings.KeyType, 0);
			SEBSettings.embeddedCertificateDataDefault.Add(SEBSettings.KeyName, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyPinEmbeddedCertificates, false);

			// Default settings for group "Network - Proxies"
			SEBSettings.proxiesDataDefault.Clear();

			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyExceptionsList, new ListObj());
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyExcludeSimpleHostnames, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyAutoDiscoveryEnabled, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyAutoConfigurationEnabled, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyAutoConfigurationJavaScript, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyAutoConfigurationURL, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPPassive, true);

			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPEnable, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPPort, 80);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPHost, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPRequires, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPUsername, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPPassword, "");

			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPSEnable, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPSPort, 443);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPSHost, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPSRequires, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPSUsername, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyHTTPSPassword, "");

			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPEnable, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPPort, 21);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPHost, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPRequires, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPUsername, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyFTPPassword, "");

			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeySOCKSEnable, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeySOCKSPort, 1080);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeySOCKSHost, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeySOCKSRequires, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeySOCKSUsername, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeySOCKSPassword, "");

			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyRTSPEnable, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyRTSPPort, 554);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyRTSPHost, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyRTSPRequires, false);
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyRTSPUsername, "");
			SEBSettings.proxiesDataDefault.Add(SEBSettings.KeyRTSPPassword, "");

			SEBSettings.bypassedProxyDataDefault = "";

			SEBSettings.settingsDefault.Add(SEBSettings.KeyProxySettingsPolicy, 0);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyProxies, SEBSettings.proxiesDataDefault);

			// Default settings for group "Security"
			SEBSettings.settingsDefault.Add(SEBSettings.KeySebServicePolicy, 1);
			SEBSettings.settingsDefault.Add(SEBSettings.KeySebServiceIgnore, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowVirtualMachine, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowScreenSharing, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnablePrivateClipboard, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyCreateNewDesktop, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyKillExplorerShell, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableLogging, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowApplicationLog, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyShowApplicationLogButton, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyLogDirectoryOSX, "~/Documents");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyLogDirectoryWin, "");
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowWLAN, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyLockOnMessageSocketClose, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyMinMacOSVersion, 4);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableAppSwitcherCheck, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyForceAppFolderInstall, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowUserAppFolderInstall, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowSiri, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowDictation, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyDetectStoppedProcess, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowDisplayMirroring, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowedDisplaysMaxNumber, 1);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowedDisplayBuiltin, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowedDisplayBuiltinEnforce, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowedDisplayIgnoreFailure, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowChromeNotifications, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyAllowWindowsUpdate, false);

			// Default selected index and string in combo box for minMacOSVersion 
			SEBSettings.intArrayDefault[SEBSettings.ValMinMacOSVersion] = 4;
			SEBSettings.strArrayDefault[SEBSettings.ValMinMacOSVersion] = "OS X 10.11 El Capitan";

			// Default selected index and string in combo box for allowedDisplaysMaxNumber
			SEBSettings.intArrayDefault[SEBSettings.ValAllowedDisplaysMaxNumber] = 0;
			SEBSettings.strArrayDefault[SEBSettings.ValAllowedDisplaysMaxNumber] = "1";

			// Default settings for group "Inside SEB"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableSwitchUser, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableLockThisComputer, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableChangeAPassword, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableStartTaskManager, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableLogOff, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableShutDown, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableEaseOfAccess, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableVmWareClientShade, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyInsideSebEnableNetworkConnectionSelector, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeySetVmwareConfiguration, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableFindPrinter, false);

			// Default settings for group "Hooked Keys"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyHookKeys, true);

			// Default settings for group "Special Keys"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableEsc, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableCtrlEsc, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableAltEsc, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableAltTab, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableAltF4, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableStartMenu, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableMiddleMouse, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableRightMouse, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnablePrintScreen, false);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableAltMouseWheel, false);

			// Default settings for group "Function Keys"
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF1, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF2, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF3, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF4, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF5, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF6, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF7, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF8, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF9, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF10, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF11, true);
			SEBSettings.settingsDefault.Add(SEBSettings.KeyEnableF12, true);


			// Clear all "current" lists and dictionaries

			SEBSettings.permittedProcessIndex = -1;
			SEBSettings.permittedProcessList.Clear();
			SEBSettings.permittedProcessData.Clear();

			SEBSettings.permittedArgumentIndex = -1;
			SEBSettings.permittedArgumentList.Clear();
			SEBSettings.permittedArgumentData.Clear();

			SEBSettings.prohibitedProcessIndex = -1;
			SEBSettings.prohibitedProcessList.Clear();
			SEBSettings.prohibitedProcessData.Clear();

			SEBSettings.additionalResourcesList.Clear();
			SEBSettings.additionalResourcesData.Clear();

			SEBSettings.urlFilterRuleIndex = -1;
			SEBSettings.urlFilterRuleList.Clear();
			SEBSettings.urlFilterRuleData.Clear();

			SEBSettings.urlFilterActionIndex = -1;
			SEBSettings.urlFilterActionList.Clear();
			SEBSettings.urlFilterActionData.Clear();

			SEBSettings.embeddedCertificateIndex = -1;
			SEBSettings.embeddedCertificateList.Clear();
			SEBSettings.embeddedCertificateData.Clear();

			SEBSettings.proxyProtocolIndex = -1;
			SEBSettings.proxiesData.Clear();

			SEBSettings.bypassedProxyIndex = -1;
			SEBSettings.bypassedProxyList.Clear();
			SEBSettings.bypassedProxyData = "";
		}



		// *****************************************
		// Restore default settings and new settings
		// *****************************************
		public static void RestoreDefaultAndCurrentSettings()
		{
			// Set all the default values for the Plist structure "settingsCurrent"

			// Create a default Dictionary "settingsDefault".
			// Create a current Dictionary "settingsCurrent".
			// Fill up new settings by default settings, where necessary.
			// This assures that every (key, value) pair is contained
			// in the "default" and "current" dictionaries,
			// even if the loaded "current" dictionary did NOT contain every pair.

			SEBSettings.CreateDefaultAndCurrentSettingsFromScratch();
			SEBSettings.settingsCurrent.Clear();
			SEBSettings.FillSettingsDictionary();
			SEBSettings.FillSettingsArrays();
		}



		// ********************
		// Copy settings arrays
		// ********************
		public static void FillSettingsArrays()
		{
			// Set all array values to default values
			for (int value = 1; value <= SEBSettings.ValNum; value++)
			{
				SEBSettings.intArrayCurrent[value] = SEBSettings.intArrayDefault[value];
				SEBSettings.strArrayCurrent[value] = SEBSettings.strArrayDefault[value];
			}
			return;
		}



		// ************************
		// Copy settings dictionary
		// ************************
		/*
				public static void CopySettingsDictionary(ref DictObj sebSettingsSource,
														  ref DictObj sebSettingsTarget)
				{
					// Copy all settings from one dictionary to another
					// Create a dictionary "target settings".
					// Copy source settings to target settings
					foreach (KeyValue pair in sebSettingsSource)
					{
						string key   = pair.Key;
						object value = pair.Value;

						if  (sebSettingsTarget.ContainsKey(key))
							 sebSettingsTarget[key] = value;
						else sebSettingsTarget.Add(key, value);
					}

					return;
				}
		*/


		// **************
		// Merge settings
		// **************
		/*
				public static void MergeSettings(ref object objectSource, ref object objectTarget)
				{
					// Determine the type of the input objects
					string typeSource = objectSource.GetType().ToString();
					string typeTarget = objectTarget.GetType().ToString();

					if (typeSource != typeTarget) return;

					// Treat the complex datatype Dictionary<string, object>
					if (typeSource.Contains("Dictionary"))
					{
						DictObj dictSource = (DictObj)objectSource;
						DictObj dictTarget = (DictObj)objectTarget;

						//foreach (KeyValue pair in dictSource)
						for (int index = 0; index < dictSource.Count; index++)
						{
							KeyValue pair  = dictSource.ElementAt(index);
							string   key   = pair.Key;
							object   value = pair.Value;
							string   type  = pair.Value.GetType().ToString();

							if  (dictTarget.ContainsKey(key))
								 dictTarget[key] = value;
							else dictTarget.Add(key, value);

							if (type.Contains("Dictionary") || type.Contains("List"))
							{
								object childSource = dictSource[key];
								object childTarget = dictTarget[key];
								MergeSettings(ref childSource, ref childTarget);
							}

						} // next (KeyValue pair in dictSource)
					} // end if (typeSource.Contains("Dictionary"))


					// Treat the complex datatype List<object>
					if (typeSource.Contains("List"))
					{
						ListObj listSource = (ListObj)objectSource;
						ListObj listTarget = (ListObj)objectTarget;

						//foreach (object elem in listSource)
						for (int index = 0; index < listSource.Count; index++)
						{
							object elem = listSource[index];
							string type = elem.GetType().ToString();

							if  (listTarget.Count > index)
								 listTarget[index] = elem;
							else listTarget.Add(elem);

							if (type.Contains("Dictionary") || type.Contains("List"))
							{
								object childSource = listSource[index];
								object childTarget = listTarget[index];
								MergeSettings(ref childSource, ref childTarget);
							}

						} // next (element in listSource)
					} // end if (typeSource.Contains("List"))

					return;
				}
		*/


		// ************************
		// Fill settings dictionary
		// ************************
		public static void FillSettingsDictionary()
		{

			// Add potentially missing keys to current Main Dictionary
			foreach (KeyValue p in SEBSettings.settingsDefault)
				if (SEBSettings.settingsCurrent.ContainsKey(p.Key) == false)
				{
					// Key is missing: Add the default value
					SEBSettings.settingsCurrent.Add(p.Key, p.Value);
				}
				else
				{
					// Key exists in new settings: Check if it has the correct object type
					object value = SEBSettings.settingsCurrent[p.Key];
					object defaultValueObject = p.Value;
					if (!value.GetType().Equals(defaultValueObject.GetType()))
					{
						// The object type is not correct: Replace the object with the default value object
						SEBSettings.settingsCurrent[p.Key] = defaultValueObject;
					}
				}

			// Get the Permitted Process List
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];

			// Traverse Permitted Processes of currently opened file
			for (int listIndex = 0; listIndex < SEBSettings.permittedProcessList.Count; listIndex++)
			{
				// Get the Permitted Process Data
				SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[listIndex];

				// Add potentially missing keys to current Process Dictionary
				foreach (KeyValue p in SEBSettings.permittedProcessDataDefault)
					if (SEBSettings.permittedProcessData.ContainsKey(p.Key) == false)
						SEBSettings.permittedProcessData.Add(p.Key, p.Value);

				// Get the Permitted Argument List
				SEBSettings.permittedArgumentList = (ListObj) SEBSettings.permittedProcessData[SEBSettings.KeyArguments];

				// Traverse Arguments of current Process
				for (int sublistIndex = 0; sublistIndex < SEBSettings.permittedArgumentList.Count; sublistIndex++)
				{
					// Get the Permitted Argument Data
					SEBSettings.permittedArgumentData = (DictObj) SEBSettings.permittedArgumentList[sublistIndex];

					// Add potentially missing keys to current Argument Dictionary
					foreach (KeyValue p in SEBSettings.permittedArgumentDataDefault)
						if (SEBSettings.permittedArgumentData.ContainsKey(p.Key) == false && p.Value as string != "")
							SEBSettings.permittedArgumentData.Add(p.Key, p.Value);

				} // next sublistIndex
			} // next listIndex



			// Get the Prohibited Process List
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];

			// Traverse Prohibited Processes of currently opened file
			for (int listIndex = 0; listIndex < SEBSettings.prohibitedProcessList.Count; listIndex++)
			{
				// Get the Prohibited Process Data
				SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[listIndex];

				// Add potentially missing keys to current Process Dictionary
				foreach (KeyValue p in SEBSettings.prohibitedProcessDataDefault)
					if (SEBSettings.prohibitedProcessData.ContainsKey(p.Key) == false)
						SEBSettings.prohibitedProcessData.Add(p.Key, p.Value);

			} // next listIndex



			// Get the Embedded Certificate List
			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];

			// Traverse Embedded Certificates of currently opened file
			for (int listIndex = 0; listIndex < SEBSettings.embeddedCertificateList.Count; listIndex++)
			{
				// Get the Embedded Certificate Data
				SEBSettings.embeddedCertificateData = (DictObj) SEBSettings.embeddedCertificateList[listIndex];

				// Add potentially missing keys to current Certificate Dictionary
				foreach (KeyValue p in SEBSettings.embeddedCertificateDataDefault)
					if (SEBSettings.embeddedCertificateData.ContainsKey(p.Key) == false)
						SEBSettings.embeddedCertificateData.Add(p.Key, p.Value);

			} // next listIndex



			//// Get the URL Filter Rule List
			//SEBSettings.urlFilterRuleList = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterRules];

			//// Traverse URL Filter Rules of currently opened file
			//for (int listIndex = 0; listIndex < SEBSettings.urlFilterRuleList.Count; listIndex++)
			//{
			//    // Get the URL Filter Rule Data
			//    SEBSettings.urlFilterRuleData = (DictObj)SEBSettings.urlFilterRuleList[listIndex];

			//    // Add potentially missing keys to current Rule Dictionary
			//    foreach (KeyValue p in SEBSettings.urlFilterRuleDataDefault)
			//                       if (SEBSettings.urlFilterRuleData.ContainsKey(p.Key) == false)
			//                           SEBSettings.urlFilterRuleData.Add        (p.Key, p.Value);

			//    // Get the URL Filter Action List
			//    SEBSettings.urlFilterActionList = (ListObj)SEBSettings.urlFilterRuleData[SEBSettings.KeyRuleActions];

			//    // Traverse Actions of current Rule
			//    for (int sublistIndex = 0; sublistIndex < SEBSettings.urlFilterActionList.Count; sublistIndex++)
			//    {
			//        // Get the URL Filter Action Data
			//        SEBSettings.urlFilterActionData = (DictObj)SEBSettings.urlFilterActionList[sublistIndex];

			//        // Add potentially missing keys to current Action Dictionary
			//        foreach (KeyValue p in SEBSettings.urlFilterActionDataDefault)
			//                           if (SEBSettings.urlFilterActionData.ContainsKey(p.Key) == false)
			//                               SEBSettings.urlFilterActionData.Add        (p.Key, p.Value);

			//    } // next sublistIndex
			//} // next listIndex



			// Get the Proxies Dictionary
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];

			// Add potentially missing keys to current Proxies Dictionary
			foreach (KeyValue p in SEBSettings.proxiesDataDefault)
				if (SEBSettings.proxiesData.ContainsKey(p.Key) == false)
					SEBSettings.proxiesData.Add(p.Key, p.Value);

			// Get the Bypassed Proxy List
			SEBSettings.bypassedProxyList = (ListObj) proxiesData[SEBSettings.KeyExceptionsList];

			// Traverse Bypassed Proxies of currently opened file
			for (int listIndex = 0; listIndex < SEBSettings.bypassedProxyList.Count; listIndex++)
			{
				if ((String) SEBSettings.bypassedProxyList[listIndex] == "")
					SEBSettings.bypassedProxyList[listIndex] = bypassedProxyDataDefault;
			} // next listIndex


			return;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Return a settings dictionary with removed empty ListObj and DictObj elements 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static DictObj CleanSettingsDictionary()
		{
			DictObj cleanSettings = new DictObj();

			// Add key/values to the clear dictionary if they're not an empty array (ListObj) or empty dictionary (DictObj)
			foreach (KeyValue p in SEBSettings.settingsDefault)
				if (!(p.Value is ListObj && ((ListObj) p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj) p.Value).Count == 0))
					cleanSettings.Add(p.Key, p.Value);


			// Get the Permitted Process List
			ListObj permittedProcessList = (ListObj) valueForDictionaryKey(cleanSettings, SEBSettings.KeyPermittedProcesses);
			if (permittedProcessList != null)
			{
				// Traverse Permitted Processes of currently opened file
				for (int listIndex = 0; listIndex < permittedProcessList.Count; listIndex++)
				{
					// Get the Permitted Process Data
					DictObj permittedProcessData = (DictObj) permittedProcessList[listIndex];
					if (permittedProcessData != null)
					{
						// Add potentially missing keys to current Process Dictionary
						foreach (KeyValue p in permittedProcessDataDefault)
							if (permittedProcessData.ContainsKey(p.Key) == false && !(p.Value is ListObj && ((ListObj) p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj) p.Value).Count == 0))
								permittedProcessData.Add(p.Key, p.Value);

						// Get the Permitted Argument List
						ListObj permittedArgumentList = (ListObj) valueForDictionaryKey(permittedProcessData, SEBSettings.KeyArguments);
						if (permittedArgumentList != null)
						{
							// Traverse Arguments of current Process
							for (int sublistIndex = 0; sublistIndex < permittedArgumentList.Count; sublistIndex++)
							{
								// Get the Permitted Argument Data
								DictObj permittedArgumentData = (DictObj) permittedArgumentList[sublistIndex];

								// Add potentially missing keys to current Argument Dictionary
								foreach (KeyValue p in permittedArgumentDataDefault)
									if (permittedArgumentData.ContainsKey(p.Key) == false && p.Value as string != "")
										permittedArgumentData.Add(p.Key, p.Value);

							} // next sublistIndex
						}
					}
				} // next listIndex
			}

			// Get the Prohibited Process List
			ListObj prohibitedProcessList = (ListObj) valueForDictionaryKey(cleanSettings, SEBSettings.KeyProhibitedProcesses);
			if (prohibitedProcessList != null)
			{
				// Traverse Prohibited Processes of currently opened file
				for (int listIndex = 0; listIndex < prohibitedProcessList.Count; listIndex++)
				{
					// Get the Prohibited Process Data
					DictObj prohibitedProcessData = (DictObj) prohibitedProcessList[listIndex];

					// Add potentially missing keys to current Process Dictionary
					foreach (KeyValue p in prohibitedProcessDataDefault)
						if (!(p.Value is ListObj && ((ListObj) p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj) p.Value).Count == 0))
							prohibitedProcessData.Add(p.Key, p.Value);

				} // next listIndex
			}

			// Get the Embedded Certificate List
			ListObj embeddedCertificateList = (ListObj) valueForDictionaryKey(cleanSettings, SEBSettings.KeyEmbeddedCertificates);
			if (embeddedCertificateList != null)
			{
				// Traverse Embedded Certificates of currently opened file
				for (int listIndex = 0; listIndex < embeddedCertificateList.Count; listIndex++)
				{
					// Get the Embedded Certificate Data
					DictObj embeddedCertificateData = (DictObj) embeddedCertificateList[listIndex];

					// Add potentially missing keys to current Certificate Dictionary
					foreach (KeyValue p in embeddedCertificateDataDefault)
						if (!(p.Value is ListObj && ((ListObj) p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj) p.Value).Count == 0))
							embeddedCertificateData.Add(p.Key, p.Value);

				} // next listIndex
			}

			//// Get the URL Filter Rule List
			//ListObj urlFilterRuleList = (ListObj)valueForDictionaryKey(cleanSettings, SEBSettings.KeyURLFilterRules);
			//if (urlFilterRuleList != null)
			//{
			//    // Traverse URL Filter Rules of currently opened file
			//    for (int listIndex = 0; listIndex < urlFilterRuleList.Count; listIndex++)
			//    {
			//        // Get the URL Filter Rule Data
			//        DictObj urlFilterRuleData = (DictObj)urlFilterRuleList[listIndex];

			//        // Add potentially missing keys to current Rule Dictionary
			//        foreach (KeyValue p in urlFilterRuleDataDefault)
			//            if (!(p.Value is ListObj && ((ListObj)p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj)p.Value).Count == 0))
			//                urlFilterRuleData.Add(p.Key, p.Value);

			//        // Get the URL Filter Action List
			//        ListObj urlFilterActionList = (ListObj)valueForDictionaryKey(urlFilterRuleData, SEBSettings.KeyRuleActions);
			//        if (urlFilterActionList != null)
			//        {
			//            // Traverse Actions of current Rule
			//            for (int sublistIndex = 0; sublistIndex < urlFilterActionList.Count; sublistIndex++)
			//            {
			//                // Get the URL Filter Action Data
			//                DictObj urlFilterActionData = (DictObj)urlFilterActionList[sublistIndex];

			//                // Add potentially missing keys to current Action Dictionary
			//                foreach (KeyValue p in urlFilterActionDataDefault)
			//                    if (!(p.Value is ListObj && ((ListObj)p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj)p.Value).Count == 0))
			//                        urlFilterActionData.Add(p.Key, p.Value);

			//            } // next sublistIndex
			//        }
			//    } // next listIndex
			//}

			// Get the Proxies Dictionary
			DictObj proxiesData = (DictObj) valueForDictionaryKey(cleanSettings, SEBSettings.KeyProxies);
			if (proxiesData != null)
			{
				// Add potentially missing keys to current Proxies Dictionary
				foreach (KeyValue p in proxiesDataDefault)
					if (proxiesData.ContainsKey(p.Key) == false && !(p.Value is ListObj && ((ListObj) p.Value).Count == 0) && !(p.Value is DictObj && ((DictObj) p.Value).Count == 0))
						proxiesData.Add(p.Key, p.Value);

				// Get the Bypassed Proxy List
				ListObj bypassedProxyList = (ListObj) valueForDictionaryKey(proxiesData, SEBSettings.KeyExceptionsList);
				if (bypassedProxyList != null)
				{
					if (bypassedProxyList.Count == 0)
					{
						//proxiesData.Remove(SEBSettings.KeyExceptionsList);
					}
					else
					{
						// Traverse Bypassed Proxies of currently opened file
						for (int listIndex = 0; listIndex < bypassedProxyList.Count; listIndex++)
						{
							if ((String) bypassedProxyList[listIndex] == "")
								bypassedProxyList[listIndex] = bypassedProxyDataDefault;
						} // next listIndex
					}
				}
			}

			return cleanSettings;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Read the value for a key from a dictionary and 
		/// return null for the value if the key doesn't exist 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static object valueForDictionaryKey(DictObj dictionary, string key)
		{
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}
			else
			{
				return null;
			}
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Clone a dictionary 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(Dictionary<TKey, TValue> original) where TValue : ICloneable
		{
			Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
			foreach (KeyValuePair<TKey, TValue> entry in original)
			{
				ret.Add(entry.Key, (TValue) entry.Value.Clone());
			}
			return ret;
		}

		public static void AddDefaultProhibitedProcesses()
		{
			// Get the Prohibited Process list
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];

			// Insert strictly prohibited processes unconditionally
			InsertProhibitedProcessesFromArray(prohibitedProcessesDefaultStrict);

			// Insert default prohibited processes only in Disable Explorer Shell kiosk mode
			if ((bool) SEBSettings.settingsCurrent[SEBSettings.KeyKillExplorerShell] == true)
			{
				InsertProhibitedProcessesFromArray(prohibitedProcessesDefault);
			}
		}

		private static void InsertProhibitedProcessesFromArray(List<string> newProhibitedProcesses)
		{
			foreach (string defaultProhibitedProcessName in newProhibitedProcesses)
			{
				// Position of this default prohibited process in Prohibited Process list
				int indexOfProcess = -1;

				string prohibitedProcessFilenameWithoutExtension = Path.GetFileNameWithoutExtension(defaultProhibitedProcessName);

				// Traverse Prohibited Processes of currently opened file
				for (int listIndex = 0; listIndex < SEBSettings.prohibitedProcessList.Count; listIndex++)
				{
					DictObj prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[listIndex];

					if ((int) prohibitedProcessData[SEBSettings.KeyOS] == IntWin)
					{
						// Check if this prohibited process already is in Prohibited Process list in current settings
						if (Path.GetFileNameWithoutExtension((string) prohibitedProcessData[SEBSettings.KeyOriginalName]).Equals(prohibitedProcessFilenameWithoutExtension, StringComparison.InvariantCultureIgnoreCase) ||
							Path.GetFileNameWithoutExtension((string) prohibitedProcessData[SEBSettings.KeyExecutable]).Equals(prohibitedProcessFilenameWithoutExtension, StringComparison.InvariantCultureIgnoreCase))
							indexOfProcess = listIndex;
					}
				} // next listIndex

				// If this default prohibited process was not in Prohibited Process list, insert it at the beginning
				if (indexOfProcess == -1)
				{
					SEBSettings.prohibitedProcessList.Insert(0, prohibitetProcessDictForProcess(defaultProhibitedProcessName));
				}
			}
		}

		private static DictObj prohibitetProcessDictForProcess(string processName)
		{
			DictObj prohibitedProcessDict = new DictObj();

			prohibitedProcessDict.Add(SEBSettings.KeyActive, true);
			prohibitedProcessDict.Add(SEBSettings.KeyCurrentUser, true);
			prohibitedProcessDict.Add(SEBSettings.KeyStrongKill, false);
			prohibitedProcessDict.Add(SEBSettings.KeyOS, IntWin);
			prohibitedProcessDict.Add(SEBSettings.KeyExecutable, processName);
			prohibitedProcessDict.Add(SEBSettings.KeyOriginalName, processName);
			prohibitedProcessDict.Add(SEBSettings.KeyDescription, "");
			prohibitedProcessDict.Add(SEBSettings.KeyIdentifier, "");
			prohibitedProcessDict.Add(SEBSettings.KeyWindowHandlingProcess, "");
			prohibitedProcessDict.Add(SEBSettings.KeyUser, "");

			return prohibitedProcessDict;
		}

		public static bool CheckForDefaultProhibitedProcesses(bool remove)
		{
			// Get the Prohibited Process list
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			bool prohibitedProcessFound = false;

			foreach (string defaultProhibitedProcessName in prohibitedProcessesDefault)
			{
				string prohibitedProcessFilenameWithoutExtension = Path.GetFileNameWithoutExtension(defaultProhibitedProcessName);
				int listIndex = 0;
				// Traverse Prohibited Processes of currently opened file
				while (listIndex < SEBSettings.prohibitedProcessList.Count)
				{
					DictObj prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[listIndex];

					// Check if this prohibited process already is in Prohibited Process list in current settings
					if (Path.GetFileNameWithoutExtension((string) prohibitedProcessData[SEBSettings.KeyOriginalName]).Equals(prohibitedProcessFilenameWithoutExtension, StringComparison.InvariantCultureIgnoreCase) ||
						Path.GetFileNameWithoutExtension((string) prohibitedProcessData[SEBSettings.KeyExecutable]).Equals(prohibitedProcessFilenameWithoutExtension, StringComparison.InvariantCultureIgnoreCase))
					{
						prohibitedProcessFound = true;
						if (remove)
						{
							SEBSettings.prohibitedProcessList.RemoveAt(listIndex);
						}
						else
						{
							break;
						}
					}
					else
					{
						listIndex++;
					}
				} // next listIndex
				if (prohibitedProcessFound && !remove)
				{
					break;
				}
			}

			return prohibitedProcessFound;
		}

		// **************
		// Print settings
		// **************
		public static void PrintSettingsRecursively(object objectSource, StreamWriter fileWriter, String indenting)
		{

			// Determine the type of the input object
			string typeSource = objectSource.GetType().ToString();


			// Treat the complex datatype Dictionary<string, object>
			if (typeSource.Contains("Dictionary"))
			{
				DictObj dictSource = (DictObj) objectSource;

				//foreach (KeyValue pair in dictSource)
				for (int index = 0; index < dictSource.Count; index++)
				{
					KeyValue pair = dictSource.ElementAt(index);
					string key = pair.Key;
					object value = pair.Value;
					string type = pair.Value.GetType().ToString();

					// Print one (key, value) pair of dictionary
					fileWriter.WriteLine(indenting + key + "=" + value);

					if (type.Contains("Dictionary") || type.Contains("List"))
					{
						object childSource = dictSource[key];
						PrintSettingsRecursively(childSource, fileWriter, indenting + "   ");
					}

				} // next (KeyValue pair in dictSource)
			} // end if (typeSource.Contains("Dictionary"))


			// Treat the complex datatype List<object>
			if (typeSource.Contains("List"))
			{
				ListObj listSource = (ListObj) objectSource;

				//foreach (object elem in listSource)
				for (int index = 0; index < listSource.Count; index++)
				{
					object elem = listSource[index];
					string type = elem.GetType().ToString();

					// Print one element of list
					fileWriter.WriteLine(indenting + elem);

					if (type.Contains("Dictionary") || type.Contains("List"))
					{
						object childSource = listSource[index];
						PrintSettingsRecursively(childSource, fileWriter, indenting + "   ");
					}

				} // next (element in listSource)
			} // end if (typeSource.Contains("List"))

			return;
		}



		// *************************
		// Print settings dictionary
		// *************************
		public static void LoggSettingsDictionary(ref DictObj sebSettings, String fileName)
		{
			FileStream fileStream;
			StreamWriter fileWriter;

			// If the .ini file already exists, delete it
			// and write it again from scratch with new data
			if (File.Exists(fileName))
				File.Delete(fileName);

			// Open the file for writing
			fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
			fileWriter = new StreamWriter(fileStream);

			// Write the header lines
			fileWriter.WriteLine("");
			fileWriter.WriteLine("number of (key, value) pairs = " + sebSettings.Count);
			fileWriter.WriteLine("");

			// Call the recursive method for printing the contents
			SEBSettings.PrintSettingsRecursively(sebSettings, fileWriter, "");

			// Close the file
			fileWriter.Close();
			fileStream.Close();
			return;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Decrypt, deserialize and store new settings as current SEB settings 
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static bool StoreDecryptedSebClientSettings(byte[] sebSettings)
		{
			DictObj settingsDict = null;
			// If we were passed empty settings, we skip decrypting and just use default settings
			if (sebSettings != null)
			{
				string filePassword = null;
				bool passwordIsHash = false;
				X509Certificate2 fileCertificateRef = null;

				try
				{
					// Decrypt the configuration settings.
					// Convert the XML structure into a C# dictionary object.

					settingsDict = SEBConfigFileManager.DecryptSEBSettings(sebSettings, false, ref filePassword, ref passwordIsHash, ref fileCertificateRef);
					if (settingsDict == null)
					{
						Logger.AddError("The .seb file could not be decrypted. ", null, null, "");
						return false;
					}
				}
				catch (Exception streamReadException)
				{
					// Let the user know what went wrong
					Logger.AddError("The .seb file could not be decrypted. ", null, streamReadException, streamReadException.Message);
					return false;
				}
			}
			// Store the new settings or use defaults if new settings were empty
			StoreSebClientSettings(settingsDict);

			return true;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Store passed new settings as current SEB settings 
		/// or use default settings if none were passed.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public static void StoreSebClientSettings(DictObj settingsDict)
		{
			// Recreate the default and current settings dictionaries
			SEBSettings.CreateDefaultAndCurrentSettingsFromScratch();
			SEBSettings.settingsCurrent.Clear();

			// If we got new settings, we use them (otherwise use defaults)
			if (settingsDict != null) SEBSettings.settingsCurrent = settingsDict;

			// Fill up the Dictionary read from file with default settings, where necessary
			SEBSettings.FillSettingsDictionary();
			SEBSettings.FillSettingsArrays();

			// Add the XulRunner process to the Permitted Process List, if necessary
			SEBSettings.AddDefaultProhibitedProcesses();
		}


		// *********************************************
		// Read the settings from the configuration file
		// *********************************************
		public static bool ReadSebConfigurationFile(String fileName, bool forEditing, ref string filePassword, ref bool passwordIsHash, ref X509Certificate2 fileCertificateRef)
		{
			DictObj newSettings = new DictObj();
			try
			{
				// Read the configuration settings from .seb file.
				// Decrypt the configuration settings.
				// Convert the XML structure into a C# object.

				byte[] encryptedSettings = File.ReadAllBytes(fileName);

				newSettings = SEBConfigFileManager.DecryptSEBSettings(encryptedSettings, forEditing, ref filePassword, ref passwordIsHash, ref fileCertificateRef);
				if (newSettings == null) return false;
			}
			catch (Exception streamReadException)
			{
				// Let the user know what went wrong
				Logger.AddError("The .seb file could not be read: ", null, streamReadException, streamReadException.Message);
				return false;
			}

			// If the settings could be read from file...
			// Recreate the default and current settings
			SEBSettings.CreateDefaultAndCurrentSettingsFromScratch();
			SEBSettings.settingsCurrent.Clear();
			SEBSettings.settingsCurrent = newSettings;

			// Fill up the Dictionary read from file with default settings, where necessary
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsDefault, "DebugSettingsDefaultInReadSebConfigurationFileFillBefore.txt");
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsCurrent, "DebugSettingsCurrentInReadSebConfigurationFileFillBefore.txt");
			SEBSettings.FillSettingsDictionary();
			SEBSettings.FillSettingsArrays();
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsDefault, "DebugSettingsDefaultInReadSebConfigurationFileFillAfter.txt");
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsCurrent, "DebugSettingsCurrentInReadSebConfigurationFileFillAfter.txt");

			// Add the XulRunner process to the Permitted Process List, if necessary
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsDefault, "DebugSettingsDefaultInReadSebConfigurationFilePermitBefore.txt");
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsCurrent, "DebugSettingsCurrentInReadSebConfigurationFilePermitBefore.txt");
			SEBSettings.AddDefaultProhibitedProcesses();
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsDefault, "DebugSettingsDefaultInReadSebConfigurationFilePermitAfter.txt");
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsCurrent, "DebugSettingsCurrentInReadSebConfigurationFilePermitAfter.txt");

			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsDefault, "DebugSettingsDefaultInReadSebConfigurationFile.txt");
			//SEBSettings.LoggSettingsDictionary(ref SEBSettings.settingsCurrent, "DebugSettingsCurrentInReadSebConfigurationFile.txt");

			return true;
		}



		// ********************************************************
		// Write the settings to the configuration file and save it
		// ********************************************************
		public static bool WriteSebConfigurationFile(String fileName, string filePassword, bool passwordIsHash, X509Certificate2 fileCertificateRef, bool useAsymmetricOnlyEncryption, SEBSettings.sebConfigPurposes configPurpose, bool forEditing = false)
		{
			try
			{
				// Convert the C# settings dictionary object into an XML structure.
				// Encrypt the configuration settings depending on passed credentials
				// Write the configuration settings into .seb file.

				byte[] encryptedSettings = SEBConfigFileManager.EncryptSEBSettingsWithCredentials(filePassword, passwordIsHash, fileCertificateRef, useAsymmetricOnlyEncryption, configPurpose, forEditing);
				File.WriteAllBytes(fileName, encryptedSettings);
			}
			catch (Exception streamWriteException)
			{
				// Let the user know what went wrong
				Logger.AddError("The configuration file could not be written: ", null, streamWriteException, streamWriteException.Message);
				return false;
			}
			return true;
		}
	}
}
