using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SebWindowsConfig.Controls;
using SebWindowsConfig.Entities;
using SebWindowsConfig.Utilities;
using DictObj = System.Collections.Generic.Dictionary<string, object>;
using ListObj = System.Collections.Generic.List<object>;



namespace SebWindowsConfig
{
	public partial class SebWindowsConfigForm : Form
	{
		public bool adminPasswordFieldsContainHash = false;
		public bool quitPasswordFieldsContainHash = false;
		public bool settingsPasswordFieldsContainHash = false;

		public bool quittingMyself = false;

		public SEBURLFilter urlFilter;

		string settingsPassword = "";

		private string lastBrowserExamKey = "";
		private string lastSettingsPassword = "";
		private int lastCryptoIdentityIndex = 0;

		internal const string SEB_CONFIG_LOG = "SebConfig.log";

		//X509Certificate2 fileCertificateRef = null;


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// OnLoad: Get the file name from command line arguments and load it.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			string[] args = Environment.GetCommandLineArgs();

			string es = string.Join(", ", args);
			Logger.AddInformation("OnLoad EventArgs: " + es, null, null);

			if (args.Length > 1)
			{
				LoadConfigurationFileIntoEditor(args[1]);
				// Update Browser Exam Key
				lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
				lastSettingsPassword = textBoxSettingsPassword.Text;
				lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
				// Display the new Browser Exam Key in Exam pane
				textBoxBrowserExamKey.Text = lastBrowserExamKey;
				textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
			}
		}


		// ***********
		//
		// Constructor
		//
		// ***********

		public SebWindowsConfigForm()
		{
			// Initialize URL filter
			urlFilter = new SEBURLFilter();

			InitializeComponent();

			// This is necessary to instanciate the password dialog
			//SEBConfigFileManager.InitSEBConfigFileManager();

			/// Initialize the Logger

			//Sets paths to files SEB has to save or read from the file system
			SEBClientInfo.SetSebPaths();

			// Set the path of the SebConfig.log file
			StringBuilder sebConfigLogFileBuilder = new StringBuilder(SEBClientInfo.SebClientLogFileDirectory).Append(SEB_CONFIG_LOG);
			string SebConfigLogFile = sebConfigLogFileBuilder.ToString();

			Logger.InitLogger(SEBClientInfo.SebClientLogFileDirectory, SebConfigLogFile);

			// Set all the default values for the Plist structure "SEBSettings.settingsCurrent"
			SEBSettings.RestoreDefaultAndCurrentSettings();
			SEBSettings.AddDefaultProhibitedProcesses();

			// Initialise the global variables for the GUI widgets
			InitialiseGlobalVariablesForGUIWidgets();

			// Initialise the GUI widgets themselves
			InitialiseGUIWidgets();

			// When starting up, load the default local client settings
			Logger.AddInformation("Loading the default local client settings", null, null);
			SEBClientInfo.LoadingSettingsFileName = "Local Client Settings";
			if (!LoadConfigurationFileIntoEditor(currentPathSebConfigFile))
			{
				// If this didn't work, then there are no local client settings and we set the current settings title to "Default Settings"
				currentPathSebConfigFile = SEBUIStrings.settingsTitleDefaultSettings;
				// Update URL filter rules, also the seb-Browser white/blacklist keys, 
				// which are necessary for compatibility to SEB 2.1.x
				urlFilter.UpdateFilterRules();
				UpdateAllWidgetsOfProgram();
			}

			// Update Browser Exam Key
			lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			lastSettingsPassword = textBoxSettingsPassword.Text;
			lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = lastBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();

			LoadVersionInfo();

		} // end of contructor   SebWindowsConfigForm()

		private void LoadVersionInfo()
		{
			var executable = Assembly.GetEntryAssembly();
			var programBuild = FileVersionInfo.GetVersionInfo(executable.Location).FileVersion;
			var programVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			var statusStrip = new StatusStrip();

			statusStrip.Dock = DockStyle.Bottom;
			statusStrip.SizingGrip = false;
			statusStrip.Items.Add($"SEB Information: Version {programVersion}, Build {programBuild}");

			Controls.Add(statusStrip);

			Height += statusStrip.Height;
		}

		// *************************************************
		// Open the configuration file and read the settings
		// *************************************************
		private Boolean LoadConfigurationFileIntoEditor(String fileName)
		{
			Cursor.Current = Cursors.WaitCursor;
			// Read the file into "new" settings
			Logger.AddInformation("Loading settings from file " + fileName, null, null);

			// Set the filename into the global variable so it gets displayed in the password dialogs
			if (String.IsNullOrEmpty(SEBClientInfo.LoadingSettingsFileName))
			{
				SEBClientInfo.LoadingSettingsFileName = Path.GetFileName(fileName);
			}

			// In these variables we get back the configuration file password the user entered for decrypting and/or 
			// the certificate reference found in the config file:
			string filePassword = null;
			X509Certificate2 fileCertificateRef = null;
			bool passwordIsHash = false;

			if (!SEBSettings.ReadSebConfigurationFile(fileName, true, ref filePassword, ref passwordIsHash, ref fileCertificateRef))
			{
				SEBClientInfo.LoadingSettingsFileName = "";
				return false;
			}
			SEBClientInfo.LoadingSettingsFileName = "";

			if (!String.IsNullOrEmpty(filePassword))
			{
				// If we got the settings password because the user entered it when opening the .seb file, 
				// we store it in a local variable
				settingsPassword = filePassword;
				settingsPasswordFieldsContainHash = passwordIsHash;
			}
			else
			{
				// We didn't get back any settings password, we clear the local variable
				settingsPassword = "";
				settingsPasswordFieldsContainHash = false;
			}

			// Check if we got a certificate reference used to encrypt the openend settings back
			if (fileCertificateRef != null)
			{
				comboBoxCryptoIdentity.SelectedIndex = 0;
				int indexOfCertificateRef = certificateReferences.IndexOf(fileCertificateRef);
				// Find this certificate reference in the list of all certificates from the certificate store
				// if found (this should always be the case), select that certificate in the comboBox list
				if (indexOfCertificateRef != -1) comboBoxCryptoIdentity.SelectedIndex = indexOfCertificateRef + 1;
			}

			// Check if default permitted processes should be removed from settings
			// CheckAndOptionallyRemoveDefaultProhibitedProcesses();

			// Update URL filter rules, also the seb-Browser white/blacklist keys, 
			// which are necessary for compatibility to SEB 2.1.x
			urlFilter.UpdateFilterRules();

			// GUI-related part: Update the widgets
			currentDireSebConfigFile = Path.GetDirectoryName(fileName);
			currentFileSebConfigFile = Path.GetFileName(fileName);
			currentPathSebConfigFile = Path.GetFullPath(fileName);

			UpdateAllWidgetsOfProgram();
			buttonRevertToLastOpened.Enabled = true;
			Cursor.Current = Cursors.Default;
			return true;
		}


		private void CheckAndOptionallyRemoveDefaultProhibitedProcesses()
		{
			//if ((bool) SEBSettings.settingsCurrent[SEBSettings.KeyCreateNewDesktop])
			//{
			//	if (SEBSettings.CheckForDefaultProhibitedProcesses(false))
			//	{
			//		var messageBoxResult = MessageBox.Show(
			//			this,
			//			"Settings contain at least one of the default prohibited processes (mostly web browsers), " +
			//			"which should not run when SEB is used with the Disable Explorer Shell kiosk mode. " +
			//			"As your settings are not using Disable Explorer Shell, " +
			//			"do you want to remove those default prohibited processes from the configuration?",
			//			"Default Prohibited Processes Found",
			//			MessageBoxButtons.YesNo,
			//			MessageBoxIcon.Question);

			//		if (messageBoxResult == DialogResult.Yes)
			//		{
			//			SEBSettings.CheckForDefaultProhibitedProcesses(true);
			//		}
			//	}
			//}
		}


		// ********************************************************
		// Write the settings to the configuration file and save it
		// ********************************************************
		private Boolean SaveConfigurationFileFromEditor(String fileName)
		{
			Cursor.Current = Cursors.WaitCursor;
			// Get settings password
			string filePassword = settingsPassword;

			// Get selected certificate
			X509Certificate2 fileCertificateRef = null;
			int selectedCertificate = (int) SEBSettings.intArrayCurrent[SEBSettings.ValCryptoIdentity];
			if (selectedCertificate > 0)
			{
				fileCertificateRef = (X509Certificate2) certificateReferences[selectedCertificate - 1];

			} //comboBoxCryptoIdentity.SelectedIndex;

			// Get selected config purpose
			int currentConfigPurpose = (int) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, SEBSettings.KeySebConfigPurpose);
			SEBSettings.sebConfigPurposes configPurpose = (SEBSettings.sebConfigPurposes) currentConfigPurpose;
			bool useOldAsymmetricOnlyEncryption = checkBoxUseOldAsymmetricOnlyEncryption.Checked;

			// Write the "new" settings to file
			if (!SEBSettings.WriteSebConfigurationFile(fileName, filePassword, settingsPasswordFieldsContainHash, fileCertificateRef, useOldAsymmetricOnlyEncryption, configPurpose, forEditing: true))
			{
				return false;
			}

			// If the settings could be written to file, update the widgets
			currentDireSebConfigFile = Path.GetDirectoryName(fileName);
			currentFileSebConfigFile = Path.GetFileName(fileName);
			currentPathSebConfigFile = Path.GetFullPath(fileName);

			UpdateAllWidgetsOfProgram();
			Cursor.Current = Cursors.Default;
			return true;
		}





		// *****************************************************
		// Set the widgets to the new settings of SebStarter.ini
		// *****************************************************
		private void UpdateAllWidgetsOfProgram()
		{
			// Update the filename in the title bar
			this.Text = this.ProductName;
			this.Text += " - ";
			this.Text += currentPathSebConfigFile;

			// Group "General"
			textBoxStartURL.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyStartURL];
			textBoxSebServerURL.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeySebServerURL];

			// If an admin password is saved in the settings (as a hash), 
			// then we fill a placeholder string into the admin password text fields
			if (!String.IsNullOrEmpty((String) SEBSettings.settingsCurrent[SEBSettings.KeyHashedAdminPassword]))
			{
				// CAUTION: Do not change the order of setting the placeholders and the flag,
				// since the fired textBox..._TextChanged() events use these data!
				textBoxAdminPassword.Text = "0000000000000000";
				adminPasswordFieldsContainHash = true;
				textBoxConfirmAdminPassword.Text = "0000000000000000";
			}
			else
			{
				// CAUTION: Do not change the order of setting the placeholders and the flag,
				// since the fired textBox..._TextChanged() events use these data!
				adminPasswordFieldsContainHash = false;
				textBoxAdminPassword.Text = "";
				textBoxConfirmAdminPassword.Text = "";
			}

			checkBoxAllowQuit.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowQuit];
			checkBoxIgnoreExitKeys.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyIgnoreExitKeys];

			// If a quit password is saved in the settings (as a hash), 
			// then we fill a placeholder string into the quit password text fields
			if (!String.IsNullOrEmpty((String) SEBSettings.settingsCurrent[SEBSettings.KeyHashedQuitPassword]))
			{
				// CAUTION: Do not change the order of setting the placeholders and the flag,
				// since the fired textBox..._TextChanged() events use these data!
				textBoxQuitPassword.Text = "0000000000000000";
				quitPasswordFieldsContainHash = true;
				textBoxConfirmQuitPassword.Text = "0000000000000000";
			}
			else
			{
				// CAUTION: Do not change the order of setting the placeholders and the flag,
				// since the fired textBox..._TextChanged() events use these data!
				quitPasswordFieldsContainHash = false;
				textBoxQuitPassword.Text = "";
				textBoxConfirmQuitPassword.Text = "";
			}

			listBoxExitKey1.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyExitKey1];
			listBoxExitKey2.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyExitKey2];
			listBoxExitKey3.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyExitKey3];

			// Group "Config File"
			radioButtonStartingAnExam.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] == 0);
			radioButtonConfiguringAClient.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] == 1);
			checkBoxAllowPreferencesWindow.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowPreferencesWindow];
			checkBoxUseOldAsymmetricOnlyEncryption.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyUseAsymmetricOnlyEncryption];
			comboBoxCryptoIdentity.SelectedIndex = SEBSettings.intArrayCurrent[SEBSettings.ValCryptoIdentity];

			// If the settings password local variable contains a hash (and it isn't empty)
			if (settingsPasswordFieldsContainHash && !String.IsNullOrEmpty(settingsPassword))
			{
				// CAUTION: We need to reset this flag BEFORE changing the textBox text value,
				// because otherwise the compare passwords method will delete the first textBox again.
				settingsPasswordFieldsContainHash = false;
				textBoxSettingsPassword.Text = "0000000000000000";
				settingsPasswordFieldsContainHash = true;
				textBoxConfirmSettingsPassword.Text = "0000000000000000";
			}
			else
			{
				textBoxSettingsPassword.Text = settingsPassword;
				textBoxConfirmSettingsPassword.Text = settingsPassword;
			}

			// Group "User Interface"
			if ((Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyTouchOptimized] == true)
			{
				radioButtonTouchOptimized.Checked = true;
			}
			else
			{
				radioButtonUseBrowserWindow.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserViewMode] == 0);
				radioButtonUseFullScreenMode.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserViewMode] == 1);
			}
			comboBoxMainBrowserWindowWidth.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowWidth];
			comboBoxMainBrowserWindowHeight.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowHeight];
			listBoxMainBrowserWindowPositioning.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowPositioning];
			checkBoxEnableBrowserWindowToolbar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableBrowserWindowToolbar];
			checkBoxHideBrowserWindowToolbar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyHideBrowserWindowToolbar];
			checkBoxShowMenuBar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowMenuBar];
			checkBoxShowTaskBar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowTaskBar];
			checkBoxHideBrowserWindowToolbar.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxAllowMainWindowAddressBar.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxAllowAdditionalWindowAddressBar.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxShowSideMenu.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowSideMenu];
			checkBoxShowReloadButton.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowReloadButton];
			comboBoxTaskBarHeight.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyTaskBarHeight].ToString();
			checkBoxEnableTouchExit.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableTouchExit];
			checkBoxAllowMainWindowAddressBar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserWindowAllowAddressBar];
			checkBoxAllowAdditionalWindowAddressBar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowAllowAddressBar];
			checkBoxAllowDeveloperConsole.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowDeveloperConsole];
			checkBoxAllowDeveloperConsole.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;

			var defaultText = "(part of application)";
			var defaultStyle = new DataGridViewCellStyle { BackColor = Color.LightGray };
			var allowedDictionaries = SEBSettings.settingsCurrent[SEBSettings.KeyAllowSpellCheckDictionary] as ListObj;
			var compressor = new FileCompressor();
			var danish = new DataGridViewRow();
			var australian = new DataGridViewRow();
			var british = new DataGridViewRow();
			var american = new DataGridViewRow();
			var spanish = new DataGridViewRow();
			var french = new DataGridViewRow();
			var portuguese = new DataGridViewRow();
			var swedish = new DataGridViewRow();
			var swedishFinland = new DataGridViewRow();

			danish.Tag = true;
			danish.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "da-DK"), Style = defaultStyle });
			danish.Cells.Add(new DataGridViewTextBoxCell { Value = "da-DK", Style = defaultStyle });
			danish.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			australian.Tag = true;
			australian.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "en-AU"), Style = defaultStyle });
			australian.Cells.Add(new DataGridViewTextBoxCell { Value = "en-AU", Style = defaultStyle });
			australian.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			british.Tag = true;
			british.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "en-GB"), Style = defaultStyle });
			british.Cells.Add(new DataGridViewTextBoxCell { Value = "en-GB", Style = defaultStyle });
			british.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			american.Tag = true;
			american.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "en-US"), Style = defaultStyle });
			american.Cells.Add(new DataGridViewTextBoxCell { Value = "en-US", Style = defaultStyle });
			american.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			spanish.Tag = true;
			spanish.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "es-ES"), Style = defaultStyle });
			spanish.Cells.Add(new DataGridViewTextBoxCell { Value = "es-ES", Style = defaultStyle });
			spanish.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			french.Tag = true;
			french.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "fr-FR"), Style = defaultStyle });
			french.Cells.Add(new DataGridViewTextBoxCell { Value = "fr-FR", Style = defaultStyle });
			french.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			portuguese.Tag = true;
			portuguese.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "pt-PT"), Style = defaultStyle });
			portuguese.Cells.Add(new DataGridViewTextBoxCell { Value = "pt-PT", Style = defaultStyle });
			portuguese.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			swedish.Tag = true;
			swedish.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "sv-SE"), Style = defaultStyle });
			swedish.Cells.Add(new DataGridViewTextBoxCell { Value = "sv-SE", Style = defaultStyle });
			swedish.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			swedishFinland.Tag = true;
			swedishFinland.Cells.Add(new DataGridViewCheckBoxCell(false) { Value = allowedDictionaries.Any(l => l as string == "sv-FI"), Style = defaultStyle });
			swedishFinland.Cells.Add(new DataGridViewTextBoxCell { Value = "sv-FI", Style = defaultStyle });
			swedishFinland.Cells.Add(new DataGridViewTextBoxCell { Value = defaultText, Style = defaultStyle });

			spellCheckerDataGridView.Rows.Clear();
			spellCheckerDataGridView.Rows.Add(danish);
			spellCheckerDataGridView.Rows.Add(australian);
			spellCheckerDataGridView.Rows.Add(british);
			spellCheckerDataGridView.Rows.Add(american);
			spellCheckerDataGridView.Rows.Add(spanish);
			spellCheckerDataGridView.Rows.Add(french);
			spellCheckerDataGridView.Rows.Add(portuguese);
			spellCheckerDataGridView.Rows.Add(swedish);
			spellCheckerDataGridView.Rows.Add(swedishFinland);

			foreach (DictObj dictionary in SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalDictionaries] as ListObj)
			{
				var locale = dictionary[SEBSettings.KeyAdditionalDictionaryLocale] as string;
				var data = dictionary[SEBSettings.KeyAdditionalDictionaryData] as string;
				var enabled = allowedDictionaries.Any(l => l as string == locale);
				var files = compressor.GetFileList(data);
				var fileList = ToDictionaryFileList(files);

				spellCheckerDataGridView.Rows.Add(enabled, locale, fileList);
			}

			// Group "Browser"
			listBoxOpenLinksHTML.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkPolicy];
			checkBoxBlockLinksHTML.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkBlockForeign];

			comboBoxNewBrowserWindowWidth.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkWidth];
			comboBoxNewBrowserWindowHeight.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkHeight];
			listBoxNewBrowserWindowPositioning.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkPositioning];

			checkBoxEnablePlugIns.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnablePlugIns];
			checkBoxEnableJava.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableJava];
			checkBoxEnableJavaScript.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableJavaScript];
			checkBoxBlockPopUpWindows.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyBlockPopUpWindows];
			checkBoxAllowVideoCapture.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowVideoCapture];
			checkBoxAllowAudioCapture.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowAudioCapture];
			checkBoxAllowBrowsingBackForward.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowBrowsingBackForward];
			checkBoxAllowNavigationNewWindow.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowNavigation];
			checkBoxAllowReload.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserWindowAllowReload];
			checkBoxShowReloadWarning.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowReloadWarning];
			checkBoxAllowReloadNewWindow.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowAllowReload];
			checkBoxShowReloadWarningNewWindow.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowShowReloadWarning];
			checkBoxEnableZoomPage.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableZoomPage];
			checkBoxEnableZoomText.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableZoomText];
			radioButtonUseZoomPage.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyZoomMode] == 0);
			radioButtonUseZoomText.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyZoomMode] == 1);
			enableZoomAdjustZoomMode();
			checkBoxEnableZoomText.CheckedChanged += checkBoxEnableZoomText_CheckedChanged;
			checkBoxEnableZoomPage.CheckedChanged += checkBoxEnableZoomPage_CheckedChanged;
			checkBoxAllowPdfReaderToolbar.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowPDFReaderToolbar];
			checkBoxAllowFind.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowFind];
			checkBoxAllowPrint.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowPrint];

			checkBoxAllowSpellCheck.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowSpellCheck];
			checkBoxAllowDictionaryLookup.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowDictionaryLookup];
			checkBoxRemoveProfile.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyRemoveBrowserProfile];
			checkBoxRemoveProfile.Enabled = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyExamSessionClearCookiesOnEnd];
			checkBoxDisableLocalStorage.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyDisableLocalStorage];
			checkBoxUseSebWithoutBrowser.Checked = !((Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableSebBrowser]);
			// BEWARE: you must invert this value since "Use Without" is "Not Enable"!

			textBoxUserAgent.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgent];

			textBoxUserAgentMacCustom.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentMacCustom];
			radioButtonUserAgentMacDefault.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentMac] == 0);
			radioButtonUserAgentMacCustom.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentMac] == 1);
			textBoxUserAgentMacCustom.TextChanged += textBoxUserAgentMacCustom_TextChanged;

			textBoxUserAgentDesktopModeCustom.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopModeCustom];
			textBoxUserAgentDesktopModeDefault.Text = SEBClientInfo.BROWSER_USERAGENT_DESKTOP;
			radioButtonUserAgentDesktopDefault.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopMode] == 0);
			radioButtonUserAgentDesktopCustom.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopMode] == 1);
			textBoxUserAgentDesktopModeCustom.TextChanged += textBoxUserAgentDesktopModeCustom_TextChanged;

			textBoxUserAgentTouchModeCustom.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchModeCustom];
			textBoxUserAgentTouchModeDefault.Text = SEBClientInfo.BROWSER_USERAGENT_TOUCH;
			textBoxUserAgentTouchModeIPad.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchModeIPad];
			radioButtonUserAgentTouchDefault.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] == 0);
			radioButtonUserAgentTouchIPad.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] == 1);
			radioButtonUserAgentTouchCustom.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] == 2);
			textBoxUserAgentTouchModeCustom.TextChanged += textBoxUserAgentTouchModeCustom_TextChanged;


			textBoxBrowserSuffix.Text = ((string) SEBSettings.settingsCurrent[SEBSettings.KeyBrowserWindowTitleSuffix]);

			checkBoxEnableAudioControl.Checked = ((bool) SEBSettings.settingsCurrent[SEBSettings.KeyAudioControlEnabled]);
			checkBoxMuteAudio.Checked = ((bool) SEBSettings.settingsCurrent[SEBSettings.KeyAudioMute]);
			checkBoxSetVolumeLevel.Checked = ((bool) SEBSettings.settingsCurrent[SEBSettings.KeyAudioSetVolumeLevel]);
			trackBarVolumeLevel.Value = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyAudioVolumeLevel]);

			comboBoxUrlPolicyMainWindow.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowUrlPolicy];
			comboBoxUrlPolicyNewWindow.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowUrlPolicy];

			// Group "Down/Uploads"
			checkBoxAllowDownUploads.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowDownUploads];
			checkBoxAllowCustomDownloadLocation.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowCustomDownUploadLocation];
			checkBoxOpenDownloads.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyOpenDownloads];
			checkBoxDownloadPDFFiles.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyDownloadPDFFiles];
			checkBoxAllowPDFPlugIn.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowPDFPlugIn];
			textBoxDownloadDirectoryWin.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyDownloadDirectoryWin];
			checkBoxTemporaryDownloadDirectory.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyUseTemporaryDownUploadDirectory];
			textBoxDownloadDirectoryWin.Enabled = !checkBoxTemporaryDownloadDirectory.Checked;
			buttonDownloadDirectoryWin.Enabled = !checkBoxTemporaryDownloadDirectory.Checked;
			textBoxDownloadDirectoryOSX.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyDownloadDirectoryOSX];
			listBoxChooseFileToUploadPolicy.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyChooseFileToUploadPolicy];
			checkBoxDownloadOpenSEBFiles.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyDownloadAndOpenSebConfig];
			checkBoxShowFileSystemElementPath.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowFileSystemElementPath];

			// Group "Exam"
			checkBoxSendBrowserExamKey.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeySendBrowserExamKey];
			checkBoxClearSessionOnEnd.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyExamSessionClearCookiesOnEnd];
			checkBoxClearSessionOnStart.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyExamSessionClearCookiesOnStart];
			textBoxBrowserExamKey.Enabled = checkBoxSendBrowserExamKey.Checked;
			textBoxConfigurationKey.Enabled = checkBoxSendBrowserExamKey.Checked;
			textBoxQuitURL.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyQuitURL];
			checkBoxQuitURLConfirm.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyQuitURLConfirm];
			checkBoxResetOnQuitUrl.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyResetOnQuitUrl];
			checkBoxUseStartURL.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamUseStartURL];
			textBoxRestartExamLink.Enabled = !(Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamUseStartURL];
			checkBoxRestartExamPasswordProtected.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamPasswordProtected];
			textBoxRestartExamLink.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamURL];
			textBoxRestartExamText.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamText];
			checkBoxAllowReconfiguration.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowReconfiguration];
			textBoxReconfigurationUrl.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyReconfigurationUrl];
			checkBoxUseStartUrlQuery.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyUseStartUrlQuery];

			// Group AdditionalResources
			tabControlSebWindowsConfig.TabPages.Remove(tabPageAdditionalResources);
			//tabPageAdditionalResources.Controls.Clear();
			//tabPageAdditionalResources.Controls.Add(new AdditionalResources());

			//FillStartupResourcesinCombobox();
			//foreach (KeyValuePair<string, string> item in comboBoxAdditionalResourceStartUrl.Items)
			//{
			//	if (item.Key == SEBSettings.settingsCurrent[SEBSettings.KeyStartResource].ToString())
			//	{
			//		comboBoxAdditionalResourceStartUrl.SelectedItem = item;
			//	}
			//}

			// Group "Applications"
			checkBoxMonitorProcesses.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyMonitorProcesses];
			checkBoxAllowSwitchToApplications.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowSwitchToApplications];
			checkBoxAllowFlashFullscreen.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowFlashFullscreen];


			// Group "Applications - Permitted/Prohibited Processes"
			// Group "Network      -    Filter/Certificates/Proxies"

			// Update the lists for the DataGridViews
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];

			SEBSettings.bypassedProxyList = (ListObj) SEBSettings.proxiesData[SEBSettings.KeyExceptionsList];

			// Check if currently loaded lists have any entries
			if (SEBSettings.permittedProcessList.Count > 0)
				SEBSettings.permittedProcessIndex = 0;
			else SEBSettings.permittedProcessIndex = -1;

			if (SEBSettings.prohibitedProcessList.Count > 0)
				SEBSettings.prohibitedProcessIndex = 0;
			else SEBSettings.prohibitedProcessIndex = -1;

			if (SEBSettings.embeddedCertificateList.Count > 0)
				SEBSettings.embeddedCertificateIndex = 0;
			else SEBSettings.embeddedCertificateIndex = -1;

			SEBSettings.proxyProtocolIndex = 0;

			if (SEBSettings.bypassedProxyList.Count > 0)
				SEBSettings.bypassedProxyIndex = 0;
			else SEBSettings.bypassedProxyIndex = -1;

			// Remove all previously displayed list entries from DataGridViews
			groupBoxPermittedProcess.Enabled = (SEBSettings.permittedProcessList.Count > 0);
			dataGridViewPermittedProcesses.Enabled = (SEBSettings.permittedProcessList.Count > 0);
			dataGridViewPermittedProcesses.Rows.Clear();

			groupBoxProhibitedProcess.Enabled = (SEBSettings.prohibitedProcessList.Count > 0);
			dataGridViewProhibitedProcesses.Enabled = (SEBSettings.prohibitedProcessList.Count > 0);
			dataGridViewProhibitedProcesses.Rows.Clear();

			dataGridViewEmbeddedCertificates.Enabled = (SEBSettings.embeddedCertificateList.Count > 0);
			dataGridViewEmbeddedCertificates.Rows.Clear();

			dataGridViewProxyProtocols.Enabled = true;
			dataGridViewProxyProtocols.Rows.Clear();

			textBoxBypassedProxyHostList.Text = "";

			// Add Permitted Processes of currently opened file to DataGridView
			for (int index = 0; index < SEBSettings.permittedProcessList.Count; index++)
			{
				SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[index];
				Boolean active = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyActive];
				Int32 os = (Int32) SEBSettings.permittedProcessData[SEBSettings.KeyOS];
				String executable = (String) SEBSettings.permittedProcessData[SEBSettings.KeyExecutable];
				String title = (String) SEBSettings.permittedProcessData[SEBSettings.KeyTitle];
				dataGridViewPermittedProcesses.Rows.Add(active, StringOS[os], executable, title);
			}

			// Add Prohibited Processes of currently opened file to DataGridView
			for (int index = 0; index < SEBSettings.prohibitedProcessList.Count; index++)
			{
				SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[index];
				Boolean active = (Boolean) SEBSettings.prohibitedProcessData[SEBSettings.KeyActive];
				Int32 os = (Int32) SEBSettings.prohibitedProcessData[SEBSettings.KeyOS];
				String executable = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyExecutable];
				String description = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyDescription];
				dataGridViewProhibitedProcesses.Rows.Add(active, StringOS[os], executable, description);
			}

			// Add Url Filters
			var filterControl = new FilterRuleControl();

			SEBSettings.urlFilterRuleList = SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterRules] as ListObj;

			foreach (DictObj config in SEBSettings.urlFilterRuleList)
			{
				var rule = FilterRule.FromConfig(config);

				filterControl.AddRule(rule);
			}

			filterControl.Dock = DockStyle.Fill;
			filterControl.DataChanged += (rules) =>
			{
				SEBSettings.urlFilterRuleList.Clear();

				foreach (var rule in rules)
				{
					var config = FilterRule.ToConfig(rule);

					SEBSettings.urlFilterRuleList.Add(config);
				}
			};

			UrlFilterContainer.Controls.Clear();
			UrlFilterContainer.Controls.Add(filterControl);

			// Add Embedded Certificates of Certificate Store to DataGridView
			for (int index = 0; index < SEBSettings.embeddedCertificateList.Count; index++)
			{
				SEBSettings.embeddedCertificateData = (DictObj) SEBSettings.embeddedCertificateList[index];
				Int32 type = (Int32) SEBSettings.embeddedCertificateData[SEBSettings.KeyType];
				String name = (String) SEBSettings.embeddedCertificateData[SEBSettings.KeyName];
				dataGridViewEmbeddedCertificates.Rows.Add(StringCertificateType[type], name);

				// For downwards compatibility of embedded SSL certs, if there is no data in the new data key certificateDataBase64
				// we read data from the old data key certificateDataWin and save it to the new key. 
				// Please note: The Browser Exam Key of these settings is changed by this
				if (type == IntSSLClientCertificate && String.IsNullOrEmpty((String) SEBSettings.embeddedCertificateData[SEBSettings.KeyCertificateDataBase64]))
				{
					String base64Data = (String) SEBSettings.embeddedCertificateData[SEBSettings.KeyCertificateDataWin];
					SEBSettings.embeddedCertificateData[SEBSettings.KeyCertificateDataBase64] = base64Data;
				}
			}
			/*
						// Get the "Enabled" boolean values of current "proxies" dictionary
						BooleanProxyProtocolEnabled[IntProxyAutoDiscovery    ] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeyAutoDiscoveryEnabled];
						BooleanProxyProtocolEnabled[IntProxyAutoConfiguration] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeyAutoConfigurationEnabled];
						BooleanProxyProtocolEnabled[IntProxyHTTP             ] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeyHTTPEnable];
						BooleanProxyProtocolEnabled[IntProxyHTTPS            ] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeyHTTPSEnable];
						BooleanProxyProtocolEnabled[IntProxyFTP              ] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeyFTPEnable];
						BooleanProxyProtocolEnabled[IntProxySOCKS            ] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeySOCKSEnable];
						BooleanProxyProtocolEnabled[IntProxyRTSP             ] = (Boolean)SEBSettings.proxiesData[SEBSettings.KeyRTSPEnable];
			*/
			// Get the "Enabled" boolean values of current "proxies" dictionary.
			// Add Proxy Protocols of currently opened file to DataGridView.
			for (int index = 0; index < NumProxyProtocols; index++)
			{
				Boolean enable = (Boolean) SEBSettings.proxiesData[KeyProxyProtocolEnable[index]];
				String type = (String) StringProxyProtocolTableCaption[index];
				dataGridViewProxyProtocols.Rows.Add(enable, type);
				BooleanProxyProtocolEnabled[index] = enable;
			}

			// Add Bypassed Proxies of currently opened file to the comma separated list
			StringBuilder bypassedProxiesStringBuilder = new StringBuilder();
			for (int index = 0; index < SEBSettings.bypassedProxyList.Count; index++)
			{
				SEBSettings.bypassedProxyData = (String) SEBSettings.bypassedProxyList[index];
				if (bypassedProxiesStringBuilder.Length > 0)
				{
					bypassedProxiesStringBuilder.Append(", ");
				}
				bypassedProxiesStringBuilder.Append(SEBSettings.bypassedProxyData);
			}
			textBoxBypassedProxyHostList.Text = bypassedProxiesStringBuilder.ToString();

			// Load the currently selected process data
			if (SEBSettings.permittedProcessList.Count > 0)
				LoadAndUpdatePermittedSelectedProcessGroup(SEBSettings.permittedProcessIndex);
			else ClearPermittedSelectedProcessGroup();

			if (SEBSettings.prohibitedProcessList.Count > 0)
				LoadAndUpdateProhibitedSelectedProcessGroup(SEBSettings.prohibitedProcessIndex);
			else ClearProhibitedSelectedProcessGroup();

			// Auto-resize the columns and cells

			//dataGridViewPermittedProcesses  .AutoResizeColumns();
			//dataGridViewProhibitedProcesses .AutoResizeColumns();
			//dataGridViewURLFilterRules      .AutoResizeColumns();
			//dataGridViewEmbeddedCertificates.AutoResizeColumns();
			//dataGridViewProxyProtocols      .AutoResizeColumns();
			//dataGridViewBypassedProxies     .AutoResizeColumns();

			//dataGridViewPermittedProcesses  .AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
			//dataGridViewProhibitedProcesses .AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
			//dataGridViewURLFilterRules      .AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
			//dataGridViewEmbeddedCertificates.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
			//dataGridViewProxyProtocols      .AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
			//dataGridViewBypassedProxies     .AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

			//Group "Network - URL Filter"
			checkBoxEnableURLFilter.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnable];
			checkBoxEnableURLContentFilter.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnableContentFilter];
			checkBoxEnableURLContentFilter.Enabled = checkBoxEnableURLFilter.Checked;

			// Group "Network - Certificates"
			checkBoxPinEmbeddedCertificates.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyPinEmbeddedCertificates];

			// Group "Network - Proxies"
			radioButtonUseSystemProxySettings.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyProxySettingsPolicy] == 0);
			radioButtonUseSebProxySettings.Checked = ((int) SEBSettings.settingsCurrent[SEBSettings.KeyProxySettingsPolicy] == 1);

			textBoxAutoProxyConfigurationURL.Text = (String) SEBSettings.proxiesData[SEBSettings.KeyAutoConfigurationURL];
			checkBoxExcludeSimpleHostnames.Checked = (Boolean) SEBSettings.proxiesData[SEBSettings.KeyExcludeSimpleHostnames];
			checkBoxUsePassiveFTPMode.Checked = (Boolean) SEBSettings.proxiesData[SEBSettings.KeyFTPPassive];

			// Group "Security"
			listBoxSebServicePolicy.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeySebServicePolicy];
			checkBoxSebServiceIgnore.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeySebServiceIgnore];
			labelSebServiceIgnore.Enabled = !checkBoxSebServiceIgnore.Checked;
			labelSebServicePolicy.Enabled = !checkBoxSebServiceIgnore.Checked;
			listBoxSebServicePolicy.Enabled = !checkBoxSebServiceIgnore.Checked;
			groupBoxInsideSeb.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowWindowsUpdate.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowScreenSharing.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowChromeNotifications.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowVirtualMachine.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowVirtualMachine];
			checkBoxAllowScreenSharing.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowScreenSharing];
			checkBoxEnablePrivateClipboard.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnablePrivateClipboard];
			radioCreateNewDesktop.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyCreateNewDesktop];
			radioKillExplorerShell.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyKillExplorerShell];
			radioNoKiosMode.Checked = !(Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyKillExplorerShell] && !(Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyCreateNewDesktop];
			checkBoxEnableLogging.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableLogging];
			textBoxLogDirectoryWin.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyLogDirectoryWin];
			checkBoxAllowLogAccess.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowApplicationLog];
			checkBoxShowLogButton.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowApplicationLogButton];
			checkBoxShowLogButton.Enabled = checkBoxAllowLogAccess.Checked;
			checkBoxAllowChromeNotifications.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowChromeNotifications];
			checkBoxAllowWindowsUpdate.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowWindowsUpdate];

			if (String.IsNullOrEmpty(textBoxLogDirectoryWin.Text))
			{
				checkBoxUseStandardDirectory.Checked = true;
			}
			else
			{
				checkBoxUseStandardDirectory.Checked = false;
			}
			textBoxLogDirectoryOSX.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyLogDirectoryOSX];
			checkboxAllowWlan.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowWLAN];
			checkBoxEnableScreenCapture.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnablePrintScreen];

			comboBoxMinMacOSVersion.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyMinMacOSVersion];
			checkBoxEnableAppSwitcherCheck.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableAppSwitcherCheck];
			checkBoxForceAppFolderInstall.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyForceAppFolderInstall];
			checkBoxAllowUserAppFolderInstall.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowUserAppFolderInstall];
			checkBoxAllowSiri.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowSiri];
			checkBoxAllowDictation.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowDictation];
			checkBoxDetectStoppedProcess.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyDetectStoppedProcess];
			checkBoxAllowDisplayMirroring.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowDisplayMirroring];
			comboBoxAllowedDisplaysMaxNumber.Text = (String) SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplaysMaxNumber].ToString();
			checkBoxAllowedDisplayBuiltin.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplayBuiltin];
			checkBoxEnforceBuiltinDisplay.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplayBuiltinEnforce];
			checkBoxAllowedDisplayIgnoreError.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplayIgnoreFailure];

			// Group "Registry"
			checkBoxInsideSebEnableSwitchUser.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableSwitchUser];
			checkBoxInsideSebEnableLockThisComputer.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableLockThisComputer];
			checkBoxInsideSebEnableChangeAPassword.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableChangeAPassword];
			checkBoxInsideSebEnableStartTaskManager.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableStartTaskManager];
			checkBoxInsideSebEnableLogOff.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableLogOff];
			checkBoxInsideSebEnableShutDown.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableShutDown];
			checkBoxInsideSebEnableEaseOfAccess.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableEaseOfAccess];
			checkBoxSetVmwareConfiguration.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeySetVmwareConfiguration];
			checkBoxInsideSebEnableVmWareClientShade.Enabled = checkBoxSetVmwareConfiguration.Checked;
			checkBoxInsideSebEnableVmWareClientShade.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableVmWareClientShade];
			checkBoxInsideSebEnableNetworkConnectionSelector.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableNetworkConnectionSelector];
			checkBoxEnableFindPrinter.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableFindPrinter];

			// Group "Hooked Keys"
			checkBoxHookKeys.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyHookKeys];

			checkBoxEnableEsc.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableEsc];
			checkBoxEnableCtrlEsc.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableCtrlEsc];
			checkBoxEnableAltEsc.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltEsc];
			checkBoxEnableAltTab.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltTab];
			checkBoxEnableAltF4.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltF4];
			checkBoxEnableMiddleMouse.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableMiddleMouse];
			checkBoxEnableRightMouse.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableRightMouse];
			checkBoxEnablePrintScreen.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnablePrintScreen];
			checkBoxEnableAltMouseWheel.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltMouseWheel];

			checkBoxEnableF1.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF1];
			checkBoxEnableF2.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF2];
			checkBoxEnableF3.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF3];
			checkBoxEnableF4.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF4];
			checkBoxEnableF5.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF5];
			checkBoxEnableF6.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF6];
			checkBoxEnableF7.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF7];
			checkBoxEnableF8.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF8];
			checkBoxEnableF9.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF9];
			checkBoxEnableF10.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF10];
			checkBoxEnableF11.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF11];
			checkBoxEnableF12.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyEnableF12];

			checkBoxShowTime.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowTime];
			checkBoxShowKeyboardLayout.Checked = (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyShowInputLanguage];

			return;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Compare password textfields and show or hide compare label accordingly
		/// if passwords are same, save the password hash
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		public void ComparePasswords(TextBox passwordField, TextBox confirmPasswordField, ref bool passwordFieldsContainHash, Label label, string settingsKey)
		{
			// Get the password text from the text fields
			string password = passwordField.Text;
			string confirmPassword = confirmPasswordField.Text;

			if (passwordFieldsContainHash)
			{
				// If the flag is set for password fields contain a placeholder 
				// instead of the hash loaded from settings (no clear text password)
				if (password.CompareTo(confirmPassword) != 0)
				{
					// and when the password texts aren't the same anymore, this means the user tries to edit the password
					// (which is only the placeholder right now), we have to clear the placeholder from the textFields
					passwordField.Text = password;
					confirmPasswordField.Text = confirmPassword;
					passwordFieldsContainHash = false;
				}
			}

			// Compare text value of password fields, regardless if they contain actual passwords or a hash
			if (password.CompareTo(confirmPassword) == 0)
			{
				/// Passwords are same
				// Hide the "Please confirm password" label
				label.Visible = false;

				String newStringHashcode = "";
				if (!passwordFieldsContainHash && !String.IsNullOrEmpty(password) && settingsKey != null)
				{
					// If the password isn't the placeholder for the hash, isn't empty 
					// and we got the key to where to save the hash (for the settings pw we don't need a hash)
					// we hash the password, otherwise just save the empty string into settings
					// Password hashing using the SHA-256 hash algorithm
					SHA256 sha256Algorithm = new SHA256Managed();
					// Hash the new quit password
					byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
					byte[] hashcodeBytes = sha256Algorithm.ComputeHash(passwordBytes);
					// Generate a base16 hash string
					newStringHashcode = BitConverter.ToString(hashcodeBytes);
					newStringHashcode = newStringHashcode.Replace("-", "");
				}
				// Save the new password into settings password variable
				if (!passwordFieldsContainHash && settingsKey == null)
				{
					settingsPassword = password;
				}
				// Save the new hash string into settings 
				if (!passwordFieldsContainHash && settingsKey != null) SEBSettings.settingsCurrent[settingsKey] = newStringHashcode;
				// Enable the save/use settings buttons
				//SetButtonsCommandsEnabled(true);
			}
			else
			{
				/// Passwords are not same

				// If this was a settings password hash and it got edited: Clear the settings password variable and the hash flag
				if (passwordFieldsContainHash && settingsKey == null)
				{
					settingsPassword = "";
					settingsPasswordFieldsContainHash = false;
				}

				//SetButtonsCommandsEnabled(false);
				label.Visible = true;
			}
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Change the enabled status of buttons and menu commands 
		/// for saving and using current settings.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private void SetButtonsCommandsEnabled(bool enabledStatus)
		{
			buttonSaveSettings.Enabled = enabledStatus;
			buttonSaveSettingsAs.Enabled = enabledStatus;
			buttonConfigureClient.Enabled = enabledStatus;
			buttonApplyAndStartSEB.Enabled = enabledStatus;

			saveSettingsToolStripMenuItem.Enabled = enabledStatus;
			saveSettingsAsToolStripMenuItem.Enabled = enabledStatus;
			configureClientToolStripMenuItem.Enabled = enabledStatus;
			applyAndStartSEBToolStripMenuItem.Enabled = enabledStatus;
		}


		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Check if there are some unconfirmed passwords and show alert if so.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private bool ArePasswordsUnconfirmed()
		{
			bool passwordIsUnconfirmed = false;
			string unconfirmedPassword;

			if (textBoxAdminPassword.Text.CompareTo(textBoxConfirmAdminPassword.Text) != 0)
			{
				unconfirmedPassword = SEBUIStrings.passwordAdmin;
				PresentAlertForUnconfirmedPassword(unconfirmedPassword);
				passwordIsUnconfirmed = true;
			}

			if (textBoxQuitPassword.Text.CompareTo(textBoxConfirmQuitPassword.Text) != 0)
			{
				unconfirmedPassword = SEBUIStrings.passwordQuit;
				PresentAlertForUnconfirmedPassword(unconfirmedPassword);
				passwordIsUnconfirmed = true;
			}

			if (textBoxSettingsPassword.Text.CompareTo(textBoxConfirmSettingsPassword.Text) != 0)
			{
				unconfirmedPassword = SEBUIStrings.passwordSettings;
				PresentAlertForUnconfirmedPassword(unconfirmedPassword);
				passwordIsUnconfirmed = true;
			}

			return passwordIsUnconfirmed;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Check if there are some unconfirmed passwords and show alert if so.
		/// </summary>
		/// ----------------------------------------------------------------------------------------
		private void PresentAlertForUnconfirmedPassword(string unconfirmedPassword)
		{
			MessageBox.Show(SEBUIStrings.unconfirmedPasswordMessage.Replace("%s", unconfirmedPassword), SEBUIStrings.unconfirmedPasswordTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		// **************
		//
		// Event handlers
		//
		// **************



		// ***************
		// Group "General"
		// ***************
		private void textBoxStartURL_TextChanged(object sender, EventArgs e)
		{
			comboBoxAdditionalResourceStartUrl.Enabled = string.IsNullOrEmpty(textBoxStartURL.Text);
			if (!string.IsNullOrEmpty(textBoxStartURL.Text))
			{
				comboBoxAdditionalResourceStartUrl.SelectedItem = null;
				comboBoxAdditionalResourceStartUrl.Text = "Choose an embedded resource...";
				SEBSettings.settingsCurrent[SEBSettings.KeyStartResource] = "";
			}
			SEBSettings.settingsCurrent[SEBSettings.KeyStartURL] = textBoxStartURL.Text;
		}

		private void textBoxSebServerURL_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeySebServerURL] = textBoxSebServerURL.Text;
		}

		private void textBoxAdminPassword_TextChanged(object sender, EventArgs e)
		{
			ComparePasswords(textBoxAdminPassword, textBoxConfirmAdminPassword, ref adminPasswordFieldsContainHash, labelAdminPasswordCompare, SEBSettings.KeyHashedAdminPassword);
		}

		private void textBoxConfirmAdminPassword_TextChanged(object sender, EventArgs e)
		{
			ComparePasswords(textBoxAdminPassword, textBoxConfirmAdminPassword, ref adminPasswordFieldsContainHash, labelAdminPasswordCompare, SEBSettings.KeyHashedAdminPassword);
		}

		private void checkBoxAllowQuit_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowQuit] = checkBoxAllowQuit.Checked;
		}

		private void textBoxQuitPassword_TextChanged(object sender, EventArgs e)
		{
			ComparePasswords(textBoxQuitPassword, textBoxConfirmQuitPassword, ref quitPasswordFieldsContainHash, labelQuitPasswordCompare, SEBSettings.KeyHashedQuitPassword);
		}


		private void textBoxConfirmQuitPassword_TextChanged(object sender, EventArgs e)
		{
			ComparePasswords(textBoxQuitPassword, textBoxConfirmQuitPassword, ref quitPasswordFieldsContainHash, labelQuitPasswordCompare, SEBSettings.KeyHashedQuitPassword);
		}


		private void checkBoxIgnoreExitKeys_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyIgnoreExitKeys] = checkBoxIgnoreExitKeys.Checked;
			groupBoxExitSequence.Enabled = !checkBoxIgnoreExitKeys.Checked;
		}

		private void listBoxExitKey1_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Make sure that all three exit keys are different.
			// If selected key is already occupied, revert to previously selected key.
			if ((listBoxExitKey1.SelectedIndex == listBoxExitKey2.SelectedIndex) ||
				(listBoxExitKey1.SelectedIndex == listBoxExitKey3.SelectedIndex))
				listBoxExitKey1.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyExitKey1];
			SEBSettings.settingsCurrent[SEBSettings.KeyExitKey1] = listBoxExitKey1.SelectedIndex;
		}

		private void listBoxExitKey2_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Make sure that all three exit keys are different.
			// If selected key is already occupied, revert to previously selected key.
			if ((listBoxExitKey2.SelectedIndex == listBoxExitKey1.SelectedIndex) ||
				(listBoxExitKey2.SelectedIndex == listBoxExitKey3.SelectedIndex))
				listBoxExitKey2.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyExitKey2];
			SEBSettings.settingsCurrent[SEBSettings.KeyExitKey2] = listBoxExitKey2.SelectedIndex;
		}

		private void listBoxExitKey3_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Make sure that all three exit keys are different.
			// If selected key is already occupied, revert to previously selected key.
			if ((listBoxExitKey3.SelectedIndex == listBoxExitKey1.SelectedIndex) ||
				(listBoxExitKey3.SelectedIndex == listBoxExitKey2.SelectedIndex))
				listBoxExitKey3.SelectedIndex = (int) SEBSettings.settingsCurrent[SEBSettings.KeyExitKey3];
			SEBSettings.settingsCurrent[SEBSettings.KeyExitKey3] = listBoxExitKey3.SelectedIndex;
		}

		private void buttonAbout_Click(object sender, EventArgs e)
		{

		}

		private void buttonHelp_Click(object sender, EventArgs e)
		{

		}

		private void buttonQuit_Click(object sender, EventArgs e)
		{
			/*
						// If no file has been opened, save the current settings
						// to the default configuration file ("SebStarter.xml/seb")
						if (currentFileSebStarterIni.Equals(""))
						{
							currentFileSebStarter = defaultFileSebStarter;
							currentPathSebStarter = defaultPathSebStarter;
						}

						// Save the configuration file so that nothing gets lost
						SaveConfigurationFile(currentPathSebStarter);
			*/
			// Close the configuration window and exit
			this.Close();
		}



		// *******************
		// Group "Config File"
		// *******************
		private void radioButtonStartingAnExam_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonStartingAnExam.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] = 0;
			else SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] = 1;
			sebConfigPurposeChanged = true;
		}

		private void radioButtonConfiguringAClient_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonConfiguringAClient.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] = 1;
			else SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] = 0;
			sebConfigPurposeChanged = true;
		}

		private void checkBoxAllowPreferencesWindow_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowPreferencesWindow] = checkBoxAllowPreferencesWindow.Checked;
		}

		private void comboBoxCryptoIdentity_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValCryptoIdentity] = comboBoxCryptoIdentity.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValCryptoIdentity] = comboBoxCryptoIdentity.Text;
		}

		private void comboBoxCryptoIdentity_TextUpdate(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValCryptoIdentity] = comboBoxCryptoIdentity.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValCryptoIdentity] = comboBoxCryptoIdentity.Text;
		}

		private void checkBoxUseOldAsymmetricOnlyEncryption_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyUseAsymmetricOnlyEncryption] = checkBoxUseOldAsymmetricOnlyEncryption.Checked;
		}

		private void textBoxSettingsPassword_TextChanged(object sender, EventArgs e)
		{
			ComparePasswords(textBoxSettingsPassword, textBoxConfirmSettingsPassword, ref settingsPasswordFieldsContainHash, labelSettingsPasswordCompare, null);
			// We can store the settings password regardless if the same is entered in the confirm text field, 
			// as saving the .seb file is only allowed when they are same
			//settingsPassword = textBoxSettingsPassword.Text;
		}

		private void textBoxConfirmSettingsPassword_TextChanged(object sender, EventArgs e)
		{
			ComparePasswords(textBoxSettingsPassword, textBoxConfirmSettingsPassword, ref settingsPasswordFieldsContainHash, labelSettingsPasswordCompare, null);
			// We can store the settings password regardless if the same is entered in the confirm text field, 
			// as saving the .seb file is only allowed when they are same
			//settingsPassword = textBoxSettingsPassword.Text;
		}

		/// ----------------------------------------------------------------------------------------
		/// <summary>
		/// Check if settings changed since last saved/opened
		/// </summary>
		/// ---------------------------------------------------------------------------------------- 
		private int checkSettingsChanged()
		{
			int result = 0;
			// Generate current Browser Exam Key
			string currentBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			if (!lastBrowserExamKey.Equals(currentBrowserExamKey) || !lastSettingsPassword.Equals(textBoxSettingsPassword.Text) || lastCryptoIdentityIndex != comboBoxCryptoIdentity.SelectedIndex)
			{
				var messageBoxResult = MessageBox.Show(SEBUIStrings.unsavedChangesQuestion, SEBUIStrings.unsavedChangesTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (messageBoxResult == DialogResult.Yes)
				{
					result = 1;
				}
				if (messageBoxResult == DialogResult.Cancel)
				{
					result = 2;
				}
			}
			return result;
		}

		private void buttonOpenSettings_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Check if settings changed since last saved/opened
			int result = checkSettingsChanged();
			// User selected cancel, abort
			if (result == 2) return;
			// User selected "Save current settings first: yes"
			if (result == 1)
			{
				// Abort if saving settings failed
				if (!saveCurrentSettings()) return;
			}

			// Set the default directory and file name in the File Dialog
			openFileDialogSebConfigFile.InitialDirectory = currentDireSebConfigFile;
			openFileDialogSebConfigFile.FileName = "";
			openFileDialogSebConfigFile.DefaultExt = "seb";
			openFileDialogSebConfigFile.Filter = "SEB Files|*.seb";

			// Get the user inputs in the File Dialog
			DialogResult fileDialogResult = openFileDialogSebConfigFile.ShowDialog();
			String fileName = openFileDialogSebConfigFile.FileName;

			// If the user clicked "Cancel", do nothing
			// If the user clicked "OK"    , read the settings from the configuration file
			if (fileDialogResult.Equals(DialogResult.Cancel)) return;
			if (fileDialogResult.Equals(DialogResult.OK))
			{
				if (!LoadConfigurationFileIntoEditor(fileName))
				{
					MessageBox.Show(SEBUIStrings.openingSettingsFailedMessage, SEBUIStrings.openingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				// Generate Browser Exam Key of this new settings
				lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
				// Save the current settings password so it can be used for comparing later if it changed
				lastSettingsPassword = textBoxSettingsPassword.Text;
				lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
				// Display the new Browser Exam Key in Exam pane
				textBoxBrowserExamKey.Text = lastBrowserExamKey;
				textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
				// Reset the path of the last saved file which is used in case "Edit duplicate" was used
				lastPathSebConfigFile = null;
			}
		}


		public void openSettingsFile(string filePath)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Check if settings changed since last saved/opened
			int result = checkSettingsChanged();
			// User selected cancel, abort
			if (result == 2) return;
			// User selected "Save current settings first: yes"
			// User selected "Save current settings first: yes"
			if (result == 1)
			{
				// Abort if saving settings failed
				if (!saveCurrentSettings()) return;
			}

			if (!LoadConfigurationFileIntoEditor(filePath))
			{
				MessageBox.Show(SEBUIStrings.openingSettingsFailedMessage, SEBUIStrings.openingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			// Generate Browser Exam Key of this new settings
			lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			// Save the current settings password so it can be used for comparing later if it changed
			lastSettingsPassword = textBoxSettingsPassword.Text;
			lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = lastBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
			// Reset the path of the last saved file which is used in case "Edit duplicate" was used
			lastPathSebConfigFile = null;
		}


		private void buttonSaveSettings_Click(object sender, EventArgs e)
		{
			saveCurrentSettings();
		}

		public bool saveCurrentSettings()
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return false;

			StringBuilder sebClientSettingsAppDataBuilder = new StringBuilder(currentDireSebConfigFile).Append(@"\").Append(currentFileSebConfigFile);
			String fileName = sebClientSettingsAppDataBuilder.ToString();

			// Update URL filter rules, also the seb-Browser white/blacklist keys, 
			// which are necessary for compatibility to SEB 2.1.x
			urlFilter.UpdateFilterRules();

			/// Generate Browser Exam Key and its salt, in case settings or the settings password changed
			string newBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			// Save current Browser Exam Key salt in case saving fails
			byte[] currentExamKeySalt = (byte[]) SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt];

			if (!lastBrowserExamKey.Equals(newBrowserExamKey) || !lastSettingsPassword.Equals(textBoxSettingsPassword.Text) || lastCryptoIdentityIndex != comboBoxCryptoIdentity.SelectedIndex)
			{
				// As the exam key changed, we will generate a new salt
				byte[] newExamKeySalt = SEBProtectionController.GenerateBrowserExamKeySalt();
				// Save the new salt
				SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt] = newExamKeySalt;
			}
			if (!SaveConfigurationFileFromEditor(fileName))
			{
				MessageBox.Show(SEBUIStrings.savingSettingsFailedMessage, SEBUIStrings.savingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
				// Restore the old Browser Exam Key salt
				SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt] = currentExamKeySalt;
				return false;
			}

			// Before displaying message check if saved settings are local client settings or the default settings (no client settings saved yet), in other words:
			// Check if not saving local client settings (in %appdata% or %commonappdata%) or if the last saved/opened file isn't the same we're saving now (usually after "Edit Duplicate") or if the flag for being duplicated was set
			if ((!currentPathSebConfigFile.Equals(SEBClientInfo.SebClientSettingsAppDataFile) && !currentPathSebConfigFile.Equals(SEBClientInfo.SebClientSettingsProgramDataFile) && sebConfigPurposeChanged) || currentSebConfigFileWasDuplicated)
			{
				// In this case tell the user what purpose the file was saved for
				string messageFilePurpose = (int) SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] == 0 ? SEBUIStrings.savingSettingsSucceededStartExam : SEBUIStrings.savingSettingsSucceededMessageConfigureClient;
				MessageBox.Show(messageFilePurpose, SEBUIStrings.savingSettingsSucceeded, MessageBoxButtons.OK, MessageBoxIcon.Information);
				currentSebConfigFileWasDuplicated = false;
				sebConfigPurposeChanged = false;
			}

			// Generate the new Browser Exam Key
			lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = lastBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
			// Save the current settings password so it can be used for comparing later if it changed
			lastSettingsPassword = textBoxSettingsPassword.Text;
			// Reset the path of the last saved file which is used in case "Edit duplicate" was used
			lastPathSebConfigFile = null;
			return true;
		}


		private void buttonSaveSettingsAs_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Set the default directory and file name in the File Dialog
			saveFileDialogSebConfigFile.InitialDirectory = currentDireSebConfigFile;
			saveFileDialogSebConfigFile.FileName = currentFileSebConfigFile;

			// Get the user inputs in the File Dialog
			DialogResult fileDialogResult = saveFileDialogSebConfigFile.ShowDialog();
			String fileName = saveFileDialogSebConfigFile.FileName;

			// If the user clicked "Cancel", do nothing
			// If the user clicked "OK"    , write the settings to the configuration file
			if (fileDialogResult.Equals(DialogResult.Cancel)) return;

			// Update URL filter rules, also the seb-Browser white/blacklist keys, 
			// which are necessary for compatibility to SEB 2.1.x
			urlFilter.UpdateFilterRules();

			// Generate Browser Exam Key and its salt, if settings changed
			string newBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			// Save current Browser Exam Key salt in case saving fails
			byte[] currentExamKeySalt = (byte[]) SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt];

			if (!lastBrowserExamKey.Equals(newBrowserExamKey) || !lastSettingsPassword.Equals(textBoxSettingsPassword.Text) || lastCryptoIdentityIndex != comboBoxCryptoIdentity.SelectedIndex)
			{
				// If the exam key changed, then settings changed and we will generate a new salt
				byte[] newExamKeySalt = SEBProtectionController.GenerateBrowserExamKeySalt();
				// Save the new salt
				SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt] = newExamKeySalt;
			}
			if (fileDialogResult.Equals(DialogResult.OK))
			{
				if (!SaveConfigurationFileFromEditor(fileName))
				{
					MessageBox.Show(SEBUIStrings.savingSettingsFailedMessage, SEBUIStrings.savingSettingsFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
					// Restore the old Browser Exam Key salt
					SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt] = currentExamKeySalt;
					return;
				}
				// If currently edited settings should be saved as local client settings (in %appdata% or %commonappdata%), then don't show the config file purpose message
				if (!currentPathSebConfigFile.Equals(SEBClientInfo.SebClientSettingsAppDataFile) && !currentPathSebConfigFile.Equals(SEBClientInfo.SebClientSettingsProgramDataFile))
				{
					// Tell the user what purpose the file was saved for
					string messageFilePurpose = (int) SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] == 0 ? SEBUIStrings.savingSettingsSucceededStartExam : SEBUIStrings.savingSettingsSucceededMessageConfigureClient;
					MessageBox.Show(messageFilePurpose, SEBUIStrings.savingSettingsSucceeded, MessageBoxButtons.OK, MessageBoxIcon.Information);
					currentSebConfigFileWasDuplicated = false;
					sebConfigPurposeChanged = false;
				}

				// Generate the new Browser Exam Key
				lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
				// Display the new Browser Exam Key in Exam pane
				textBoxBrowserExamKey.Text = lastBrowserExamKey;
				textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
				// Save the current settings password so it can be used for comparing later if it changed
				lastSettingsPassword = textBoxSettingsPassword.Text;
				lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
				// Reset the path of the last saved file which is used in case "Edit duplicate" was used
				lastPathSebConfigFile = null;
			}
		}


		private void buttonRevertToDefaultSettings_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Check if settings changed since last saved/opened
			int result = checkSettingsChanged();
			// User selected cancel, abort
			if (result == 2) return;
			// User selected "Save current settings first: yes"
			if (result == 1)
			{
				// Abort if saving settings failed
				if (!saveCurrentSettings()) return;
			}

			settingsPassword = "";
			settingsPasswordFieldsContainHash = false;
			SEBSettings.RestoreDefaultAndCurrentSettings();
			SEBSettings.AddDefaultProhibitedProcesses();

			// Check if currently edited settings are local client settings (in %appdata% or %commonappdata%) or the default settings (no client settings saved yet)
			if (!currentPathSebConfigFile.Equals(SEBClientInfo.SebClientSettingsAppDataFile) && !currentPathSebConfigFile.Equals(SEBClientInfo.SebClientSettingsProgramDataFile) && !currentPathSebConfigFile.Equals(SEBUIStrings.settingsTitleDefaultSettings))
			{
				// If reverting other than local client/default settings, use "starting exam" as config purpose
				SEBSettings.settingsCurrent[SEBSettings.KeySebConfigPurpose] = 0;
			}

			// Update URL filter rules, also the seb-Browser white/blacklist keys, 
			// which are necessary for compatibility to SEB 2.1.x
			urlFilter.UpdateFilterRules();

			UpdateAllWidgetsOfProgram();
			// Generate Browser Exam Key of default settings
			string currentBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			lastSettingsPassword = textBoxSettingsPassword.Text;
			lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = currentBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
		}


		private void buttonRevertToLocalClientSettings_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Check if settings changed since last saved/opened
			int result = checkSettingsChanged();
			// User selected cancel, abort
			if (result == 2) return;
			// User selected "Save current settings first: yes"
			if (result == 1)
			{
				// Abort if saving settings failed
				if (!saveCurrentSettings()) return;
			}

			// Get the path to the local client settings configuration file
			currentDireSebConfigFile = SEBClientInfo.SebClientSettingsAppDataDirectory;
			currentFileSebConfigFile = SEBClientInfo.SEB_CLIENT_CONFIG;
			StringBuilder sebClientSettingsAppDataBuilder = new StringBuilder(currentDireSebConfigFile).Append(currentFileSebConfigFile);
			currentPathSebConfigFile = sebClientSettingsAppDataBuilder.ToString();

			if (!LoadConfigurationFileIntoEditor(currentPathSebConfigFile))
			{
				settingsPassword = "";
				settingsPasswordFieldsContainHash = false;
				SEBSettings.RestoreDefaultAndCurrentSettings();
				SEBSettings.AddDefaultProhibitedProcesses();
				currentPathSebConfigFile = SEBUIStrings.settingsTitleDefaultSettings;
				// Update URL filter rules, also the seb-Browser white/blacklist keys, 
				// which are necessary for compatibility to SEB 2.1.x
				urlFilter.UpdateFilterRules();
				UpdateAllWidgetsOfProgram();
			}
			// Generate Browser Exam Key of this new settings
			lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			lastSettingsPassword = textBoxSettingsPassword.Text;
			lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = lastBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
		}


		private void buttonRevertToLastOpened_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Check if settings changed since last saved/opened
			int result = checkSettingsChanged();
			// User selected cancel, abort
			if (result == 2) return;
			// User selected "Save current settings first: yes"
			if (result == 1)
			{
				// Abort if saving settings failed
				if (!saveCurrentSettings()) return;
			}

			if (!LoadConfigurationFileIntoEditor(String.IsNullOrEmpty(lastPathSebConfigFile) ? currentPathSebConfigFile : lastPathSebConfigFile)) return;
			lastPathSebConfigFile = null;
			// Generate Browser Exam Key of this new settings
			lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			lastSettingsPassword = textBoxSettingsPassword.Text;
			lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = lastBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
		}


		private void buttonEditDuplicate_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Check if settings changed since last saved/opened
			int result = checkSettingsChanged();
			// User selected cancel, abort
			if (result == 2) return;
			// User selected yes: Save current settings first and proceed only, when saving didn't fail
			// User selected "Save current settings first: yes"
			if (result == 1)
			{
				// Abort if saving settings failed
				if (!saveCurrentSettings()) return;
			}

			// Add string " copy" (or " n+1" if the filename already ends with " copy" or " copy n") to the config name filename
			// Get the filename without extension
			string filename = Path.GetFileNameWithoutExtension(currentFileSebConfigFile);
			// Get the extension (should be .seb)
			string extension = Path.GetExtension(currentFileSebConfigFile);
			StringBuilder newFilename = new StringBuilder();
			if (filename.Length == 0)
			{
				newFilename.Append(SEBUIStrings.settingsUntitledFilename);
				extension = ".seb";
			}
			else
			{
				int copyStringPosition = filename.LastIndexOf(SEBUIStrings.settingsDuplicateSuffix);
				if (copyStringPosition == -1)
				{
					newFilename.Append(filename).Append(SEBUIStrings.settingsDuplicateSuffix);
				}
				else
				{
					newFilename.Append(filename.Substring(0, copyStringPosition + SEBUIStrings.settingsDuplicateSuffix.Length));
					string copyNumberString = filename.Substring(copyStringPosition + SEBUIStrings.settingsDuplicateSuffix.Length);
					if (copyNumberString.Length == 0)
					{
						newFilename.Append(" 1");
					}
					else
					{
						int copyNumber = Convert.ToInt16(copyNumberString.Substring(1));
						if (copyNumber == 0)
						{
							newFilename.Append(SEBUIStrings.settingsDuplicateSuffix);
						}
						else
						{
							newFilename.Append(" ").Append((copyNumber + 1).ToString());
						}
					}
				}
			}
			lastPathSebConfigFile = currentPathSebConfigFile;
			currentFileSebConfigFile = newFilename.Append(extension).ToString();

			StringBuilder sebClientSettingsAppDataBuilder = new StringBuilder(currentDireSebConfigFile).Append(@"\").Append(currentFileSebConfigFile);
			currentPathSebConfigFile = sebClientSettingsAppDataBuilder.ToString();
			currentSebConfigFileWasDuplicated = true;
			// Update title of edited settings file
			UpdateAllWidgetsOfProgram();
		}


		private void buttonConfigureClient_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Get the path to the local client settings configuration file
			StringBuilder sebClientSettingsAppDataBuilder = new StringBuilder(SEBClientInfo.SebClientSettingsAppDataDirectory).Append(SEBClientInfo.SEB_CLIENT_CONFIG);
			string filename = sebClientSettingsAppDataBuilder.ToString();

			// Update URL filter rules, also the seb-Browser white/blacklist keys, 
			// which are necessary for compatibility to SEB 2.1.x
			urlFilter.UpdateFilterRules();

			// Generate Browser Exam Key and its salt, if settings changed
			string newBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			// Save current Browser Exam Key salt in case saving fails
			byte[] currentExamKeySalt = (byte[]) SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt];

			if (!lastBrowserExamKey.Equals(newBrowserExamKey) || !lastSettingsPassword.Equals(textBoxSettingsPassword.Text))
			{
				// If the exam key changed, then settings changed and we will generate a new salt
				byte[] newExamKeySalt = SEBProtectionController.GenerateBrowserExamKeySalt();
				// Save the new salt
				SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt] = newExamKeySalt;
			}
			if (!SaveConfigurationFileFromEditor(filename))
			{
				// SebClientSettings.seb config file wasn't saved successfully, revert changed settings
				// Restore the old Browser Exam Key salt
				SEBSettings.settingsCurrent[SEBSettings.KeyExamKeySalt] = currentExamKeySalt;
				return;
			}
			// Generate the new Browser Exam Key
			lastBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			// Save the current settings password so it can be used for comparing later if it changed
			lastSettingsPassword = textBoxSettingsPassword.Text;
			lastCryptoIdentityIndex = comboBoxCryptoIdentity.SelectedIndex;
			// Display the new Browser Exam Key in Exam pane
			textBoxBrowserExamKey.Text = lastBrowserExamKey;
			textBoxConfigurationKey.Text = SEBProtectionController.ComputeConfigurationKey();
		}


		private void buttonApplyAndStartSEB_Click(object sender, EventArgs e)
		{
			// Check if there are unconfirmed passwords, if yes show an alert and abort saving
			if (ArePasswordsUnconfirmed()) return;

			// Update URL filter rules, also the seb-Browser white/blacklist keys, 
			// which are necessary for compatibility to SEB 2.1.x
			urlFilter.UpdateFilterRules();

			// Check if settings changed and save them if yes
			// Generate current Browser Exam Key
			string currentBrowserExamKey = SEBProtectionController.ComputeBrowserExamKey();
			if (!lastBrowserExamKey.Equals(currentBrowserExamKey) || !lastSettingsPassword.Equals(textBoxSettingsPassword.Text) || !String.IsNullOrEmpty(lastPathSebConfigFile))
			{
				if (!saveCurrentSettings()) return;
			}
			// Get the path to the local client settings configuration file
			currentDireSebConfigFile = SEBClientInfo.SebClientSettingsAppDataDirectory;
			currentFileSebConfigFile = SEBClientInfo.SEB_CLIENT_CONFIG;
			StringBuilder sebClientSettingsAppDataBuilder = new StringBuilder(currentDireSebConfigFile).Append(currentFileSebConfigFile);
			string localSebClientSettings = sebClientSettingsAppDataBuilder.ToString();

			StringBuilder sebClientExeBuilder = new StringBuilder(SEBClientInfo.SebClientDirectory).Append(SEBClientInfo.PRODUCT_NAME).Append(".exe");
			string sebClientExe = sebClientExeBuilder.ToString();

			var p = new Process();
			p.StartInfo.FileName = sebClientExe;

			if (!currentPathSebConfigFile.Equals(localSebClientSettings) && !currentPathSebConfigFile.Equals(SEBUIStrings.settingsTitleDefaultSettings))
			{
				p.StartInfo.Arguments = String.Format("\"{0}\"", currentPathSebConfigFile);
			}

			p.Start();

			Application.Exit();
		}


		// **********************
		// Group "User Interface"
		// **********************
		private void radioButtonUseBrowserWindow_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUseBrowserWindow.Checked == true)
			{
				groupBoxMainBrowserWindow.Enabled = true;
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserViewMode] = 0;
				SEBSettings.settingsCurrent[SEBSettings.KeyTouchOptimized] = false;
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserScreenKeyboard] = false;
				checkBoxEnableTouchExit.Enabled = false;
				checkBoxEnableTouchExit.Checked = false;
			}
		}

		private void radioButtonUseFullScreenMode_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUseFullScreenMode.Checked == true)
			{
				groupBoxMainBrowserWindow.Enabled = false;
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserViewMode] = 1;
				SEBSettings.settingsCurrent[SEBSettings.KeyTouchOptimized] = false;
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserScreenKeyboard] = false;
				checkBoxEnableTouchExit.Enabled = false;
				checkBoxEnableTouchExit.Checked = false;
			}
		}

		private void radioButtonTouchOptimized_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonTouchOptimized.Checked == true)
			{
				if ((Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyCreateNewDesktop])
				{
					MessageBox.Show("Touch optimization will not work when kiosk mode is set to 'Create new desktop', please change kiosk mode to 'Disable Explorer Shell' in the Security tab.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				groupBoxMainBrowserWindow.Enabled = false;
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserViewMode] = 1;
				SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkWidth] = "100%";
				SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkHeight] = "100%";
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserViewMode] = 1;

				if ((bool) SEBSettings.settingsCurrent[SEBSettings.KeyTouchOptimized] == false)
				{
					checkBoxEnableTouchExit.Checked = (bool) SEBSettings.settingsCurrent[SEBSettings.KeyEnableTouchExit];
				}
				SEBSettings.settingsCurrent[SEBSettings.KeyTouchOptimized] = true;
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserScreenKeyboard] = true;
				checkBoxEnableTouchExit.Enabled = true;
			}
		}

		private void comboBoxMainBrowserWindowWidth_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.Text;
		}

		private void comboBoxMainBrowserWindowWidth_TextUpdate(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowWidth] = comboBoxMainBrowserWindowWidth.Text;
		}

		private void comboBoxMainBrowserWindowHeight_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.Text;
		}

		private void comboBoxMainBrowserWindowHeight_TextUpdate(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowHeight] = comboBoxMainBrowserWindowHeight.Text;
		}

		private void listBoxMainBrowserWindowPositioning_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowPositioning] = listBoxMainBrowserWindowPositioning.SelectedIndex;
		}

		private void checkBoxEnableBrowserWindowToolbar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableBrowserWindowToolbar] = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxHideBrowserWindowToolbar.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxAllowMainWindowAddressBar.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxAllowAdditionalWindowAddressBar.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
			checkBoxAllowDeveloperConsole.Enabled = checkBoxEnableBrowserWindowToolbar.Checked;
		}

		private void checkBoxHideBrowserWindowToolbar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyHideBrowserWindowToolbar] = checkBoxHideBrowserWindowToolbar.Checked;
		}

		private void checkBoxShowMenuBar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowMenuBar] = checkBoxShowMenuBar.Checked;
		}

		private void checkBoxShowTaskBar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowTaskBar] = checkBoxShowTaskBar.Checked;
			comboBoxTaskBarHeight.Enabled = checkBoxShowTaskBar.Checked;
		}

		private void comboBoxTaskBarHeight_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValTaskBarHeight] = comboBoxTaskBarHeight.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValTaskBarHeight] = comboBoxTaskBarHeight.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyTaskBarHeight] = comboBoxTaskBarHeight.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyTaskBarHeight] = Int32.Parse(comboBoxTaskBarHeight.Text);
		}

		private void comboBoxTaskBarHeight_TextUpdate(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValTaskBarHeight] = comboBoxTaskBarHeight.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValTaskBarHeight] = comboBoxTaskBarHeight.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyTaskBarHeight] = comboBoxTaskBarHeight.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyTaskBarHeight] = Int32.Parse(comboBoxTaskBarHeight.Text);
		}

		private void addDictionaryButton_Click(object sender, EventArgs e)
		{
			var validLocale = TryGetLocale(out string locale);

			if (!validLocale)
			{
				return;
			}

			var validFiles = TryGetFiles(out IEnumerable<string> files);

			if (!validFiles)
			{
				return;
			}

			var saved = TrySaveDictionary(files, locale, out Exception exception);

			if (!saved)
			{
				var message = $"Failed to save dictionary data for locale '{locale}' and files '{String.Join("', '", files)}'!";

				Logger.AddError(message, this, exception);
				MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool TryGetLocale(out string locale)
		{
			locale = string.Empty;

			var dialog = new Form
			{
				FormBorderStyle = FormBorderStyle.FixedDialog,
				Height = 150,
				MaximizeBox = false,
				MinimizeBox = false,
				Owner = this,
				StartPosition = FormStartPosition.CenterParent,
				Text = "Dictionary Locale",
				Width = 450
			};
			var description = new Label
			{
				Height = 40,
				Left = 15,
				Text = @"Please enter the locale for the dictionary you wish to add. The locale must comply with the format ""language-COUNTRY"", e.g. ""en-US"" or ""de-CH"".",
				Top = 10,
				Width = 400
			};
			var textBox = new TextBox { Left = 15, Top = 50, Width = 400 };
			var button = new Button { DialogResult = DialogResult.OK, Enabled = false, Left = 175, Width = 75, Text = "OK", Top = 75, };
			var isValid = false;

			button.Click += (o, args) => dialog.Close();
			textBox.TextChanged += (o, args) =>
			{
				isValid = Regex.IsMatch(textBox.Text, @"^[a-z]{2}-[A-Z]{2}$");
				button.Enabled = isValid;
				textBox.ForeColor = isValid ? Color.Green : Color.Red;
			};
			dialog.Controls.Add(description);
			dialog.Controls.Add(textBox);
			dialog.Controls.Add(button);
			dialog.AcceptButton = button;

			var result = dialog.ShowDialog();

			if (isValid)
			{
				locale = textBox.Text;
			}

			foreach (DataGridViewRow row in spellCheckerDataGridView.Rows)
			{
				if (row.Cells[spellCheckerDictionaryLocaleColumn.Index].Value as string == locale)
				{
					isValid = false;
					MessageBox.Show(this, $"You can only specify one dictionary per locale ({locale})!", "Duplicate Locale", MessageBoxButtons.OK, MessageBoxIcon.Error);

					break;
				}
			}

			return result == DialogResult.OK && isValid;
		}

		private bool TryGetFiles(out IEnumerable<string> files)
		{
			files = Enumerable.Empty<string>();

			var dialog = new OpenFileDialog();
			var isValid = false;

			dialog.Filter = "dictionary files (*.aff, *.dic)|*.aff;*.dic";
			dialog.FilterIndex = 2;
			dialog.Multiselect = true;
			dialog.RestoreDirectory = true;

			var result = dialog.ShowDialog();

			isValid = dialog.FileNames.Count() == 2;
			isValid &= dialog.FileNames.Count(f => f.EndsWith(".aff")) == 1;
			isValid &= dialog.FileNames.Count(f => f.EndsWith(".dic")) == 1;

			if (isValid)
			{
				files = dialog.FileNames;
			}
			else if (result == DialogResult.OK)
			{
				MessageBox.Show(this, "You need to select one .aff and one .dic file!", "Invalid File(s)", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return result == DialogResult.OK && isValid;
		}

		private bool TrySaveDictionary(IEnumerable<string> files, string locale, out Exception exception)
		{
			exception = null;

			try
			{
				var allowedDictionaries = SEBSettings.settingsCurrent[SEBSettings.KeyAllowSpellCheckDictionary] as ListObj;
				var dictionaries = SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalDictionaries] as ListObj;
				var data = ZipAndEncodeDictionaryFiles(files, locale);
				var format = (int) SEBSettings.DictionaryFormat.Mozilla;
				var fileList = ToDictionaryFileList(files);
				var dictionary = new DictObj
				{
					{ SEBSettings.KeyAdditionalDictionaryData, data },
					{ SEBSettings.KeyAdditionalDictionaryFormat, format },
					{ SEBSettings.KeyAdditionalDictionaryLocale, locale }
				};

				dictionaries.Add(dictionary);
				allowedDictionaries.Add(locale);
				spellCheckerDataGridView.Rows.Add(true, locale, fileList);
			}
			catch (Exception e)
			{
				exception = e;
			}

			return exception == null;
		}

		private string ZipAndEncodeDictionaryFiles(IEnumerable<string> files, string locale)
		{
			var compressor = new FileCompressor();
			var tempDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), locale));

			foreach (var file in files)
			{
				var tempFile = Path.Combine(tempDirectory.FullName, Path.GetFileName(file));

				File.Copy(file, tempFile, true);
			}

			var data = compressor.CompressAndEncodeEntireDirectory(tempDirectory.FullName);

			tempDirectory.Delete(true);

			return data;
		}

		private string ToDictionaryFileList(IEnumerable<string> files)
		{
			var affFile = Path.GetFileName(files.First(f => f.EndsWith(".aff")));
			var dicFile = Path.GetFileName(files.First(f => f.EndsWith(".dic")));

			return $@"{affFile}{Environment.NewLine}{dicFile}";
		}

		private void spellCheckerDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.ColumnIndex == spellCheckerDictionaryEnabledColumn.Index)
			{
				var allowedDictionaries = SEBSettings.settingsCurrent[SEBSettings.KeyAllowSpellCheckDictionary] as ListObj;
				var enabled = spellCheckerDataGridView.Rows[e.RowIndex].Cells[spellCheckerDictionaryEnabledColumn.Index].Value is true;
				var locale = spellCheckerDataGridView.Rows[e.RowIndex].Cells[spellCheckerDictionaryLocaleColumn.Index].Value as string;

				if (enabled && allowedDictionaries.All(l => l as string != locale))
				{
					allowedDictionaries.Add(locale);
				}
				else if (!enabled)
				{
					allowedDictionaries.RemoveAll(l => l as string == locale);
				}
			}
		}

		private void spellCheckerDataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (spellCheckerDataGridView.IsCurrentCellDirty && spellCheckerDataGridView.CurrentCell.ColumnIndex == spellCheckerDictionaryEnabledColumn.Index)
			{
				spellCheckerDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void removeDictionaryButton_Click(object sender, EventArgs e)
		{
			var warningSeen = false;

			void ShowDefaultDictionaryInfo()
			{
				if (!warningSeen)
				{
					MessageBox.Show(this, "Default dictionaries cannot be removed, as they are part of the application installation.", "Default Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Information);
					warningSeen = true;
				}
			}

			if (spellCheckerDataGridView.SelectedRows.Count > 0)
			{
				foreach (DataGridViewRow row in spellCheckerDataGridView.SelectedRows)
				{
					if (row.Tag is true)
					{
						ShowDefaultDictionaryInfo();
					}
					else
					{
						RemoveDictionary(row);
					}
				}
			}
			else if (spellCheckerDataGridView.CurrentRow != null)
			{
				if (spellCheckerDataGridView.CurrentRow.Tag is true)
				{
					ShowDefaultDictionaryInfo();
				}
				else
				{
					RemoveDictionary(spellCheckerDataGridView.CurrentRow);
				}
			}
		}

		private void RemoveDictionary(DataGridViewRow row)
		{
			var allowedDictionaries = SEBSettings.settingsCurrent[SEBSettings.KeyAllowSpellCheckDictionary] as ListObj;
			var dictionaries = SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalDictionaries] as ListObj;
			var locale = row.Cells[spellCheckerDictionaryLocaleColumn.Index].Value as string;

			allowedDictionaries.RemoveAll(l => l as string == locale);
			dictionaries.RemoveAll(d => (d as DictObj).TryGetValue(SEBSettings.KeyAdditionalDictionaryLocale, out object l) && l as string == locale);
			spellCheckerDataGridView.Rows.Remove(row);
		}

		// ***************
		// Group "Browser"
		// ***************
		private void listBoxOpenLinksHTML_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkPolicy] = listBoxOpenLinksHTML.SelectedIndex;
		}


		private void checkBoxBlockLinksHTML_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkBlockForeign] = checkBoxBlockLinksHTML.Checked;
		}


		private void comboBoxNewBrowserWindowWidth_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.Text;
		}

		private void comboBoxNewBrowserWindowWidth_TextUpdate(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkWidth] = comboBoxNewBrowserWindowWidth.Text;
		}

		private void comboBoxNewBrowserWindowHeight_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.Text;
		}

		private void comboBoxNewBrowserWindowHeight_TextUpdate(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.Text;
			//SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.SelectedIndex;
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkHeight] = comboBoxNewBrowserWindowHeight.Text;
		}

		private void listBoxNewBrowserWindowPositioning_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowByLinkPositioning] = listBoxNewBrowserWindowPositioning.SelectedIndex;
		}

		private void checkBoxEnablePlugins_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnablePlugIns] = checkBoxEnablePlugIns.Checked;
		}

		private void checkBoxEnableJava_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableJava] = checkBoxEnableJava.Checked;
		}

		private void checkBoxEnableJavaScript_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableJavaScript] = checkBoxEnableJavaScript.Checked;
		}

		private void checkBoxBlockPopUpWindows_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBlockPopUpWindows] = checkBoxBlockPopUpWindows.Checked;
		}

		private void checkBoxAllowVideoCapture_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowVideoCapture] = checkBoxAllowVideoCapture.Checked;
		}

		private void checkBoxAllowAudioCapture_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowAudioCapture] = checkBoxAllowAudioCapture.Checked;
		}

		private void checkBoxAllowBrowsingBackForward_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowBrowsingBackForward] = checkBoxAllowBrowsingBackForward.Checked;
			checkBoxEnableAltMouseWheel.Checked = checkBoxAllowBrowsingBackForward.Checked;
		}

		private void checkBoxAllowReload_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserWindowAllowReload] = checkBoxAllowReload.Checked;
		}

		private void checkBoxShowReloadWarning_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowReloadWarning] = checkBoxShowReloadWarning.Checked;
		}

		private void checkBoxAllowNavigationNewWindow_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowNavigation] = checkBoxAllowNavigationNewWindow.Checked;
		}

		private void checkBoxAllowReloadNewWindow_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowAllowReload] = checkBoxAllowReloadNewWindow.Checked;
		}

		private void checkBoxShowReloadWarningNewWindow_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowShowReloadWarning] = checkBoxShowReloadWarningNewWindow.Checked;
		}

		private void checkBoxRemoveProfile_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyRemoveBrowserProfile] = checkBoxRemoveProfile.Checked;
		}

		private void checkBoxDisableLocalStorage_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyDisableLocalStorage] = checkBoxDisableLocalStorage.Checked;
		}

		// BEWARE: you must invert this value since "Use Without" is "Not Enable"!
		private void checkBoxUseSebWithoutBrowser_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableSebBrowser] = !(checkBoxUseSebWithoutBrowser.Checked);
		}

		private void checkBoxShowReloadButton_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowReloadButton] = checkBoxShowReloadButton.Checked;
		}

		private void radioButtonUseZoomPage_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUseZoomPage.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyZoomMode] = 0;
			else SEBSettings.settingsCurrent[SEBSettings.KeyZoomMode] = 1;
		}

		private void radioButtonUseZoomText_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUseZoomText.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyZoomMode] = 1;
			else SEBSettings.settingsCurrent[SEBSettings.KeyZoomMode] = 0;
		}

		private void radioButtonUserAgentDesktopDefault_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentDesktopDefault.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopMode] = 0;
			//else SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopMode] = 1;
		}

		private void radioButtonUserAgentDesktopCustom_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentDesktopCustom.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopMode] = 1;
			//else SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopMode] = 0;
		}

		private void textBoxUserAgent_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgent] = textBoxUserAgent.Text;
		}

		private void textBoxUserAgentTouchModeIPad_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchModeIPad] = textBoxUserAgentTouchModeIPad.Text;
		}

		private void textBoxUserAgentDesktopModeCustom_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentDesktopModeCustom] = textBoxUserAgentDesktopModeCustom.Text;
			radioButtonUserAgentDesktopCustom.Checked = true;
		}

		private void radioButtonUserAgentTouchDefault_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentTouchDefault.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] = 0;
			//else SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] = 1;
		}

		private void radioButtonUserAgentTouchIPad_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentTouchIPad.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] = 1;
		}

		private void radioButtonUserAgentTouchCustom_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentTouchCustom.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] = 2;
			//else SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchMode] = 0;
		}

		private void textBoxUserAgentTouchModeCustom_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentTouchModeCustom] = textBoxUserAgentTouchModeCustom.Text;
			radioButtonUserAgentTouchCustom.Checked = true;
		}


		private void radioButtonUserAgentMacDefault_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentMacDefault.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentMac] = 0;
		}

		private void radioButtonUserAgentMacCustom_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUserAgentMacCustom.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentMac] = 1;
		}

		private void textBoxUserAgentMacCustom_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserUserAgentMacCustom] = textBoxUserAgentMacCustom.Text;
			radioButtonUserAgentMacCustom.Checked = true;
		}


		// ********************
		// Group "Down/Uploads"
		// ********************
		private void checkBoxAllowDownUploads_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowDownUploads] = checkBoxAllowDownUploads.Checked;
		}

		private void buttonDownloadDirectoryWin_Click(object sender, EventArgs e)
		{
			// Set the default directory in the Folder Browser Dialog
			folderBrowserDialogDownloadDirectoryWin.RootFolder = Environment.SpecialFolder.DesktopDirectory;
			//          folderBrowserDialogDownloadDirectoryWin.RootFolder = Environment.CurrentDirectory;

			// Get the user inputs in the File Dialog
			DialogResult dialogResult = folderBrowserDialogDownloadDirectoryWin.ShowDialog();
			String path = folderBrowserDialogDownloadDirectoryWin.SelectedPath;

			// If the user clicked "Cancel", do nothing
			if (dialogResult.Equals(DialogResult.Cancel)) return;

			// If the user clicked "OK", ...
			string pathUsingEnvironmentVariables = SEBClientInfo.ContractEnvironmentVariables(path);
			SEBSettings.settingsCurrent[SEBSettings.KeyDownloadDirectoryWin] = pathUsingEnvironmentVariables;
			textBoxDownloadDirectoryWin.Text = pathUsingEnvironmentVariables;
		}

		private void textBoxDownloadDirectoryWin_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyDownloadDirectoryWin] = textBoxDownloadDirectoryWin.Text;
		}

		private void textBoxDownloadDirectoryOSX_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyDownloadDirectoryOSX] = textBoxDownloadDirectoryOSX.Text;
		}

		private void checkBoxDownloadOpenSEBFiles_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyDownloadAndOpenSebConfig] = checkBoxDownloadOpenSEBFiles.Checked;
		}

		private void checkBoxOpenDownloads_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyOpenDownloads] = checkBoxOpenDownloads.Checked;
		}

		private void listBoxChooseFileToUploadPolicy_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyChooseFileToUploadPolicy] = listBoxChooseFileToUploadPolicy.SelectedIndex;
		}

		private void checkBoxDownloadPDFFiles_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyDownloadPDFFiles] = checkBoxDownloadPDFFiles.Checked;
		}

		private void checkBoxAllowPDFPlugIn_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowPDFPlugIn] = checkBoxAllowPDFPlugIn.Checked;
		}


		// ************
		// Group "Exam"
		// ************

		private void textBoxBrowserExamKey_TextChanged(object sender, EventArgs e)
		{
		}

		private void checkBoxSendBrowserExamKey_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeySendBrowserExamKey] = checkBoxSendBrowserExamKey.Checked;
			textBoxBrowserExamKey.Enabled = checkBoxSendBrowserExamKey.Checked;
			textBoxConfigurationKey.Enabled = checkBoxSendBrowserExamKey.Checked;
		}

		private void textBoxQuitURL_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyQuitURL] = textBoxQuitURL.Text;
		}

		private void checkBoxQuitURLConfirm_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyQuitURLConfirm] = checkBoxQuitURLConfirm.Checked;
		}

		private void checkBoxUseStartURL_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamUseStartURL] = checkBoxUseStartURL.Checked;
			textBoxRestartExamLink.Enabled = !checkBoxUseStartURL.Checked;
		}

		private void textBoxRestartExamLink_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamURL] = textBoxRestartExamLink.Text;
		}

		private void textBoxRestartExamText_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamText] = textBoxRestartExamText.Text;
		}

		private void checkBoxRestartExamPasswordProtected_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyRestartExamPasswordProtected] = checkBoxRestartExamPasswordProtected.Checked;
		}



		// ********************
		// Group "Applications"
		// ********************
		private void checkBoxMonitorProcesses_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyMonitorProcesses] = checkBoxMonitorProcesses.Checked;
		}


		// ******************************************
		// Group "Applications - Permitted Processes"
		// ******************************************
		private void checkBoxAllowSwitchToApplications_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowSwitchToApplications] = checkBoxAllowSwitchToApplications.Checked;
			checkBoxAllowFlashFullscreen.Enabled = checkBoxAllowSwitchToApplications.Checked;
		}

		private void checkBoxAllowFlashFullscreen_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowFlashFullscreen] = checkBoxAllowFlashFullscreen.Checked;
		}


		private void LoadAndUpdatePermittedSelectedProcessGroup(int selectedProcessIndex)
		{
			// Get the process data of the selected process
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[selectedProcessIndex];
			SEBSettings.permittedArgumentList = (ListObj) SEBSettings.permittedProcessData[SEBSettings.KeyArguments];

			// Beware double events:
			// Update the widgets in "Selected Process" group,
			// but prevent the following "widget changed" event from firing the "cell changed" event once more!
			ignoreWidgetEventPermittedProcessesActive = true;
			ignoreWidgetEventPermittedProcessesOS = true;
			ignoreWidgetEventPermittedProcessesExecutable = true;
			ignoreWidgetEventPermittedProcessesTitle = true;

			// Update the widgets in the "Selected Process" group
			checkBoxPermittedProcessActive.Checked = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyActive];
			checkBoxPermittedProcessAutostart.Checked = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyAutostart];
			checkBoxPermittedProcessIconInTaskbar.Checked = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyIconInTaskbar];
			checkBoxPermittedProcessAutohide.Checked = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyRunInBackground];
			checkBoxPermittedProcessIconInTaskbar.Enabled = !checkBoxPermittedProcessAutohide.Checked | checkBoxPermittedProcessAutostart.Checked;
			checkBoxPermittedProcessAllowUser.Checked = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyAllowUser];
			checkBoxPermittedProcessStrongKill.Checked = (Boolean) SEBSettings.permittedProcessData[SEBSettings.KeyStrongKill];
			listBoxPermittedProcessOS.SelectedIndex = (Int32) SEBSettings.permittedProcessData[SEBSettings.KeyOS];
			textBoxPermittedProcessTitle.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyTitle];
			textBoxPermittedProcessDescription.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyDescription];
			textBoxPermittedProcessExecutable.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyExecutable];
			textBoxPermittedProcessOriginalName.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyOriginalName];
			textBoxPermittedProcessExecutables.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyWindowHandlingProcess];
			textBoxPermittedProcessPath.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyPath];
			textBoxPermittedProcessIdentifier.Text = (String) SEBSettings.permittedProcessData[SEBSettings.KeyIdentifier];

			// Reset the ignore widget event flags
			ignoreWidgetEventPermittedProcessesActive = false;
			ignoreWidgetEventPermittedProcessesOS = false;
			ignoreWidgetEventPermittedProcessesExecutable = false;
			ignoreWidgetEventPermittedProcessesTitle = false;

			// Check if selected process has any arguments
			if (SEBSettings.permittedArgumentList.Count > 0)
				SEBSettings.permittedArgumentIndex = 0;
			else SEBSettings.permittedArgumentIndex = -1;

			// Remove all previously displayed arguments from DataGridView
			dataGridViewPermittedProcessArguments.Enabled = (SEBSettings.permittedArgumentList.Count > 0);
			dataGridViewPermittedProcessArguments.Rows.Clear();

			// Add arguments of selected process to DataGridView
			for (int index = 0; index < SEBSettings.permittedArgumentList.Count; index++)
			{
				SEBSettings.permittedArgumentData = (DictObj) SEBSettings.permittedArgumentList[index];
				Boolean active = (Boolean) SEBSettings.permittedArgumentData[SEBSettings.KeyActive];
				String argument = (String) SEBSettings.permittedArgumentData[SEBSettings.KeyArgument];
				dataGridViewPermittedProcessArguments.Rows.Add(active, argument);
			}

			// Get the selected argument data
			if (SEBSettings.permittedArgumentList.Count > 0)
				SEBSettings.permittedArgumentData = (DictObj) SEBSettings.permittedArgumentList[SEBSettings.permittedArgumentIndex];
		}


		private void ClearPermittedSelectedProcessGroup()
		{
			// Beware double events:
			// Update the widgets in "Selected Process" group,
			// but prevent the following "widget changed" event from firing the "cell changed" event once more!
			ignoreWidgetEventPermittedProcessesActive = true;
			ignoreWidgetEventPermittedProcessesOS = true;
			ignoreWidgetEventPermittedProcessesExecutable = true;
			ignoreWidgetEventPermittedProcessesTitle = true;

			// Clear the widgets in the "Selected Process" group
			checkBoxPermittedProcessActive.Checked = true;
			checkBoxPermittedProcessAutostart.Checked = true;
			checkBoxPermittedProcessAutohide.Checked = true;
			checkBoxPermittedProcessAllowUser.Checked = true;
			checkBoxPermittedProcessStrongKill.Checked = false;
			listBoxPermittedProcessOS.SelectedIndex = IntWin;
			textBoxPermittedProcessTitle.Text = "";
			textBoxPermittedProcessDescription.Text = "";
			textBoxPermittedProcessExecutable.Text = "";
			textBoxPermittedProcessOriginalName.Text = "";
			textBoxPermittedProcessExecutables.Text = "";
			textBoxPermittedProcessPath.Text = "";
			textBoxPermittedProcessIdentifier.Text = "";

			// Reset the ignore widget event flags
			ignoreWidgetEventPermittedProcessesActive = false;
			ignoreWidgetEventPermittedProcessesOS = false;
			ignoreWidgetEventPermittedProcessesExecutable = false;
			ignoreWidgetEventPermittedProcessesTitle = false;

			// Remove all previously displayed arguments from DataGridView
			dataGridViewPermittedProcessArguments.Enabled = false;
			dataGridViewPermittedProcessArguments.Rows.Clear();
		}


		private void dataGridViewPermittedProcesses_SelectionChanged(object sender, EventArgs e)
		{
			// CAUTION:
			// If a row was previously selected and the user clicks onto another row,
			// the SelectionChanged() event is fired TWICE!!!
			// The first time, it is only for UNselecting the old row,
			// so the SelectedRows.Count is ZERO, so ignore this event handler!
			// The second time, SelectedRows.Count is ONE.
			// Now you can set the widgets in the "Selected Process" groupBox.

			if (dataGridViewPermittedProcesses.SelectedRows.Count != 1) return;
			SEBSettings.permittedProcessIndex = dataGridViewPermittedProcesses.SelectedRows[0].Index;

			// The process list should contain at least one element here:
			// SEBSettings.permittedProcessList.Count >  0
			// SEBSettings.permittedProcessIndex      >= 0
			LoadAndUpdatePermittedSelectedProcessGroup(SEBSettings.permittedProcessIndex);
		}


		private void dataGridViewPermittedProcesses_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// When a CheckBox/ListBox/TextBox entry of a DataGridView table cell is edited,
			// immediately call the CellValueChanged() event,
			// which will update the SelectedProcess data and widgets.
			if (dataGridViewPermittedProcesses.IsCurrentCellDirty)
				dataGridViewPermittedProcesses.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}


		private void dataGridViewPermittedProcesses_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreCellEventPermittedProcessesActive == true) return;
			if (ignoreCellEventPermittedProcessesOS == true) return;
			if (ignoreCellEventPermittedProcessesExecutable == true) return;
			if (ignoreCellEventPermittedProcessesTitle == true) return;

			// Get the current cell where the user has changed a value
			int row = dataGridViewPermittedProcesses.CurrentCellAddress.Y;
			int column = dataGridViewPermittedProcesses.CurrentCellAddress.X;

			// At the beginning, row = -1 and column = -1, so skip this event
			if (row < 0) return;
			if (column < 0) return;

			// Get the changed value of the current cell
			object value = dataGridViewPermittedProcesses.CurrentCell.EditedFormattedValue;

			// Convert the selected "OS" ListBox entry from String to Integer
			if (column == IntColumnProcessOS)
			{
				if ((String) value == StringOSX) value = IntOSX;
				else if ((String) value == StringWin) value = IntWin;
			}

			// Get the process data of the process belonging to the current row
			SEBSettings.permittedProcessIndex = row;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];

			// Update the process data belonging to the current cell
			if (column == IntColumnProcessActive) SEBSettings.permittedProcessData[SEBSettings.KeyActive] = (Boolean) value;
			if (column == IntColumnProcessOS) SEBSettings.permittedProcessData[SEBSettings.KeyOS] = (Int32) value;
			if (column == IntColumnProcessExecutable) SEBSettings.permittedProcessData[SEBSettings.KeyExecutable] = (String) value;
			if (column == IntColumnProcessTitle) SEBSettings.permittedProcessData[SEBSettings.KeyTitle] = (String) value;

			// Beware double events:
			// when a cell is being edited by the user, update its corresponding widget in "Selected Process" group,
			// but prevent the following "widget changed" event from firing the "cell changed" event once more!
			if (column == IntColumnProcessActive) ignoreWidgetEventPermittedProcessesActive = true;
			if (column == IntColumnProcessOS) ignoreWidgetEventPermittedProcessesOS = true;
			if (column == IntColumnProcessExecutable) ignoreWidgetEventPermittedProcessesExecutable = true;
			if (column == IntColumnProcessTitle) ignoreWidgetEventPermittedProcessesTitle = true;

			// In "Selected Process" group: update the widget belonging to the current cell
			// (this will fire the corresponding "widget changed" event).
			if (column == IntColumnProcessActive) checkBoxPermittedProcessActive.Checked = (Boolean) value;
			if (column == IntColumnProcessOS) listBoxPermittedProcessOS.SelectedIndex = (Int32) value;
			if (column == IntColumnProcessExecutable) textBoxPermittedProcessExecutable.Text = (String) value;
			if (column == IntColumnProcessTitle) textBoxPermittedProcessTitle.Text = (String) value;

			// Reset the ignore widget event flags
			if (column == IntColumnProcessActive) ignoreWidgetEventPermittedProcessesActive = false;
			if (column == IntColumnProcessOS) ignoreWidgetEventPermittedProcessesOS = false;
			if (column == IntColumnProcessExecutable) ignoreWidgetEventPermittedProcessesExecutable = false;
			if (column == IntColumnProcessTitle) ignoreWidgetEventPermittedProcessesTitle = false;
		}


		private void buttonAddPermittedProcess_Click(object sender, EventArgs e)
		{
			// Get the process list
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];

			if (SEBSettings.permittedProcessList.Count > 0)
			{
				if (dataGridViewPermittedProcesses.SelectedRows.Count != 1) return;
				//SEBSettings.permittedProcessIndex = dataGridViewPermittedProcesses.SelectedRows[0].Index;
				SEBSettings.permittedProcessIndex = SEBSettings.permittedProcessList.Count;
			}
			else
			{
				// If process list was empty before, enable it
				SEBSettings.permittedProcessIndex = 0;
				dataGridViewPermittedProcesses.Enabled = true;
				groupBoxPermittedProcess.Enabled = true;
			}

			// Create new process dataset containing default values
			DictObj processData = new DictObj();

			processData[SEBSettings.KeyActive] = true;
			processData[SEBSettings.KeyAutostart] = false;
			processData[SEBSettings.KeyIconInTaskbar] = true;
			processData[SEBSettings.KeyRunInBackground] = false;
			processData[SEBSettings.KeyAllowUser] = false;
			processData[SEBSettings.KeyStrongKill] = false;
			processData[SEBSettings.KeyOS] = IntWin;
			processData[SEBSettings.KeyTitle] = "";
			processData[SEBSettings.KeyDescription] = "";
			processData[SEBSettings.KeyExecutable] = "";
			processData[SEBSettings.KeyOriginalName] = "";
			processData[SEBSettings.KeyWindowHandlingProcess] = "";
			processData[SEBSettings.KeyPath] = "";
			processData[SEBSettings.KeyIdentifier] = "";
			processData[SEBSettings.KeyArguments] = new ListObj();

			// Insert new process into process list at position index
			SEBSettings.permittedProcessList.Insert(SEBSettings.permittedProcessIndex, processData);
			dataGridViewPermittedProcesses.Rows.Insert(SEBSettings.permittedProcessIndex, true, StringOS[IntWin], "", "");
			dataGridViewPermittedProcesses.Rows[SEBSettings.permittedProcessIndex].Selected = true;
		}


		private void buttonRemovePermittedProcess_Click(object sender, EventArgs e)
		{
			if (dataGridViewPermittedProcesses.SelectedRows.Count != 1) return;

			// Clear the widgets in the "Selected Process" group
			ClearPermittedSelectedProcessGroup();

			// Delete process from process list at position index
			SEBSettings.permittedProcessIndex = dataGridViewPermittedProcesses.SelectedRows[0].Index;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessList.RemoveAt(SEBSettings.permittedProcessIndex);
			dataGridViewPermittedProcesses.Rows.RemoveAt(SEBSettings.permittedProcessIndex);

			if (SEBSettings.permittedProcessIndex == SEBSettings.permittedProcessList.Count)
				SEBSettings.permittedProcessIndex--;

			if (SEBSettings.permittedProcessList.Count > 0)
			{
				dataGridViewPermittedProcesses.Rows[SEBSettings.permittedProcessIndex].Selected = true;
			}
			else
			{
				// If process list is now empty, disable it
				SEBSettings.permittedProcessIndex = -1;
				dataGridViewPermittedProcesses.Enabled = false;
				groupBoxPermittedProcess.Enabled = false;
			}
		}


		private void buttonChoosePermittedApplication_Click(object sender, EventArgs e)
		{
			var permittedApplicationInformation = ChooseApplicationDialog();
			if (permittedApplicationInformation != null)
			{
				buttonAddPermittedProcess_Click(this, EventArgs.Empty);
				textBoxPermittedProcessExecutable.Text = permittedApplicationInformation.Executable;
				textBoxPermittedProcessOriginalName.Text = permittedApplicationInformation.OriginalName;
				textBoxPermittedProcessTitle.Text = permittedApplicationInformation.Title;
				textBoxPermittedProcessPath.Text = permittedApplicationInformation.Path;
			}
		}

		private void ButtonChooseExecutable_Click(object sender, EventArgs e)
		{
			var permittedApplicationInformation = ChooseApplicationDialog();
			if (permittedApplicationInformation != null)
			{
				textBoxPermittedProcessExecutable.Text = permittedApplicationInformation.Executable;
				textBoxPermittedProcessOriginalName.Text = permittedApplicationInformation.OriginalName;
				textBoxPermittedProcessTitle.Text = permittedApplicationInformation.Title;
				textBoxPermittedProcessPath.Text = permittedApplicationInformation.Path;
			}
		}

		private PermittedApplicationInformation ChooseApplicationDialog()
		{
			var permittedApplicationInformation = new PermittedApplicationInformation();

			var fileDialog = new OpenFileDialog
			{
				InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory),
				Multiselect = false
			};
			var res = fileDialog.ShowDialog();

			if (res == DialogResult.OK)
			{
				var filename = fileDialog.FileName.ToLower();
				permittedApplicationInformation.Title = Path.GetFileNameWithoutExtension(fileDialog.FileName);
				permittedApplicationInformation.Executable = Path.GetFileName(filename);

				var filePath = Path.GetDirectoryName(fileDialog.FileName);
				if (filePath == null)
				{
					return null;
				}
				filePath = filePath.ToLower();

				//Check SebWindo2wsClientForm.GetApplicationPath() for how SEB searches the locations
				//Check if Path to the executable is in Registry - SEB gets the path from there if it exists
				using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, ""))
				{
					string subKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + Path.GetFileName(fileDialog.FileName);
					using (Microsoft.Win32.RegistryKey subkey = key.OpenSubKey(subKeyName))
					{
						if (subkey != null)
						{
							object path = subkey.GetValue("Path");
							if (path != null)
							{
								filePath = filePath.Replace(path.ToString().ToLower(), "");
								filePath = filePath.Replace(path.ToString().TrimEnd('\\').ToLower(), "");
							}
						}
					}
				}

				//Replace all the seach locations - SEB looks in all these directories
				filePath = filePath
					.Replace(SEBClientInfo.ProgramFilesX86Directory.ToLower() + "\\", "")
					.Replace(SEBClientInfo.ProgramFilesX86Directory.ToLower(), "")
					.Replace(Environment.SystemDirectory.ToLower() + "\\", "")
					.Replace(Environment.SystemDirectory.ToLower(), "");

				permittedApplicationInformation.Path = filePath;
				permittedApplicationInformation.OriginalName = FileVersionInfo.GetVersionInfo(filename).OriginalFilename;

				return permittedApplicationInformation;
			}
			return null;
		}

		private void buttonChoosePermittedProcess_Click(object sender, EventArgs e)
		{

		}


		private void checkBoxPermittedProcessActive_CheckedChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventPermittedProcessesActive == true) return;
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyActive] = checkBoxPermittedProcessActive.Checked;
			Boolean active = checkBoxPermittedProcessActive.Checked;
			ignoreCellEventPermittedProcessesActive = true;
			dataGridViewPermittedProcesses.Rows[SEBSettings.permittedProcessIndex].Cells[IntColumnProcessActive].Value = active.ToString();
			ignoreCellEventPermittedProcessesActive = false;
		}


		private void checkBoxPermittedProcessAutostart_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyAutostart] = checkBoxPermittedProcessAutostart.Checked;
			checkBoxPermittedProcessIconInTaskbar.Enabled = !checkBoxPermittedProcessAutohide.Checked | checkBoxPermittedProcessAutostart.Checked;
		}

		private void checkBoxPermittedProcessIconInTaskbar_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyIconInTaskbar] = checkBoxPermittedProcessIconInTaskbar.Checked;
		}

		private void checkBoxPermittedProcessAutohide_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyRunInBackground] = checkBoxPermittedProcessAutohide.Checked;
			checkBoxPermittedProcessIconInTaskbar.Enabled = !checkBoxPermittedProcessAutohide.Checked | checkBoxPermittedProcessAutostart.Checked;
		}

		private void checkBoxPermittedProcessAllowUser_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyAllowUser] = checkBoxPermittedProcessAllowUser.Checked;
		}

		private void checkBoxPermittedProcessStrongKill_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyStrongKill] = checkBoxPermittedProcessStrongKill.Checked;
		}


		private void listBoxPermittedProcessOS_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventPermittedProcessesOS == true) return;
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyOS] = listBoxPermittedProcessOS.SelectedIndex;
			Int32 os = listBoxPermittedProcessOS.SelectedIndex;
			ignoreCellEventPermittedProcessesOS = true;
			dataGridViewPermittedProcesses.Rows[SEBSettings.permittedProcessIndex].Cells[IntColumnProcessOS].Value = StringOS[os];
			ignoreCellEventPermittedProcessesOS = false;
		}


		private void textBoxPermittedProcessTitle_TextChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventPermittedProcessesTitle == true) return;
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyTitle] = textBoxPermittedProcessTitle.Text;
			String title = textBoxPermittedProcessTitle.Text;
			ignoreCellEventPermittedProcessesTitle = true;
			dataGridViewPermittedProcesses.Rows[SEBSettings.permittedProcessIndex].Cells[IntColumnProcessTitle].Value = title;
			ignoreCellEventPermittedProcessesTitle = false;
		}


		private void textBoxPermittedProcessDescription_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyDescription] = textBoxPermittedProcessDescription.Text;
		}


		private void textBoxPermittedProcessExecutable_TextChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventPermittedProcessesExecutable == true) return;
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyExecutable] = textBoxPermittedProcessExecutable.Text;
			String executable = textBoxPermittedProcessExecutable.Text;
			ignoreCellEventPermittedProcessesExecutable = true;
			dataGridViewPermittedProcesses.Rows[SEBSettings.permittedProcessIndex].Cells[IntColumnProcessExecutable].Value = executable;
			ignoreCellEventPermittedProcessesExecutable = false;
		}

		private void textBoxPermittedProcessOriginalName_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyOriginalName] = textBoxPermittedProcessOriginalName.Text;
		}

		private void textBoxPermittedProcessPath_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyPath] = textBoxPermittedProcessPath.Text;
		}

		private void textBoxPermittedProcessIdentifier_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyIdentifier] = textBoxPermittedProcessIdentifier.Text;
		}

		private void textBoxPermittedProcessExecutables_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.permittedProcessIndex < 0) return;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedProcessData[SEBSettings.KeyWindowHandlingProcess] = textBoxPermittedProcessExecutables.Text;
		}

		private void buttonPermittedProcessCodeSignature_Click(object sender, EventArgs e)
		{

		}


		private void dataGridViewPermittedProcessArguments_SelectionChanged(object sender, EventArgs e)
		{
			// CAUTION:
			// If a row was previously selected and the user clicks onto another row,
			// the SelectionChanged() event is fired TWICE!!!
			// The first time, it is only for UNselecting the old row,
			// so the SelectedRows.Count is ZERO, so ignore this event handler!
			// The second time, SelectedRows.Count is ONE.

			if (dataGridViewPermittedProcessArguments.SelectedRows.Count != 1) return;

			// Get the argument data of the selected argument
			SEBSettings.permittedArgumentIndex = dataGridViewPermittedProcessArguments.SelectedRows[0].Index;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedArgumentList = (ListObj) SEBSettings.permittedProcessData[SEBSettings.KeyArguments];
			SEBSettings.permittedArgumentData = (DictObj) SEBSettings.permittedArgumentList[SEBSettings.permittedArgumentIndex];
		}


		private void dataGridViewPermittedProcessArguments_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// When a CheckBox/ListBox/TextBox entry of a DataGridView table cell is edited,
			// immediately call the CellValueChanged() event,
			// which will update the SelectedProcess data and widgets.
			if (dataGridViewPermittedProcessArguments.IsCurrentCellDirty)
				dataGridViewPermittedProcessArguments.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}


		private void dataGridViewPermittedProcessArguments_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Get the current cell where the user has changed a value
			int row = dataGridViewPermittedProcessArguments.CurrentCellAddress.Y;
			int column = dataGridViewPermittedProcessArguments.CurrentCellAddress.X;

			// At the beginning, row = -1 and column = -1, so skip this event
			if (row < 0) return;
			if (column < 0) return;

			// Get the changed value of the current cell
			object value = dataGridViewPermittedProcessArguments.CurrentCell.EditedFormattedValue;

			// Get the argument data of the argument belonging to the cell (row)
			SEBSettings.permittedArgumentIndex = row;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedArgumentList = (ListObj) SEBSettings.permittedProcessData[SEBSettings.KeyArguments];
			SEBSettings.permittedArgumentData = (DictObj) SEBSettings.permittedArgumentList[SEBSettings.permittedArgumentIndex];

			// Update the argument data belonging to the current cell
			if (column == IntColumnProcessActive) SEBSettings.permittedArgumentData[SEBSettings.KeyActive] = (Boolean) value;
			if (column == IntColumnProcessArgument) SEBSettings.permittedArgumentData[SEBSettings.KeyArgument] = (String) value;
		}


		private void buttonPermittedProcessAddArgument_Click(object sender, EventArgs e)
		{
			// Get the permitted argument list
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedArgumentList = (ListObj) SEBSettings.permittedProcessData[SEBSettings.KeyArguments];

			if (SEBSettings.permittedArgumentList.Count > 0)
			{
				if (dataGridViewPermittedProcessArguments.SelectedRows.Count != 1) return;
				//SEBSettings.permittedArgumentIndex = dataGridViewPermittedProcessArguments.SelectedRows[0].Index;
				SEBSettings.permittedArgumentIndex = SEBSettings.permittedArgumentList.Count;
			}
			else
			{
				// If argument list was empty before, enable it
				SEBSettings.permittedArgumentIndex = 0;
				dataGridViewPermittedProcessArguments.Enabled = true;
			}

			// Create new argument dataset containing default values
			DictObj argumentData = new DictObj();

			argumentData[SEBSettings.KeyActive] = true;
			argumentData[SEBSettings.KeyArgument] = "";

			// Insert new argument into argument list at position SEBSettings.permittedArgumentIndex
			SEBSettings.permittedArgumentList.Insert(SEBSettings.permittedArgumentIndex, argumentData);
			dataGridViewPermittedProcessArguments.Rows.Insert(SEBSettings.permittedArgumentIndex, true, "");
			dataGridViewPermittedProcessArguments.Rows[SEBSettings.permittedArgumentIndex].Selected = true;
		}


		private void buttonPermittedProcessRemoveArgument_Click(object sender, EventArgs e)
		{
			if (dataGridViewPermittedProcessArguments.SelectedRows.Count != 1) return;

			// Get the permitted argument list
			SEBSettings.permittedArgumentIndex = dataGridViewPermittedProcessArguments.SelectedRows[0].Index;
			SEBSettings.permittedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses];
			SEBSettings.permittedProcessData = (DictObj) SEBSettings.permittedProcessList[SEBSettings.permittedProcessIndex];
			SEBSettings.permittedArgumentList = (ListObj) SEBSettings.permittedProcessData[SEBSettings.KeyArguments];

			// Delete argument from argument list at position SEBSettings.permittedArgumentIndex
			SEBSettings.permittedArgumentList.RemoveAt(SEBSettings.permittedArgumentIndex);
			dataGridViewPermittedProcessArguments.Rows.RemoveAt(SEBSettings.permittedArgumentIndex);

			if (SEBSettings.permittedArgumentIndex == SEBSettings.permittedArgumentList.Count)
				SEBSettings.permittedArgumentIndex--;

			if (SEBSettings.permittedArgumentList.Count > 0)
			{
				dataGridViewPermittedProcessArguments.Rows[SEBSettings.permittedArgumentIndex].Selected = true;
			}
			else
			{
				// If argument list is now empty, disable it
				SEBSettings.permittedArgumentIndex = -1;
				//SEBSettings.permittedArgumentList.Clear();
				//SEBSettings.permittedArgumentData.Clear();
				dataGridViewPermittedProcessArguments.Enabled = false;
			}
		}



		// *******************************************
		// Group "Applications - Prohibited Processes"
		// *******************************************
		private void LoadAndUpdateProhibitedSelectedProcessGroup(int selectedProcessIndex)
		{
			// Get the process data of the selected process
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[selectedProcessIndex];

			// Beware double events:
			// Update the widgets in "Selected Process" group,
			// but prevent the following "widget changed" event from firing the "cell changed" event once more!
			ignoreWidgetEventProhibitedProcessesActive = true;
			ignoreWidgetEventProhibitedProcessesOS = true;
			ignoreWidgetEventProhibitedProcessesExecutable = true;
			ignoreWidgetEventProhibitedProcessesDescription = true;

			// Update the widgets in the "Selected Process" group
			checkBoxProhibitedProcessActive.Checked = (Boolean) SEBSettings.prohibitedProcessData[SEBSettings.KeyActive];
			checkBoxProhibitedProcessCurrentUser.Checked = (Boolean) SEBSettings.prohibitedProcessData[SEBSettings.KeyCurrentUser];
			checkBoxProhibitedProcessStrongKill.Checked = (Boolean) SEBSettings.prohibitedProcessData[SEBSettings.KeyStrongKill];
			listBoxProhibitedProcessOS.SelectedIndex = (Int32) SEBSettings.prohibitedProcessData[SEBSettings.KeyOS];
			textBoxProhibitedProcessExecutable.Text = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyExecutable];
			textBoxProhibitedProcessOriginalName.Text = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyOriginalName];
			textBoxProhibitedProcessDescription.Text = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyDescription];
			textBoxProhibitedProcessIdentifier.Text = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyIdentifier];
			textBoxProhibitedProcessUser.Text = (String) SEBSettings.prohibitedProcessData[SEBSettings.KeyUser];

			// Reset the ignore widget event flags
			ignoreWidgetEventProhibitedProcessesActive = false;
			ignoreWidgetEventProhibitedProcessesOS = false;
			ignoreWidgetEventProhibitedProcessesExecutable = false;
			ignoreWidgetEventProhibitedProcessesDescription = false;
		}


		private void ClearProhibitedSelectedProcessGroup()
		{
			// Beware double events:
			// Update the widgets in "Selected Process" group,
			// but prevent the following "widget changed" event from firing the "cell changed" event once more!
			ignoreWidgetEventProhibitedProcessesActive = true;
			ignoreWidgetEventProhibitedProcessesOS = true;
			ignoreWidgetEventProhibitedProcessesExecutable = true;
			ignoreWidgetEventProhibitedProcessesDescription = true;

			// Clear the widgets in the "Selected Process" group
			checkBoxProhibitedProcessActive.Checked = true;
			checkBoxProhibitedProcessCurrentUser.Checked = true;
			checkBoxProhibitedProcessStrongKill.Checked = false;
			listBoxProhibitedProcessOS.SelectedIndex = IntWin;
			textBoxProhibitedProcessExecutable.Text = "";
			textBoxProhibitedProcessOriginalName.Text = "";
			textBoxProhibitedProcessDescription.Text = "";
			textBoxProhibitedProcessIdentifier.Text = "";
			textBoxProhibitedProcessUser.Text = "";

			// Reset the ignore widget event flags
			ignoreWidgetEventProhibitedProcessesActive = false;
			ignoreWidgetEventProhibitedProcessesOS = false;
			ignoreWidgetEventProhibitedProcessesExecutable = false;
			ignoreWidgetEventProhibitedProcessesDescription = false;
		}


		private void dataGridViewProhibitedProcesses_SelectionChanged(object sender, EventArgs e)
		{
			// CAUTION:
			// If a row was previously selected and the user clicks onto another row,
			// the SelectionChanged() event is fired TWICE!!!
			// The first time, it is only for UNselecting the old row,
			// so the SelectedRows.Count is ZERO, so ignore this event handler!
			// The second time, SelectedRows.Count is ONE.
			// Now you can set the widgets in the "Selected Process" groupBox.

			if (dataGridViewProhibitedProcesses.SelectedRows.Count != 1) return;
			SEBSettings.prohibitedProcessIndex = dataGridViewProhibitedProcesses.SelectedRows[0].Index;

			// The process list should contain at least one element here:
			// SEBSettings.prohibitedProcessList.Count >  0
			// SEBSettings.prohibitedProcessIndex      >= 0
			LoadAndUpdateProhibitedSelectedProcessGroup(SEBSettings.prohibitedProcessIndex);
		}


		private void dataGridViewProhibitedProcesses_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// When a CheckBox/ListBox/TextBox entry of a DataGridView table cell is edited,
			// immediately call the CellValueChanged() event,
			// which will update the SelectedProcess data and widgets.
			if (dataGridViewProhibitedProcesses.IsCurrentCellDirty)
				dataGridViewProhibitedProcesses.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}


		private void dataGridViewProhibitedProcesses_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{

			// Prevent double events from switching to false process index
			if (ignoreCellEventProhibitedProcessesActive == true) return;
			if (ignoreCellEventProhibitedProcessesOS == true) return;
			if (ignoreCellEventProhibitedProcessesExecutable == true) return;
			if (ignoreCellEventProhibitedProcessesDescription == true) return;

			// Get the current cell where the user has changed a value
			int row = dataGridViewProhibitedProcesses.CurrentCellAddress.Y;
			int column = dataGridViewProhibitedProcesses.CurrentCellAddress.X;

			// At the beginning, row = -1 and column = -1, so skip this event
			if (row < 0) return;
			if (column < 0) return;

			// Get the changed value of the current cell
			object value = dataGridViewProhibitedProcesses.CurrentCell.EditedFormattedValue;

			// Convert the selected "OS" ListBox entry from String to Integer
			if (column == IntColumnProcessOS)
			{
				if ((String) value == StringOSX) value = IntOSX;
				else if ((String) value == StringWin) value = IntWin;
			}

			// Get the process data of the process belonging to the current row
			SEBSettings.prohibitedProcessIndex = row;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];

			// Update the process data belonging to the current cell
			if (column == IntColumnProcessActive) SEBSettings.prohibitedProcessData[SEBSettings.KeyActive] = (Boolean) value;
			if (column == IntColumnProcessOS) SEBSettings.prohibitedProcessData[SEBSettings.KeyOS] = (Int32) value;
			if (column == IntColumnProcessExecutable) SEBSettings.prohibitedProcessData[SEBSettings.KeyExecutable] = (String) value;
			if (column == IntColumnProcessDescription) SEBSettings.prohibitedProcessData[SEBSettings.KeyDescription] = (String) value;

			// Beware double events:
			// when a cell has been edited, update its corresponding widget in "Selected Process" group,
			// but prevent the following "widget changed" event from firing the "cell changed" event once more!
			if (column == IntColumnProcessActive) ignoreWidgetEventProhibitedProcessesActive = true;
			if (column == IntColumnProcessOS) ignoreWidgetEventProhibitedProcessesOS = true;
			if (column == IntColumnProcessExecutable) ignoreWidgetEventProhibitedProcessesExecutable = true;
			if (column == IntColumnProcessDescription) ignoreWidgetEventProhibitedProcessesDescription = true;

			// In "Selected Process" group: update the widget belonging to the current cell
			// (this will fire the corresponding "widget changed" event).
			if (column == IntColumnProcessActive) checkBoxProhibitedProcessActive.Checked = (Boolean) value;
			if (column == IntColumnProcessOS) listBoxProhibitedProcessOS.SelectedIndex = (Int32) value;
			if (column == IntColumnProcessExecutable) textBoxProhibitedProcessExecutable.Text = (String) value;
			if (column == IntColumnProcessDescription) textBoxProhibitedProcessDescription.Text = (String) value;

			// Reset the ignore widget event flags
			if (column == IntColumnProcessActive) ignoreWidgetEventProhibitedProcessesActive = false;
			if (column == IntColumnProcessOS) ignoreWidgetEventProhibitedProcessesOS = false;
			if (column == IntColumnProcessExecutable) ignoreWidgetEventProhibitedProcessesExecutable = false;
			if (column == IntColumnProcessDescription) ignoreWidgetEventProhibitedProcessesDescription = false;
		}


		private void buttonAddProhibitedProcess_Click(object sender, EventArgs e)
		{
			// Get the process list
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];

			if (SEBSettings.prohibitedProcessList.Count > 0)
			{
				if (dataGridViewProhibitedProcesses.SelectedRows.Count != 1) return;
				//SEBSettings.prohibitedProcessIndex = dataGridViewProhibitedProcesses.SelectedRows[0].Index;
				SEBSettings.prohibitedProcessIndex = SEBSettings.prohibitedProcessList.Count;
			}
			else
			{
				// If process list was empty before, enable it
				SEBSettings.prohibitedProcessIndex = 0;
				dataGridViewProhibitedProcesses.Enabled = true;
				groupBoxProhibitedProcess.Enabled = true;
			}

			// Create new process dataset containing default values
			DictObj processData = new DictObj();

			processData[SEBSettings.KeyActive] = true;
			processData[SEBSettings.KeyCurrentUser] = true;
			processData[SEBSettings.KeyStrongKill] = false;
			processData[SEBSettings.KeyOS] = IntWin;
			processData[SEBSettings.KeyExecutable] = "";
			processData[SEBSettings.KeyOriginalName] = "";
			processData[SEBSettings.KeyDescription] = "";
			processData[SEBSettings.KeyIdentifier] = "";
			processData[SEBSettings.KeyUser] = "";

			// Insert new process into process list at position index
			SEBSettings.prohibitedProcessList.Insert(SEBSettings.prohibitedProcessIndex, processData);
			dataGridViewProhibitedProcesses.Rows.Insert(SEBSettings.prohibitedProcessIndex, true, StringOS[IntWin], "", "");
			dataGridViewProhibitedProcesses.Rows[SEBSettings.prohibitedProcessIndex].Selected = true;
		}


		private void buttonRemoveProhibitedProcess_Click(object sender, EventArgs e)
		{
			if (dataGridViewProhibitedProcesses.SelectedRows.Count != 1) return;

			// Clear the widgets in the "Selected Process" group
			ClearProhibitedSelectedProcessGroup();

			// Delete process from process list at position index
			SEBSettings.prohibitedProcessIndex = dataGridViewProhibitedProcesses.SelectedRows[0].Index;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessList.RemoveAt(SEBSettings.prohibitedProcessIndex);
			dataGridViewProhibitedProcesses.Rows.RemoveAt(SEBSettings.prohibitedProcessIndex);

			if (SEBSettings.prohibitedProcessIndex == SEBSettings.prohibitedProcessList.Count)
				SEBSettings.prohibitedProcessIndex--;

			if (SEBSettings.prohibitedProcessList.Count > 0)
			{
				dataGridViewProhibitedProcesses.Rows[SEBSettings.prohibitedProcessIndex].Selected = true;
			}
			else
			{
				// If process list is now empty, disable it
				SEBSettings.prohibitedProcessIndex = -1;
				dataGridViewProhibitedProcesses.Enabled = false;
				groupBoxProhibitedProcess.Enabled = false;
			}
		}


		private void buttonChooseProhibitedExecutable_Click(object sender, EventArgs e)
		{

		}

		private void buttonChooseProhibitedProcess_Click(object sender, EventArgs e)
		{

		}


		private void checkBoxProhibitedProcessActive_CheckedChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventProhibitedProcessesActive == true) return;
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyActive] = checkBoxProhibitedProcessActive.Checked;
			Boolean active = checkBoxProhibitedProcessActive.Checked;
			ignoreCellEventProhibitedProcessesActive = true;
			dataGridViewProhibitedProcesses.Rows[SEBSettings.prohibitedProcessIndex].Cells[IntColumnProcessActive].Value = active.ToString();
			ignoreCellEventProhibitedProcessesActive = false;
		}


		private void checkBoxProhibitedProcessCurrentUser_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyCurrentUser] = checkBoxProhibitedProcessCurrentUser.Checked;
		}

		private void checkBoxProhibitedProcessStrongKill_CheckedChanged(object sender, EventArgs e)
		{
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyStrongKill] = checkBoxProhibitedProcessStrongKill.Checked;
		}


		private void listBoxProhibitedProcessOS_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventProhibitedProcessesOS == true) return;
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyOS] = listBoxProhibitedProcessOS.SelectedIndex;
			Int32 os = listBoxProhibitedProcessOS.SelectedIndex;
			ignoreCellEventProhibitedProcessesOS = true;
			dataGridViewProhibitedProcesses.Rows[SEBSettings.prohibitedProcessIndex].Cells[IntColumnProcessOS].Value = StringOS[os];
			ignoreCellEventProhibitedProcessesOS = false;
		}


		private void textBoxProhibitedProcessExecutable_TextChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventProhibitedProcessesExecutable == true) return;
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyExecutable] = textBoxProhibitedProcessExecutable.Text;
			String executable = textBoxProhibitedProcessExecutable.Text;
			ignoreCellEventProhibitedProcessesExecutable = true;
			dataGridViewProhibitedProcesses.Rows[SEBSettings.prohibitedProcessIndex].Cells[IntColumnProcessExecutable].Value = executable;
			ignoreCellEventProhibitedProcessesExecutable = false;
		}

		private void textBoxProhibitedProcessOriginalName_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyOriginalName] = textBoxProhibitedProcessOriginalName.Text;
		}

		private void textBoxProhibitedProcessDescription_TextChanged(object sender, EventArgs e)
		{
			// Prevent double events from switching to false process index
			if (ignoreWidgetEventProhibitedProcessesDescription == true) return;
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyDescription] = textBoxProhibitedProcessDescription.Text;
			String description = textBoxProhibitedProcessDescription.Text;
			ignoreCellEventProhibitedProcessesDescription = true;
			dataGridViewProhibitedProcesses.Rows[SEBSettings.prohibitedProcessIndex].Cells[IntColumnProcessDescription].Value = description;
			ignoreCellEventProhibitedProcessesDescription = false;
		}


		private void textBoxProhibitedProcessIdentifier_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyIdentifier] = textBoxProhibitedProcessIdentifier.Text;
		}

		private void textBoxProhibitedProcessUser_TextChanged(object sender, EventArgs e)
		{
			if (SEBSettings.prohibitedProcessIndex < 0) return;
			SEBSettings.prohibitedProcessList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyProhibitedProcesses];
			SEBSettings.prohibitedProcessData = (DictObj) SEBSettings.prohibitedProcessList[SEBSettings.prohibitedProcessIndex];
			SEBSettings.prohibitedProcessData[SEBSettings.KeyUser] = textBoxProhibitedProcessUser.Text;
		}

		private void buttonProhibitedProcessCodeSignature_Click(object sender, EventArgs e)
		{

		}

		private void checkBoxEnableURLFilter_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnable] = checkBoxEnableURLFilter.Checked;
			checkBoxEnableURLContentFilter.Enabled = checkBoxEnableURLFilter.Checked;
		}

		private void checkBoxEnableURLContentFilter_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnableContentFilter] = checkBoxEnableURLContentFilter.Checked;
		}

		// ******************************
		// Group "Network - Certificates"
		// ******************************
		private void comboBoxChooseSSLServerCertificate_SelectedIndexChanged(object sender, EventArgs e)
		{
			var cert = (X509Certificate2) certificateSSLReferences[comboBoxChooseSSLServerCertificate.SelectedIndex];

			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];

			SEBSettings.embeddedCertificateIndex = SEBSettings.embeddedCertificateList.Count;

			DictObj certData = new DictObj();

			String stringSSLCertificateType;

			certData[SEBSettings.KeyCertificateDataBase64] = exportToPEM(cert);
			if (checkBoxDebugCertificate.Checked)
			{
				certData[SEBSettings.KeyType] = IntSSLDebugCertificate;
				stringSSLCertificateType = StringSSLDebugCertificate;
				checkBoxDebugCertificate.Checked = false;
			}
			else
			{
				// We also save the certificate data into the deprecated subkey certificateDataWin (for downwards compatibility to < SEB 2.2)
				certData[SEBSettings.KeyCertificateDataWin] = exportToPEM(cert);
				certData[SEBSettings.KeyType] = IntSSLClientCertificate;
				stringSSLCertificateType = StringSSLServerCertificate;
			}
			certData[SEBSettings.KeyName] = comboBoxChooseSSLServerCertificate.SelectedItem;

			SEBSettings.embeddedCertificateList.Insert(SEBSettings.embeddedCertificateIndex, certData);

			dataGridViewEmbeddedCertificates.Rows.Insert(SEBSettings.embeddedCertificateIndex, stringSSLCertificateType, comboBoxChooseSSLServerCertificate.SelectedItem);
			dataGridViewEmbeddedCertificates.Rows[SEBSettings.embeddedCertificateIndex].Selected = true;

			comboBoxChooseSSLServerCertificate.BeginInvoke((Action) (() =>
			{
				comboBoxChooseSSLServerCertificate.Text = SEBUIStrings.ChooseEmbeddedCert;
			}));

			dataGridViewEmbeddedCertificates.Enabled = true;
		}

		private void comboBoxChooseCACertificate_SelectedIndexChanged(object sender, EventArgs e)
		{
			var cert = (X509Certificate2) certificateSSLReferences[comboBoxChooseCACertificate.SelectedIndex];

			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];

			SEBSettings.embeddedCertificateIndex = SEBSettings.embeddedCertificateList.Count;

			DictObj certData = new DictObj();

			certData[SEBSettings.KeyCertificateDataBase64] = exportToPEM(cert);
			certData[SEBSettings.KeyType] = IntCACertificate;
			certData[SEBSettings.KeyName] = comboBoxChooseCACertificate.SelectedItem;

			SEBSettings.embeddedCertificateList.Insert(SEBSettings.embeddedCertificateIndex, certData);

			dataGridViewEmbeddedCertificates.Rows.Insert(SEBSettings.embeddedCertificateIndex, StringCACertificate, comboBoxChooseCACertificate.SelectedItem);
			dataGridViewEmbeddedCertificates.Rows[SEBSettings.embeddedCertificateIndex].Selected = true;

			comboBoxChooseCACertificate.BeginInvoke((Action) (() =>
			{
				comboBoxChooseCACertificate.Text = SEBUIStrings.ChooseEmbeddedCert;
			}));

			dataGridViewEmbeddedCertificates.Enabled = true;
		}

		private void checkBoxPinEmbeddedCertificates_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyPinEmbeddedCertificates] = checkBoxPinEmbeddedCertificates.Checked;
		}

		private void comboBoxChooseIdentityToEmbed_SelectedIndexChanged(object sender, EventArgs e)
		{
			var cert = (X509Certificate2) certificateReferences[comboBoxChooseIdentityToEmbed.SelectedIndex];

			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];

			SEBSettings.embeddedCertificateIndex = SEBSettings.embeddedCertificateList.Count;

			DictObj identityToEmbedd = new DictObj();

			byte[] certData = new byte[0];

			try
			{
				certData = cert.Export(X509ContentType.Pkcs12, SEBClientInfo.DEFAULT_KEY);
			}

			catch (Exception certExportException)
			{
				Logger.AddError(string.Format("The identity (certificate with private key) {0} could not be exported", comboBoxChooseIdentityToEmbed.SelectedItem), null, certExportException, certExportException.Message);

				MessageBox.Show(SEBUIStrings.identityExportError, string.Format(SEBUIStrings.identityExportErrorMessage, comboBoxChooseIdentityToEmbed.SelectedItem), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (certData.Length > 0)
			{
				identityToEmbedd[SEBSettings.KeyCertificateData] = certData;
				//certData[SEBSettings.KeyCertificateDataWin] = exportToPEM(cert);
				identityToEmbedd[SEBSettings.KeyType] = 1;
				identityToEmbedd[SEBSettings.KeyName] = comboBoxChooseIdentityToEmbed.SelectedItem;

				SEBSettings.embeddedCertificateList.Insert(SEBSettings.embeddedCertificateIndex, identityToEmbedd);

				dataGridViewEmbeddedCertificates.Rows.Insert(SEBSettings.embeddedCertificateIndex, "Identity", comboBoxChooseIdentityToEmbed.SelectedItem);
				dataGridViewEmbeddedCertificates.Rows[SEBSettings.embeddedCertificateIndex].Selected = true;
			}

			comboBoxChooseIdentityToEmbed.BeginInvoke((Action) (() =>
			{
				comboBoxChooseIdentityToEmbed.Text = SEBUIStrings.ChooseEmbeddedCert;
			}));

			dataGridViewEmbeddedCertificates.Enabled = true;
		}

		/// <summary>
		/// Export a certificate to a PEM format string
		/// </summary>
		/// <param name="cert">The certificate to export</param>
		/// <returns>A PEM encoded string</returns>
		private string exportToPEM(X509Certificate cert)
		{
			string certToBase64String = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
			//certToBase64String = certToBase64String.Replace("/", @"\/");
			//certToBase64String = certToBase64String.Substring(0, certToBase64String.Length - 1);

			StringBuilder builder = new StringBuilder();

			//builder.Append("-----BEGIN CERTIFICATE-----");
			builder.Append(certToBase64String); //Convert.ToBase64String(cert.Export(X509ContentType.Cert))); //, Base64FormattingOptions.InsertLineBreaks));
												//builder.Append("-----END CERTIFICATE-----");

			return builder.ToString();
		}

		private void dataGridViewEmbeddedCertificates_SelectionChanged(object sender, EventArgs e)
		{
			// CAUTION:
			// If a row was previously selected and the user clicks onto another row,
			// the SelectionChanged() event is fired TWICE!!!
			// The first time, it is only for UNselecting the old row,
			// so the SelectedRows.Count is ZERO, so ignore this event handler!
			// The second time, SelectedRows.Count is ONE.
			// Now you can set the widgets in the "Selected Process" groupBox.

			if (dataGridViewEmbeddedCertificates.SelectedRows.Count != 1) return;
			SEBSettings.embeddedCertificateIndex = dataGridViewEmbeddedCertificates.SelectedRows[0].Index;
		}


		private void dataGridViewEmbeddedCertificates_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// When a CheckBox/ListBox/TextBox entry of a DataGridView table cell is edited,
			// immediately call the CellValueChanged() event.
			if (dataGridViewEmbeddedCertificates.IsCurrentCellDirty)
				dataGridViewEmbeddedCertificates.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}


		private void dataGridViewEmbeddedCertificates_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Get the current cell where the user has changed a value
			int row = dataGridViewEmbeddedCertificates.CurrentCellAddress.Y;
			int column = dataGridViewEmbeddedCertificates.CurrentCellAddress.X;

			// At the beginning, row = -1 and column = -1, so skip this event
			if (row < 0) return;
			if (column < 0) return;

			// Get the changed value of the current cell
			object value = dataGridViewEmbeddedCertificates.CurrentCell.EditedFormattedValue;

			// Convert the selected Type ListBox entry from String to Integer
			if (column == IntColumnCertificateType)
			{
				if ((String) value == StringSSLServerCertificate) value = IntSSLClientCertificate;
				else if ((String) value == StringIdentity) value = IntIdentity;
				else if ((String) value == StringCACertificate) value = IntCACertificate;
				else if ((String) value == StringSSLDebugCertificate) value = IntSSLDebugCertificate;
			}

			// Get the data of the certificate belonging to the cell (row)
			SEBSettings.embeddedCertificateIndex = row;
			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];
			SEBSettings.embeddedCertificateData = (DictObj) SEBSettings.embeddedCertificateList[SEBSettings.embeddedCertificateIndex];

			// Update the certificate data belonging to the current cell
			if (column == IntColumnCertificateType) SEBSettings.embeddedCertificateData[SEBSettings.KeyType] = (Int32) value;
			if (column == IntColumnCertificateName) SEBSettings.embeddedCertificateData[SEBSettings.KeyName] = (String) value;
		}


		private void buttonRemoveEmbeddedCertificate_Click(object sender, EventArgs e)
		{
			if (dataGridViewEmbeddedCertificates.SelectedRows.Count != 1) return;
			SEBSettings.embeddedCertificateIndex = dataGridViewEmbeddedCertificates.SelectedRows[0].Index;

			// Delete certificate from certificate list at position index
			SEBSettings.embeddedCertificateList = (ListObj) SEBSettings.settingsCurrent[SEBSettings.KeyEmbeddedCertificates];
			SEBSettings.embeddedCertificateList.RemoveAt(SEBSettings.embeddedCertificateIndex);
			dataGridViewEmbeddedCertificates.Rows.RemoveAt(SEBSettings.embeddedCertificateIndex);

			if (SEBSettings.embeddedCertificateIndex == SEBSettings.embeddedCertificateList.Count)
				SEBSettings.embeddedCertificateIndex--;

			if (SEBSettings.embeddedCertificateList.Count > 0)
			{
				dataGridViewEmbeddedCertificates.Rows[SEBSettings.embeddedCertificateIndex].Selected = true;
			}
			else
			{
				// If certificate list is now empty, disable it
				SEBSettings.embeddedCertificateIndex = -1;
				dataGridViewEmbeddedCertificates.Enabled = false;
			}
		}



		// *************************
		// Group "Network - Proxies"
		// *************************
		private void radioButtonUseSystemProxySettings_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUseSystemProxySettings.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyProxySettingsPolicy] = 0;
			else SEBSettings.settingsCurrent[SEBSettings.KeyProxySettingsPolicy] = 1;
		}

		private void radioButtonUseSebProxySettings_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonUseSebProxySettings.Checked == true)
				SEBSettings.settingsCurrent[SEBSettings.KeyProxySettingsPolicy] = 1;
			else SEBSettings.settingsCurrent[SEBSettings.KeyProxySettingsPolicy] = 0;
		}

		private void checkBoxExcludeSimpleHostnames_CheckedChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[SEBSettings.KeyExcludeSimpleHostnames] = checkBoxExcludeSimpleHostnames.Checked;
		}

		private void checkBoxUsePassiveFTPMode_CheckedChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[SEBSettings.KeyFTPPassive] = checkBoxUsePassiveFTPMode.Checked;
		}


		private void dataGridViewProxyProtocols_SelectionChanged(object sender, EventArgs e)
		{
			// CAUTION:
			// If a row was previously selected and the user clicks onto another row,
			// the SelectionChanged() event is fired TWICE!!!
			// The first time, it is only for UNselecting the old row,
			// so the SelectedRows.Count is ZERO, so ignore this event handler!
			// The second time, SelectedRows.Count is ONE.
			// Now you can set the widgets in the "Selected Process" groupBox.

			if (dataGridViewProxyProtocols.SelectedRows.Count != 1) return;
			SEBSettings.proxyProtocolIndex = dataGridViewProxyProtocols.SelectedRows[0].Index;

			// if proxyProtocolIndex is    0 (AutoDiscovery    ), do nothing
			// if proxyProtocolIndex is    1 (AutoConfiguration), enable Proxy URL    widgets
			// if proxyProtocolIndex is >= 2 (... Proxy Server ), enable Proxy Server widgets

			Boolean useAutoConfiguration = (SEBSettings.proxyProtocolIndex == IntProxyAutoConfiguration);
			Boolean useProxyServer = (SEBSettings.proxyProtocolIndex > IntProxyAutoConfiguration);

			// Enable the proxy widgets belonging to Auto Configuration
			labelAutoProxyConfigurationURL.Visible = useAutoConfiguration;
			labelProxyConfigurationFileURL.Visible = useAutoConfiguration;
			textBoxIfYourNetworkAdministrator.Visible = useAutoConfiguration;
			textBoxAutoProxyConfigurationURL.Visible = useAutoConfiguration;
			buttonChooseProxyConfigurationFile.Visible = useAutoConfiguration;

			// Enable the proxy widgets belonging to Proxy Server
			// (HTTP, HTTPS, FTP, SOCKS, RTSP)
			labelProxyServerHost.Visible = useProxyServer;
			labelProxyServerPort.Visible = useProxyServer;
			textBoxProxyServerHost.Visible = useProxyServer;
			textBoxProxyServerPort.Visible = useProxyServer;

			labelProxyServerUsername.Visible = useProxyServer;
			labelProxyServerPassword.Visible = useProxyServer;
			textBoxProxyServerUsername.Visible = useProxyServer;
			textBoxProxyServerPassword.Visible = useProxyServer;

			checkBoxProxyServerRequires.Visible = useProxyServer;

			if (useProxyServer)
			{
				labelProxyServerHost.Text = StringProxyProtocolServerLabel[SEBSettings.proxyProtocolIndex];
				labelProxyServerHost.Text += " Proxy Server";
			}

			// Get the proxy protocol type
			String KeyProtocolType = KeyProxyProtocolType[SEBSettings.proxyProtocolIndex];

			// Get the proxies data
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];

			// Update the proxy widgets
			if (useAutoConfiguration)
			{
				textBoxAutoProxyConfigurationURL.Text = (String) SEBSettings.proxiesData[SEBSettings.KeyAutoConfigurationURL];
			}

			if (useProxyServer)
			{
				checkBoxProxyServerRequires.Checked = (Boolean) SEBSettings.proxiesData[KeyProtocolType + SEBSettings.KeyRequires];
				textBoxProxyServerHost.Text = (String) SEBSettings.proxiesData[KeyProtocolType + SEBSettings.KeyHost];
				textBoxProxyServerPort.Text = (String) SEBSettings.proxiesData[KeyProtocolType + SEBSettings.KeyPort].ToString();
				textBoxProxyServerUsername.Text = (String) SEBSettings.proxiesData[KeyProtocolType + SEBSettings.KeyUsername];
				textBoxProxyServerPassword.Text = (String) SEBSettings.proxiesData[KeyProtocolType + SEBSettings.KeyPassword];

				// Disable the username/password textboxes when they are not required
				textBoxProxyServerUsername.Enabled = checkBoxProxyServerRequires.Checked;
				textBoxProxyServerPassword.Enabled = checkBoxProxyServerRequires.Checked;
			}
		}


		private void dataGridViewProxyProtocols_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// When a CheckBox/ListBox/TextBox entry of a DataGridView table cell is edited,
			// immediately call the CellValueChanged() event.
			if (dataGridViewProxyProtocols.IsCurrentCellDirty)
				dataGridViewProxyProtocols.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}


		private void dataGridViewProxyProtocols_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Get the current cell where the user has changed a value
			int row = dataGridViewProxyProtocols.CurrentCellAddress.Y;
			int column = dataGridViewProxyProtocols.CurrentCellAddress.X;

			// At the beginning, row = -1 and column = -1, so skip this event
			if (row < 0) return;
			if (column < 0) return;

			// Get the changed value of the current cell
			object value = dataGridViewProxyProtocols.CurrentCell.EditedFormattedValue;

			// Get the proxies data of the proxy protocol belonging to the cell (row)
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];

			SEBSettings.proxyProtocolIndex = row;

			// Update the proxy enable data belonging to the current cell
			if (column == IntColumnProxyProtocolEnable)
			{
				String key = KeyProxyProtocolEnable[row];
				SEBSettings.proxiesData[key] = (Boolean) value;
				BooleanProxyProtocolEnabled[row] = (Boolean) value;
			}
		}


		private void textBoxAutoProxyConfigurationURL_TextChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[SEBSettings.KeyAutoConfigurationURL] = textBoxAutoProxyConfigurationURL.Text;
		}


		private void buttonChooseProxyConfigurationFile_Click(object sender, EventArgs e)
		{

		}


		private void textBoxProxyServerHost_TextChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			String key = KeyProxyProtocolType[SEBSettings.proxyProtocolIndex] + SEBSettings.KeyHost;
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[key] = textBoxProxyServerHost.Text;
		}

		private void textBoxProxyServerPort_TextChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			String key = KeyProxyProtocolType[SEBSettings.proxyProtocolIndex] + SEBSettings.KeyPort;
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];

			// Convert the "Port" string to an integer
			try
			{
				SEBSettings.proxiesData[key] = Int32.Parse(textBoxProxyServerPort.Text);
			}
			catch (FormatException)
			{
				textBoxProxyServerPort.Text = "";
			}
		}

		private void checkBoxProxyServerRequiresPassword_CheckedChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			String key = KeyProxyProtocolType[SEBSettings.proxyProtocolIndex] + SEBSettings.KeyRequires;
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[key] = (Boolean) checkBoxProxyServerRequires.Checked;

			// Disable the username/password textboxes when they are not required
			textBoxProxyServerUsername.Enabled = checkBoxProxyServerRequires.Checked;
			textBoxProxyServerPassword.Enabled = checkBoxProxyServerRequires.Checked;
		}

		private void textBoxProxyServerUsername_TextChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			String key = KeyProxyProtocolType[SEBSettings.proxyProtocolIndex] + SEBSettings.KeyUsername;
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[key] = textBoxProxyServerUsername.Text;
		}

		private void textBoxProxyServerPassword_TextChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			String key = KeyProxyProtocolType[SEBSettings.proxyProtocolIndex] + SEBSettings.KeyPassword;
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			SEBSettings.proxiesData[key] = textBoxProxyServerPassword.Text;
		}


		private void textBoxBypassedProxyHostList_TextChanged(object sender, EventArgs e)
		{
			// Get the proxies data
			SEBSettings.proxiesData = (DictObj) SEBSettings.settingsCurrent[SEBSettings.KeyProxies];
			string bypassedProxiesCommaSeparatedList = textBoxBypassedProxyHostList.Text;
			// Create List
			List<string> bypassedProxyHostList = bypassedProxiesCommaSeparatedList.Split(',').ToList();
			// Trim whitespace from host strings
			ListObj bypassedProxyTrimmedHostList = new ListObj();
			foreach (string host in bypassedProxyHostList)
			{
				bypassedProxyTrimmedHostList.Add(host.Trim());
			}
			SEBSettings.proxiesData[SEBSettings.KeyExceptionsList] = bypassedProxyTrimmedHostList;
		}


		// ****************
		// Group "Security"
		// ****************
		private void listBoxSebServicePolicy_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeySebServicePolicy] = listBoxSebServicePolicy.SelectedIndex;
		}

		private void checkBoxAllowVirtualMachine_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowVirtualMachine] = checkBoxAllowVirtualMachine.Checked;
		}

		private void checkBoxAllowScreenSharing_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowScreenSharing] = checkBoxAllowScreenSharing.Checked;
		}

		private void checkBoxEnablePrivateClipboard_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnablePrivateClipboard] = checkBoxEnablePrivateClipboard.Checked;
		}

		private void radioCreateNewDesktop_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyCreateNewDesktop] = radioCreateNewDesktop.Checked;
			checkBoxMonitorProcesses.Enabled = !radioCreateNewDesktop.Checked;
			checkBoxMonitorProcesses.Checked = radioCreateNewDesktop.Checked;

			if (radioCreateNewDesktop.Checked && (Boolean) SEBSettings.settingsCurrent[SEBSettings.KeyTouchOptimized] == true)
			{
				MessageBox.Show("Touch optimization will not work when kiosk mode is set to Create New Desktop, please change the appearance.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			if (radioCreateNewDesktop.Checked)
			{
				CheckAndOptionallyRemoveDefaultProhibitedProcesses();
				UpdateAllWidgetsOfProgram();
			}
		}

		private void radioKillExplorerShell_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyKillExplorerShell] = radioKillExplorerShell.Checked;
			checkBoxMonitorProcesses.Enabled = !radioKillExplorerShell.Checked;
			checkBoxMonitorProcesses.Checked = radioKillExplorerShell.Checked;
		}

		private void checkBoxAllowWlan_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowWLAN] = checkboxAllowWlan.Checked;
		}

		private void checkBoxAllowSiri_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowSiri] = checkBoxAllowSiri.Checked;
		}

		private void checkBoxEnableAppSwitcherCheck_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableAppSwitcherCheck] = checkBoxEnableAppSwitcherCheck.Checked;
		}

		private void checkBoxForceAppFolderInstall_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyForceAppFolderInstall] = checkBoxForceAppFolderInstall.Checked;
		}

		private void checkBoxEnableLogging_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableLogging] = checkBoxEnableLogging.Checked;
		}

		private void buttonLogDirectoryWin_Click(object sender, EventArgs e)
		{
			// Set the default directory in the Folder Browser Dialog
			//folderBrowserDialogLogDirectoryWin.SelectedPath = textBoxLogDirectoryWin.Text;
			folderBrowserDialogLogDirectoryWin.RootFolder = Environment.SpecialFolder.Desktop;
			//          folderBrowserDialogLogDirectoryWin.RootFolder = Environment.CurrentDirectory;

			// Get the user inputs in the File Dialog
			DialogResult dialogResult = folderBrowserDialogLogDirectoryWin.ShowDialog();
			String path = folderBrowserDialogLogDirectoryWin.SelectedPath;

			// If the user clicked "Cancel", do nothing
			if (dialogResult.Equals(DialogResult.Cancel)) return;

			// If the user clicked "OK", ...
			string pathUsingEnvironmentVariables = SEBClientInfo.ContractEnvironmentVariables(path);
			SEBSettings.settingsCurrent[SEBSettings.KeyLogDirectoryWin] = pathUsingEnvironmentVariables;
			textBoxLogDirectoryWin.Text = pathUsingEnvironmentVariables;
			if (String.IsNullOrEmpty(path))
			{
				checkBoxUseStandardDirectory.Checked = true;
			}
			else
			{
				checkBoxUseStandardDirectory.Checked = false;
			}
		}

		private void textBoxLogDirectoryWin_TextChanged(object sender, EventArgs e)
		{
			string path = textBoxLogDirectoryWin.Text;
			SEBSettings.settingsCurrent[SEBSettings.KeyLogDirectoryWin] = path;

			if (String.IsNullOrEmpty(path))
			{
				checkBoxUseStandardDirectory.Checked = true;
			}
			else
			{
				checkBoxUseStandardDirectory.Checked = false;
			}
		}

		private void checkBoxUseStandardDirectory_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxUseStandardDirectory.Checked)
			{
				SEBSettings.settingsCurrent[SEBSettings.KeyLogDirectoryWin] = "";
				textBoxLogDirectoryWin.Text = "";
			}
		}

		private void textBoxLogDirectoryOSX_TextChanged(object sender, EventArgs e)
		{
			string path = textBoxLogDirectoryOSX.Text;
			SEBSettings.settingsCurrent[SEBSettings.KeyLogDirectoryOSX] = path;
		}

		private void checkBoxAllowDictation_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowDictation] = checkBoxAllowDictation.Checked;
		}

		private void checkBoxDetectStoppedProcess_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyDetectStoppedProcess] = checkBoxDetectStoppedProcess.Checked;
		}

		private void checkBoxAllowDisplayMirroring_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowDisplayMirroring] = checkBoxAllowDisplayMirroring.Checked;
		}

		private void checkBoxAllowUserAppFolderInstall_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowUserAppFolderInstall] = checkBoxAllowUserAppFolderInstall.Checked;
		}

		private void checkBoxAllowedDisplayBuiltin_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplayBuiltin] = checkBoxAllowedDisplayBuiltin.Checked;
		}

		private void comboBoxMinMacOSVersion_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValMinMacOSVersion] = comboBoxMinMacOSVersion.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValMinMacOSVersion] = comboBoxMinMacOSVersion.Text;
			SEBSettings.settingsCurrent[SEBSettings.KeyMinMacOSVersion] = comboBoxMinMacOSVersion.SelectedIndex;
		}

		private void comboBoxMinMacOSVersion_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValMinMacOSVersion] = comboBoxMinMacOSVersion.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValMinMacOSVersion] = comboBoxMinMacOSVersion.Text;
			SEBSettings.settingsCurrent[SEBSettings.KeyMinMacOSVersion] = comboBoxMinMacOSVersion.SelectedIndex;
		}

		private void comboBoxAllowedDisplaysMaxNumber_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValAllowedDisplaysMaxNumber] = comboBoxAllowedDisplaysMaxNumber.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValAllowedDisplaysMaxNumber] = comboBoxAllowedDisplaysMaxNumber.Text;
			int allowedDisplaysMaxNumber = 1;
			int.TryParse(comboBoxAllowedDisplaysMaxNumber.Text, out allowedDisplaysMaxNumber);
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplaysMaxNumber] = allowedDisplaysMaxNumber;
		}

		private void comboBoxAllowedDisplaysMaxNumber_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.intArrayCurrent[SEBSettings.ValAllowedDisplaysMaxNumber] = comboBoxAllowedDisplaysMaxNumber.SelectedIndex;
			SEBSettings.strArrayCurrent[SEBSettings.ValAllowedDisplaysMaxNumber] = comboBoxAllowedDisplaysMaxNumber.Text;
			int allowedDisplaysMaxNumber = 1;
			int.TryParse(comboBoxAllowedDisplaysMaxNumber.Text, out allowedDisplaysMaxNumber);
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplaysMaxNumber] = allowedDisplaysMaxNumber;
		}


		// ****************
		// Group "Registry"
		// ****************



		// ******************
		// Group "Inside SEB"
		// ******************
		private void checkBoxInsideSebEnableSwitchUser_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableSwitchUser] = checkBoxInsideSebEnableSwitchUser.Checked;
		}

		private void checkBoxInsideSebEnableLockThisComputer_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableLockThisComputer] = checkBoxInsideSebEnableLockThisComputer.Checked;
		}

		private void checkBoxInsideSebEnableChangeAPassword_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableChangeAPassword] = checkBoxInsideSebEnableChangeAPassword.Checked;
		}

		private void checkBoxInsideSebEnableStartTaskManager_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableStartTaskManager] = checkBoxInsideSebEnableStartTaskManager.Checked;

		}

		private void checkBoxInsideSebEnableLogOff_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableLogOff] = checkBoxInsideSebEnableLogOff.Checked;
		}

		private void checkBoxInsideSebEnableShutDown_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableShutDown] = checkBoxInsideSebEnableShutDown.Checked;
		}

		private void checkBoxInsideSebEnableEaseOfAccess_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableEaseOfAccess] = checkBoxInsideSebEnableEaseOfAccess.Checked;
		}

		private void checkBoxInsideSebEnableVmWareClientShade_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableVmWareClientShade] = checkBoxInsideSebEnableVmWareClientShade.Checked;
		}

		private void checkBoxInsideSebEnableNetworkConnectionSelector_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyInsideSebEnableNetworkConnectionSelector] = checkBoxInsideSebEnableNetworkConnectionSelector.Checked;
		}


		// *******************
		// Group "Hooked Keys"
		// *******************
		private void checkBoxHookKeys_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyHookKeys] = checkBoxHookKeys.Checked;
		}



		// ********************
		// Group "Special Keys"
		// ********************
		private void checkBoxEnableEsc_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableEsc] = checkBoxEnableEsc.Checked;
		}

		private void checkBoxEnableCtrlEsc_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableCtrlEsc] = checkBoxEnableCtrlEsc.Checked;
		}

		private void checkBoxEnableAltEsc_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltEsc] = checkBoxEnableAltEsc.Checked;
		}

		private void checkBoxEnableAltTab_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltTab] = checkBoxEnableAltTab.Checked;
		}

		private void checkBoxEnableAltF4_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltF4] = checkBoxEnableAltF4.Checked;
		}

		private void checkBoxEnableRightMouse_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableRightMouse] = checkBoxEnableRightMouse.Checked;
		}

		private void checkBoxEnablePrintScreen_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnablePrintScreen] = checkBoxEnablePrintScreen.Checked;
			checkBoxEnableScreenCapture.Checked = checkBoxEnablePrintScreen.Checked;

		}

		private void checkBoxEnableAltMouseWheel_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableAltMouseWheel] = checkBoxEnableAltMouseWheel.Checked;
			checkBoxAllowBrowsingBackForward.Checked = checkBoxEnableAltMouseWheel.Checked;
		}


		// *********************
		// Group "Function Keys"
		// *********************
		private void checkBoxEnableF1_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF1] = checkBoxEnableF1.Checked;
		}

		private void checkBoxEnableF2_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF2] = checkBoxEnableF2.Checked;
		}

		private void checkBoxEnableF3_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF3] = checkBoxEnableF3.Checked;
		}

		private void checkBoxEnableF4_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF4] = checkBoxEnableF4.Checked;
		}

		private void checkBoxEnableF5_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF5] = checkBoxEnableF5.Checked;
		}

		private void checkBoxEnableF6_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF6] = checkBoxEnableF6.Checked;
		}

		private void checkBoxEnableF7_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF7] = checkBoxEnableF7.Checked;
		}

		private void checkBoxEnableF8_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF8] = checkBoxEnableF8.Checked;
		}

		private void checkBoxEnableF9_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF9] = checkBoxEnableF9.Checked;
		}

		private void checkBoxEnableF10_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF10] = checkBoxEnableF10.Checked;
		}

		private void checkBoxEnableF11_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF11] = checkBoxEnableF11.Checked;
		}

		private void checkBoxEnableF12_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableF12] = checkBoxEnableF12.Checked;
		}

		private void labelHashedAdminPassword_Click(object sender, EventArgs e)
		{

		}

		private void labelOpenLinksHTML_Click(object sender, EventArgs e)
		{

		}

		private void label6_Click(object sender, EventArgs e)
		{

		}

		private void checkBoxEnableZoomText_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableZoomText] = checkBoxEnableZoomText.Checked;
			enableZoomAdjustZoomMode();
		}

		private void checkBoxEnableZoomPage_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableZoomPage] = checkBoxEnableZoomPage.Checked;
			enableZoomAdjustZoomMode();
		}

		private void enableZoomAdjustZoomMode()
		{
			if (!checkBoxEnableZoomPage.Checked && !checkBoxEnableZoomText.Checked)
			{
				groupBoxZoomMode.Enabled = false;
			}
			else if (checkBoxEnableZoomPage.Checked && !checkBoxEnableZoomText.Checked)
			{
				groupBoxZoomMode.Enabled = true;
				radioButtonUseZoomPage.Checked = true;
				radioButtonUseZoomPage.Enabled = true;
				radioButtonUseZoomText.Enabled = false;
			}
			else if (!checkBoxEnableZoomPage.Checked && checkBoxEnableZoomText.Checked)
			{
				groupBoxZoomMode.Enabled = true;
				radioButtonUseZoomText.Checked = true;
				radioButtonUseZoomText.Enabled = true;
				radioButtonUseZoomPage.Enabled = false;
			}
			else
			{
				groupBoxZoomMode.Enabled = true;
				radioButtonUseZoomPage.Enabled = true;
				radioButtonUseZoomText.Enabled = true;
			}
		}

		private void checkBoxAllowSpellCheck_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowSpellCheck] = checkBoxAllowSpellCheck.Checked;
		}

		private void checkBoxAllowDictionaryLookup_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowDictionaryLookup] = checkBoxAllowDictionaryLookup.Checked;
		}


		private void checkBoxShowTime_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowTime] = checkBoxShowTime.Checked;
		}

		private void checkBoxShowKeyboardLayout_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowInputLanguage] = checkBoxShowKeyboardLayout.Checked;
		}

		private void SebWindowsConfigForm_Load(object sender, EventArgs e)
		{

		}

		private void editDuplicateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonEditDuplicate_Click(null, null);
		}

		private void configureClientToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonConfigureClient_Click(null, null);
		}

		private void applyAndStartSEBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonApplyAndStartSEB_Click(null, null);
		}

		private void openSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonOpenSettings_Click(null, null);
		}

		private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonSaveSettings_Click(null, null);
		}

		private void saveSettingsAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonSaveSettingsAs_Click(null, null);
		}

		private void defaultSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonRevertToDefaultSettings_Click(null, null);
		}

		private void localClientSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonRevertToLocalClientSettings_Click(null, null);
		}

		private void lastOpenedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			buttonRevertToLastOpened_Click(null, null);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ArePasswordsUnconfirmed()) return;

			Application.Exit();
		}

		private void SebWindowsConfigForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!quittingMyself)
			{
				if (ArePasswordsUnconfirmed())
				{
					e.Cancel = true;
					return;
				}

				int result = checkSettingsChanged();
				// User selected cancel, abort
				if (result == 2)
				{
					e.Cancel = true;
					return;
				}
				// User selected "Save current settings first: yes"
				if (result == 1)
				{
					// Abort if saving settings failed
					if (!saveCurrentSettings())
					{
						e.Cancel = true;
						return;
					}
					quittingMyself = true;
					Application.Exit();
				}
			}
		}

		private void SebWindowsConfigForm_DragDrop(object sender, DragEventArgs e)
		{
			string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
			if (files.Length > 0)
			{
				string filePath = files[0];
				string fileExtension = Path.GetExtension(filePath);
				if (String.Equals(fileExtension, ".seb",
				   StringComparison.OrdinalIgnoreCase))
				{
					openSettingsFile(filePath);
				}
			}
		}

		private void SebWindowsConfigForm_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		private void tabControlSebWindowsConfig_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (ArePasswordsUnconfirmed())
			{
				e.Cancel = true;
			}
			checkBoxEnableURLFilter.Checked = (bool) SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnable];
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void checkBoxEnableScreenCapture_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnablePrintScreen] = checkBoxEnableScreenCapture.Checked;
			checkBoxEnablePrintScreen.Checked = checkBoxEnableScreenCapture.Checked;
		}

		private void checkBoxEnableTouchExit_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableTouchExit] = checkBoxEnableTouchExit.Checked;
		}

		private void comboBoxAdditionalResourceStartUrl_DropDown(object sender, EventArgs e)
		{
			FillStartupResourcesinCombobox();
		}

		private void AddResourceToStartupResourceDropdown(DictObj resource)
		{
			if (!string.IsNullOrEmpty((string) resource[SEBSettings.KeyAdditionalResourcesResourceData]))
			{
				//check if SEB is the launcher
				//if (
				//    (string)((DictObj)((ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyPermittedProcesses])[
				//        (int)resource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher]])[SEBSettings.KeyTitle] ==
				//    "SEB")
				if ((int) resource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher] == 0)
				{
					comboBoxAdditionalResourceStartUrl.Items.Add(
					new KeyValuePair<string, string>((string) resource[SEBSettings.KeyAdditionalResourcesIdentifier],
						(string) resource[SEBSettings.KeyAdditionalResourcesTitle]));
				}
			}
		}

		private void FillStartupResourcesinCombobox()
		{
			comboBoxAdditionalResourceStartUrl.Items.Clear();
			foreach (DictObj l0Resource in SEBSettings.additionalResourcesList)
			{
				AddResourceToStartupResourceDropdown(l0Resource);
				foreach (DictObj l1Resource in (ListObj) l0Resource[SEBSettings.KeyAdditionalResources])
				{
					AddResourceToStartupResourceDropdown(l1Resource);
					foreach (DictObj l2Resource in (ListObj) l1Resource[SEBSettings.KeyAdditionalResources])
					{
						AddResourceToStartupResourceDropdown(l2Resource);
					}
				}
			}
		}

		private void comboBoxAdditionalResourceStartUrl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxAdditionalResourceStartUrl.SelectedItem is KeyValuePair<string, string>)
			{
				var selectedItem = (KeyValuePair<string, string>) comboBoxAdditionalResourceStartUrl.SelectedItem;
				textBoxStartURL.Text = "";
				SEBSettings.settingsCurrent[SEBSettings.KeyStartResource] = selectedItem.Key;
				SEBSettings.settingsCurrent[SEBSettings.KeyStartURL] = "";
			}
		}

		private void label10_Click(object sender, EventArgs e)
		{

		}

		private void textBoxBrowserSuffix_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserWindowTitleSuffix] = textBoxBrowserSuffix.Text;
		}

		private void checkBoxEnableAudioControl_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAudioControlEnabled] = checkBoxEnableAudioControl.Checked;
		}

		private void checkBoxMuteAudio_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAudioMute] = checkBoxMuteAudio.Checked;
		}

		private void checkBoxSetVolumeLevel_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAudioSetVolumeLevel] = checkBoxSetVolumeLevel.Checked;
		}

		private void trackBarVolumeLevel_Scroll(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAudioVolumeLevel] = trackBarVolumeLevel.Value;
		}

		private void collectLogFilesToolStripMenuItem_Click(object sender, EventArgs args)
		{
			new LogCollector(this).Run();
		}

		private void checkBoxAllowMainWindowAddressBar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyBrowserWindowAllowAddressBar] = checkBoxAllowMainWindowAddressBar.Checked;
		}

		private void checkBoxAllowAdditionalWindowAddressBar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowAllowAddressBar] = checkBoxAllowAdditionalWindowAddressBar.Checked;
		}

		private void checkBoxClearSessionOnStart_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyExamSessionClearCookiesOnStart] = checkBoxClearSessionOnStart.Checked;
		}

		private void checkBoxClearSessionOnEnd_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyExamSessionClearCookiesOnEnd] = checkBoxClearSessionOnEnd.Checked;
			checkBoxRemoveProfile.Enabled = checkBoxClearSessionOnEnd.Checked;
		}

		private void checkBoxShowSideMenu_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowSideMenu] = checkBoxShowSideMenu.Checked;
		}

		private void checkBoxAllowLogAccess_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowApplicationLog] = checkBoxAllowLogAccess.Checked;
			checkBoxShowLogButton.Enabled = checkBoxAllowLogAccess.Checked;
		}

		private void checkBoxShowLogButton_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowApplicationLogButton] = checkBoxShowLogButton.Checked;
		}

		private void checkBoxAllowChromeNotifications_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowChromeNotifications] = checkBoxAllowChromeNotifications.Checked;
		}

		private void checkBoxAllowWindowsUpdate_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowWindowsUpdate] = checkBoxAllowWindowsUpdate.Checked;
		}

		private void checkBoxDeveloperConsole_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowDeveloperConsole] = checkBoxAllowDeveloperConsole.Checked;
		}

		private void checkBoxAllowPdfReaderToolbar_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowPDFReaderToolbar] = checkBoxAllowPdfReaderToolbar.Checked;
		}

		private void checkBoxSetVmwareConfiguration_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeySetVmwareConfiguration] = checkBoxSetVmwareConfiguration.Checked;
			checkBoxInsideSebEnableVmWareClientShade.Enabled = checkBoxSetVmwareConfiguration.Checked;
		}

		private void checkBoxSebServiceIgnore_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeySebServiceIgnore] = checkBoxSebServiceIgnore.Checked;
			labelSebServiceIgnore.Enabled = !checkBoxSebServiceIgnore.Checked;
			labelSebServicePolicy.Enabled = !checkBoxSebServiceIgnore.Checked;
			listBoxSebServicePolicy.Enabled = !checkBoxSebServiceIgnore.Checked;
			groupBoxInsideSeb.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowWindowsUpdate.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowScreenSharing.Enabled = !checkBoxSebServiceIgnore.Checked;
			checkBoxAllowChromeNotifications.Enabled = !checkBoxSebServiceIgnore.Checked;
		}

		private void checkBoxAllowCustomDownloadLocation_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowCustomDownUploadLocation] = checkBoxAllowCustomDownloadLocation.Checked;
		}

		private void checkBoxAllowFind_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowFind] = checkBoxAllowFind.Checked;
		}

		private void checkBoxAllowReconfiguration_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowReconfiguration] = checkBoxAllowReconfiguration.Checked;
		}

		private void textBoxReconfigurationUrl_TextChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyReconfigurationUrl] = textBoxReconfigurationUrl.Text;
		}

		private void checkBoxResetOnQuitUrl_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyResetOnQuitUrl] = checkBoxResetOnQuitUrl.Checked;
		}

		private void checkBoxUseStartUrlQuery_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyUseStartUrlQuery] = checkBoxUseStartUrlQuery.Checked;
		}

		private void comboBoxUrlPolicyMainWindow_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyMainBrowserWindowUrlPolicy] = comboBoxUrlPolicyMainWindow.SelectedIndex;
		}

		private void comboBoxUrlPolicyNewWindow_SelectedIndexChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyNewBrowserWindowUrlPolicy] = comboBoxUrlPolicyNewWindow.SelectedIndex;
		}

		private void checkBoxEnforceBuiltinDisplay_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplayBuiltinEnforce] = checkBoxEnforceBuiltinDisplay.Checked;
		}

		private void checkBoxAllowedDisplayIgnoreError_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowedDisplayIgnoreFailure] = checkBoxAllowedDisplayIgnoreError.Checked;
		}

		private void checkBoxTemporaryDownloadDirectory_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyUseTemporaryDownUploadDirectory] = checkBoxTemporaryDownloadDirectory.Checked;
			buttonDownloadDirectoryWin.Enabled = !checkBoxTemporaryDownloadDirectory.Checked;
			textBoxDownloadDirectoryWin.Enabled = !checkBoxTemporaryDownloadDirectory.Checked;
		}

		private void checkBoxEnableMiddleMouse_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableMiddleMouse] = checkBoxEnableMiddleMouse.Checked;
		}

		private void checkBoxAllowPrint_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyAllowPrint] = checkBoxAllowPrint.Checked;
		}

		private void checkBoxEnableFindPrinter_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyEnableFindPrinter] = checkBoxEnableFindPrinter.Checked;
		}

		private void checkBoxShowFileSystemElementPath_CheckedChanged(object sender, EventArgs e)
		{
			SEBSettings.settingsCurrent[SEBSettings.KeyShowFileSystemElementPath] = checkBoxShowFileSystemElementPath.Checked;
		}
	}
}
