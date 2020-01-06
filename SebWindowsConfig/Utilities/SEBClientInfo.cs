using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
//
//  SEBClientInfo.cs
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

namespace SebWindowsConfig.Utilities
{
	public enum chooseFileToUploadPolicies
    {
        manuallyWithFileRequester               = 0,
        attemptUploadSameFileDownloadedBefore   = 1,
        onlyAllowUploadSameFileDownloadedBefore = 2
    };

    public enum newBrowserWindowPolicies
    {
        getGenerallyBlocked = 0,
        openInSameWindow    = 1,
        openInNewWindow     = 2
    };

    public enum sebServicePolicies
    {
        ignoreService          = 0,
        indicateMissingService = 1,
        forceSebService        = 2
    };

    public enum browserViewModes
    {
        browserViewModeWindow     = 0,
        browserViewModeFullscreen = 1
    };

    // MAC
    public enum sebPurposePolicies
    {
        sebPurposePolicyStartingExam      = 0,
        sebPurposePolicyConfiguringClient = 1
    };

    public enum URLFilterRuleActions
    {
        block = 0,
        allow = 1,
        ignore = 2,
        unknown = 3
    };

    public enum SEBMinMacOSVersion
    {
        SEBMinOSX10_7 = 0,
        SEBMinOSX10_8 = 1,
        SEBMinOSX10_9 = 2,
        SEBMinOSX10_10 = 3,
        SEBMinOSX10_11 = 4,
        SEBMinMacOS10_12 = 5,
        SEBMinMacOS10_13 = 6,
        SEBMinMacOS10_14 = 7
    };

    public class SEBClientInfo
    {
		#region Imports
        [DllImport("kernel32.Dll")]
        public static extern short GetVersionEx(ref OSVERSIONINFO o);
		#endregion

		// Socket protocol
		//static int ai_family   = AF_INET;
		//static int ai_socktype = SOCK_STREAM;
		//static int ai_protocol = IPPROTO_TCP;

		#region Constants

		// Name and location of SEB configuration files and logfiles
		public const string SEB_CLIENT_CONFIG = "SebClientSettings.seb";
		public const string SEB_CLIENT_LOG = "SebClient.log";
		private const string XUL_RUNNER_CONFIG = "config.json";
		public const string SEB_SHORTNAME = "SEB";
		public const string XUL_RUNNER = "firefox.exe";
		private const string XUL_RUNNER_INI = "seb.ini";

		// Application path contains [MANUFACTURER]\[PRODUCT_NAME]
		// (see also "SebWindowsPackageSetup" Project in MS Visual Studio 10)
		public const string MANUFACTURER_LOCAL     = "SafeExamBrowser";
        //private const string MANUFACTURER         = "ETH Zuerich";
        public const string PRODUCT_NAME           = "SafeExamBrowser";
        public const string SEB_SERVICE_DIRECTORY = "SebWindowsServiceWCF";
        public const string SEB_BROWSER_DIRECTORY = "SebWindowsBrowser";
        private const string XUL_RUNNER_DIRECTORY = "xulrunner";
        private const string XUL_SEB_DIRECTORY = "xul_seb";
        public const string FILENAME_SEB = "SafeExamBrowser.exe";
        public const string FILENAME_SEBCONFIGTOOL = "SEBConfigTool.exe";
        public const string FILENAME_SEBSERVICE = "SebWindowsServiceWCF.exe";
        public const string FILENAME_DLL_FLECK = "Fleck.dll";
        public const string FILENAME_DLL_ICONLIB = "IconLib.dll";
        public const string FILENAME_DLL_IONICZIP = "Ionic.Zip.dll";
        public const string FILENAME_DLL_METRO = "MetroFramework.dll";
        public const string FILENAME_DLL_NAUDIO = "NAudio.dll";
        public const string FILENAME_DLL_NEWTONSOFTJSON = "Newtonsoft.Json.dll";
        public const string FILENAME_DLL_SERVICECONTRACTS = "SEBWindowsServiceContracts.dll";
        public const string BROWSER_USERAGENT_DESKTOP = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0";
        public const string BROWSER_USERAGENT_TOUCH = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:52.0; Touch) Gecko/20100101 Firefox/52.0";
        public const string BROWSER_USERAGENT_TOUCH_IPAD = "Mozilla/5.0 (iPad; CPU OS 11_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.3 Mobile/15E216 Safari/605.1.15";
        public const string BROWSER_USERAGENT_SEB = "SEB";

        public  const string END_OF_STRING_KEYWORD   = "---SEB---";
        private const string DEFAULT_USERNAME        = "";
        private const string DEFAULT_HOSTNAME        = "localhost";
        private const string DEFAULT_HOST_IP_ADDRESS = "127.0.0.1";
        private const int    DEFAULT_PORTNUMBER      = 57016;
        public const string  DEFAULT_KEY             = "Di𝈭l𝈖Ch𝈒ah𝉇t𝈁a𝉈Hai1972";
        private const int    DEFAULT_SEND_INTERVAL = 100;
        private const int    DEFAULT_RECV_TIMEOUT    = 100;
        private const int    DEFAULT_NUM_MESSAGES    = 3;

        public const string SEB_NEW_DESKTOP_NAME     = "SEBDesktop";
        public const string SEB_WINDOWS_SERVICE_NAME = "SebWindowsService";

        #endregion

        #region Public Properties

        public static bool ExplorerShellWasKilled { get; set; }
        public static bool IsNewOS { get; set; }
        public static bool examMode = false;

        // SEB Client Socket properties
        public static char[] UserNameRegistryFlags { get; set; }
        public static char[] RegistryFlags         { get; set; }
        public static string HostName              { get; set; }
        public static string HostIpAddress         { get; set; }
        public static string UserName  { get; set; }
        public static char[] UserSid   { get; set; }
        public static int PortNumber   { get; set; }
        public static int SendInterval { get; set; }
        public static int RecvTimeout  { get; set; }
        public static int NumMessages  { get; set; }
        public static int MessageNr    { get; set; }
        public static string DesktopName { get; set; }

       // SEB Client Directories properties
        public static string ApplicationExecutableDirectory { get; set; }
        public static string ProgramFilesX86Directory       { get; set; }
        public static bool   LogFileDesiredMsgHook          { get; set; }
        public static bool   LogFileDesiredSebClient        { get; set; }
        public static string SebClientLogFileDirectory      { get; set; }
        public static string SebClientDirectory             { get; set; }
        public static string SebClientLogFile               { get; set; }
        public static string SebClientSettingsProgramDataDirectory { get; set; }
        public static string SebClientSettingsAppDataDirectory   { get; set; }
		public static string XulRunnerAdditionalDictionariesDirectory { get; set; }
        public static string XulRunnerDirectory { get; set; }
        public static string XulSebDirectory    { get; set; }
        public static string SebClientSettingsProgramDataFile;
        public static string SebClientSettingsAppDataFile; 
        public static string XulRunnerConfigFileDirectory { get; set; }
        public static string XulRunnerConfigFile;
        public static string XulRunnerExePath;
        public static string XulRunnerSebIniPath;
        public static string XulRunnerParameter;
        //public static string XulRunnerFlashContainerState { get; set; }

        public static string ExamUrl { get; set; }
        public static string QuitPassword { get; set; }
        public static string QuitHashcode { get; set; }

        //public static Dictionary<string, object> sebSettings = new Dictionary<string, object>();

        public static string LoadingSettingsFileName = "";

        public static float scaleFactor = 1;
        public static int appChooserHeight = 132;

        #endregion

		#region Structures
       /// <summary>
        /// Stores windows version info.
        /// </summary>
         [StructLayout(LayoutKind.Sequential)]
        public struct OSVERSIONINFO
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
        }
        #endregion

         //public static SEBClientConfig sebClientConfig;

         public static Dictionary<string, object> getSebSetting(string key)
         {
             object sebSetting = null;
             try
             {
                 sebSetting = SEBSettings.settingsCurrent[key];
              } 
             catch 
             {
                 sebSetting = null;
             }

             if (sebSetting != null)
                 return SEBSettings.settingsCurrent;
             else
                 return SEBSettings.settingsDefault;
         }

        /// <summary>
         /// Sets user, host info, send-recv interval, recv timeout, Logger and read SebClient configuration.
        /// </summary>
        /// <returns></returns>
        public static bool SetSebClientConfiguration()
        {

            bool setSebClientConfiguration = false;

            // Initialise socket properties
            IsNewOS                = false;
            ExplorerShellWasKilled = false;
            UserNameRegistryFlags  = new char[100];
            RegistryFlags          = new char[50];
            UserSid                = new char[512];
            UserName               = DEFAULT_USERNAME;
            HostName               = DEFAULT_HOSTNAME;
            HostIpAddress          = DEFAULT_HOST_IP_ADDRESS;
            PortNumber             = DEFAULT_PORTNUMBER;
            SendInterval           = DEFAULT_SEND_INTERVAL;
            RecvTimeout            = DEFAULT_RECV_TIMEOUT;
            NumMessages            = DEFAULT_NUM_MESSAGES;

            //Sets paths to files SEB has to save or read from the file system
            SetSebPaths();

            byte[] sebClientSettings = null;

            // Create a string builder for a temporary log (until we can write it with the Logger)
            StringBuilder tempLogStringBuilder = new StringBuilder();

            // Try to read the SebClientSettigs.seb file from the program data directory
            try
            {
                sebClientSettings = File.ReadAllBytes(SebClientSettingsProgramDataFile);
            }
            catch (Exception streamReadException)
            {
                // Write error into string with temporary log string builder
                tempLogStringBuilder.Append("Could not load SebClientSettigs.seb from the Program Data directory").Append(streamReadException == null ? null : streamReadException.GetType().ToString()).Append(streamReadException.Message);
            }
            if (sebClientSettings == null)
            {
                // Try to read the SebClientSettigs.seb file from the local application data directory
                try
                {
                    sebClientSettings = File.ReadAllBytes(SebClientSettingsAppDataFile);
                }
                catch (Exception streamReadException)
                {
                    // Write error into string with temporary log string builder
                    tempLogStringBuilder.Append("Could not load SebClientSettigs.seb from the Roaming Application Data directory. ").Append(streamReadException == null ? null : streamReadException.GetType().ToString()).Append(streamReadException.Message);
                }
            }

            // Store the decrypted configuration settings.
            if (!SEBSettings.StoreDecryptedSebClientSettings(sebClientSettings))
                return false;

            // Initialise Logger, if enabled
            InitializeLogger();

            // Save the temporary log string into the log
            Logger.AddError(tempLogStringBuilder.ToString(), null, null);

            // Set username
            UserName = Environment.UserName;

            setSebClientConfiguration = true;
            
            // Write settings into log
            StringBuilder userInfo =
                new StringBuilder ("User Name: "                   ).Append(UserName)
                          .Append(" Host Name: "                   ).Append(HostName)                         
                          .Append(" Port Number: "                 ).Append(PortNumber)
                          .Append(" Send Interval: "               ).Append(SendInterval)
                          .Append(" Recv Timeout: "                ).Append(RecvTimeout)
                          .Append(" Num Messages: "                ).Append(NumMessages)
                          .Append(" SebClientConfigFileDirectory: ").Append(SebClientSettingsAppDataDirectory)
                          .Append(" SebClientConfigFile: "         ).Append(SebClientSettingsAppDataFile);
            Logger.AddInformation(userInfo.ToString(), null, null);

            return setSebClientConfiguration;
        }

        /// <summary>
        /// Initialise Logger if it's enabled.
        /// </summary>
        public static void InitializeLogger()
        {
            if ((Boolean)getSebSetting(SEBSettings.KeyEnableLogging)[SEBSettings.KeyEnableLogging])
            {
                string logDirectory = (string)SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, SEBSettings.KeyLogDirectoryWin);
                if (!String.IsNullOrEmpty(logDirectory))
                {
                    // Expand environment variables in log file path
                    SebClientLogFileDirectory = Environment.ExpandEnvironmentVariables(logDirectory);

                    SebClientLogFile = String.Format(@"{0}\{1}", SebClientLogFileDirectory, SEB_CLIENT_LOG);
                }
                else
                {
                    SEBClientInfo.SetDefaultClientLogFile();
                }
                Logger.InitLogger(SEBClientInfo.SebClientLogFileDirectory, null);
            }
        }

        /// <summary>
        /// Sets paths to files SEB has to save or read from the file system.
        /// </summary>
        public static void SetSebPaths()
        {
            // Get the path of the directory the application executable lies in
            ApplicationExecutableDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            // Get the path of the "Program Files X86" directory.
            ProgramFilesX86Directory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // Get the path of the "Program Data" and "Local Application Data" directory.
            string programDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); //GetEnvironmentVariable("PROGRAMMDATA");
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            /// Get paths for the two possible locations of the SebClientSettings.seb file
            /// 
            // In the program data directory (for managed systems, only an administrator can write in this directory):
            // If there is a SebClientSettigs.seb file, then this has priority and is used by the SEB client, another
            // SebClientSettigs.seb file in the local app data folder is ignored then and the SEB client cannot be 
            // reconfigured by opening a .seb file saved for configuring a client
            StringBuilder sebClientSettingsProgramDataDirectoryBuilder = new StringBuilder(programDataDirectory).Append("\\").Append(MANUFACTURER_LOCAL).Append("\\"); //.Append(PRODUCT_NAME).Append("\\");
            SebClientSettingsProgramDataDirectory = sebClientSettingsProgramDataDirectoryBuilder.ToString();

            // In the local application data directory (for unmanaged systems like student computers, user can write in this directory):
            // A SebClientSettigs.seb file in this directory can be created or replaced by opening a .seb file saved for configuring a client
            StringBuilder sebClientSettingsAppDataDirectoryBuilder = new StringBuilder(appDataDirectory).Append("\\").Append(MANUFACTURER_LOCAL).Append("\\"); //.Append(PRODUCT_NAME).Append("\\");
            SebClientSettingsAppDataDirectory = sebClientSettingsAppDataDirectoryBuilder.ToString();

            // Set the location of the SebWindowsClientDirectory
            StringBuilder sebClientDirectoryBuilder = new StringBuilder(ProgramFilesX86Directory).Append("\\").Append(PRODUCT_NAME).Append("\\");
            SebClientDirectory = sebClientDirectoryBuilder.ToString();

			// The directory into which additional dictionaries are extracted.
			XulRunnerAdditionalDictionariesDirectory = Path.Combine(SebClientSettingsAppDataDirectory, "Dictionaries");

			// Set the location of the XulRunnerDirectory
			//StringBuilder xulRunnerDirectoryBuilder = new StringBuilder(SebClientDirectory).Append(XUL_RUNNER_DIRECTORY).Append("\\");
			//XulRunnerDirectory = xulRunnerDirectoryBuilder.ToString();
			StringBuilder xulRunnerDirectoryBuilder = new StringBuilder(SEB_BROWSER_DIRECTORY).Append("\\").Append(XUL_RUNNER_DIRECTORY).Append("\\");
            XulRunnerDirectory = xulRunnerDirectoryBuilder.ToString();

            // Set the location of the XulSebDirectory
            //StringBuilder xulSebDirectoryBuilder = new StringBuilder(SebClientDirectory).Append(XUL_SEB_DIRECTORY).Append("\\");
            //XulSebDirectory = xulSebDirectoryBuilder.ToString();
            StringBuilder xulSebDirectoryBuilder = new StringBuilder(SEB_BROWSER_DIRECTORY).Append("\\").Append(XUL_SEB_DIRECTORY).Append("\\");
            XulSebDirectory = xulSebDirectoryBuilder.ToString();

            // Set the location of the XulRunnerExePath
            //StringBuilder xulRunnerExePathBuilder = new StringBuilder("\"").Append(XulRunnerDirectory).Append(XUL_RUNNER).Append("\"");
            //XulRunnerExePath = xulRunnerExePathBuilder.ToString();
            StringBuilder xulRunnerExePathBuilder = new StringBuilder(XulRunnerDirectory).Append(XUL_RUNNER); //.Append("\"");
            XulRunnerExePath = xulRunnerExePathBuilder.ToString();

            // Set the location of the seb.ini
            StringBuilder xulRunnerSebIniPathBuilder = new StringBuilder(XulSebDirectory).Append(XUL_RUNNER_INI); //.Append("\"");
            XulRunnerSebIniPath = xulRunnerSebIniPathBuilder.ToString();

            // Get the two possible paths of the SebClientSettings.seb file
            StringBuilder sebClientSettingsProgramDataBuilder = new StringBuilder(SebClientSettingsProgramDataDirectory).Append(SEB_CLIENT_CONFIG);
            SebClientSettingsProgramDataFile = sebClientSettingsProgramDataBuilder.ToString();

            StringBuilder sebClientSettingsAppDataBuilder = new StringBuilder(SebClientSettingsAppDataDirectory).Append(SEB_CLIENT_CONFIG);
            SebClientSettingsAppDataFile = sebClientSettingsAppDataBuilder.ToString();

            // Set the default location of the SebClientLogFileDirectory
            SetDefaultClientLogFile();
        }

        /// <summary>
        /// Set the default location of the SebClientLogFileDirectory.
        /// </summary>
        public static void SetDefaultClientLogFile()
        {
            StringBuilder SebClientLogFileDirectoryBuilder = new StringBuilder(SebClientSettingsAppDataDirectory); //.Append("\\").Append(MANUFACTURER_LOCAL).Append("\\");
            SebClientLogFileDirectory = SebClientLogFileDirectoryBuilder.ToString();

            // Set the path of the SebClient.log file
            StringBuilder sebClientLogFileBuilder = new StringBuilder(SebClientLogFileDirectory).Append(SEB_CLIENT_LOG);
            SebClientLogFile = sebClientLogFileBuilder.ToString();
        }

        public static bool CreateNewDesktopOldValue { get; set; }

        public static string ContractEnvironmentVariables(string path)
        {
            path = Path.GetFullPath(path);
            DictionaryEntry currentEntry = new DictionaryEntry("", "");
            foreach (object key in Environment.GetEnvironmentVariables().Keys)
            {
                string value = (string)Environment.GetEnvironmentVariables()[key];
                if (path.ToUpperInvariant().Contains(value.ToUpperInvariant()) && value.Length > ((string)currentEntry.Value).Length)
                {
                    currentEntry.Key = (string)key;
                    currentEntry.Value = value;
                }
            }
            return path.Replace((string)currentEntry.Value, "%" + (string)currentEntry.Key + "%");
        }

    }
}
