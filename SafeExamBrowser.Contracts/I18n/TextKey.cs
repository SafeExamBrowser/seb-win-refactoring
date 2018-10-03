/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.I18n
{
	/// <summary>
	/// Defines all text elements of the user interface. Use the pattern "LogicalGroup_Description" to allow for a better overview over all
	/// keys and their usage (where applicable).
	/// </summary>
	public enum TextKey
	{
		Browser_ShowDeveloperConsole,
		LogWindow_Title,
		MessageBox_ApplicationError,
		MessageBox_ApplicationErrorTitle,
		MessageBox_ClientConfigurationQuestion,
		MessageBox_ClientConfigurationQuestionTitle,
		MessageBox_ConfigurationDownloadError,
		MessageBox_ConfigurationDownloadErrorTitle,
		MessageBox_Quit,
		MessageBox_QuitTitle,
		MessageBox_QuitError,
		MessageBox_QuitErrorTitle,
		MessageBox_ReconfigurationDenied,
		MessageBox_ReconfigurationDeniedTitle,
		MessageBox_ReconfigurationError,
		MessageBox_ReconfigurationErrorTitle,
		MessageBox_ReconfigurationQuestion,
		MessageBox_ReconfigurationQuestionTitle,
		MessageBox_SessionStartError,
		MessageBox_SessionStartErrorTitle,
		MessageBox_SessionStopError,
		MessageBox_SessionStopErrorTitle,
		MessageBox_ShutdownError,
		MessageBox_ShutdownErrorTitle,
		MessageBox_SingleInstance,
		MessageBox_SingleInstanceTitle,
		MessageBox_StartupError,
		MessageBox_StartupErrorTitle,
		Notification_AboutTooltip,
		Notification_LogTooltip,
		OperationStatus_CloseRuntimeConnection,
		OperationStatus_EmptyClipboard,
		OperationStatus_FinalizeServiceSession,
		OperationStatus_InitializeBrowser,
		OperationStatus_InitializeConfiguration,
		OperationStatus_InitializeKioskMode,
		OperationStatus_InitializeProcessMonitoring,
		OperationStatus_InitializeRuntimeConnection,
		OperationStatus_InitializeServiceSession,
		OperationStatus_InitializeTaskbar,
		OperationStatus_InitializeWindowMonitoring,
		OperationStatus_InitializeWorkingArea,
		OperationStatus_RestartCommunicationHost,
		OperationStatus_RestoreWorkingArea,
		OperationStatus_RevertKioskMode,
		OperationStatus_ShutdownProcedure,
		OperationStatus_StartClient,
		OperationStatus_StartCommunicationHost,
		OperationStatus_StartEventHandling,
		OperationStatus_StartKeyboardInterception,
		OperationStatus_StartMouseInterception,
		OperationStatus_InitializeSession,
		OperationStatus_StopClient,
		OperationStatus_StopCommunicationHost,
		OperationStatus_StopEventHandling,
		OperationStatus_StopKeyboardInterception,
		OperationStatus_StopMouseInterception,
		OperationStatus_StopProcessMonitoring,
		OperationStatus_StopWindowMonitoring,
		OperationStatus_TerminateBrowser,
		OperationStatus_TerminateTaskbar,
		OperationStatus_WaitExplorerStartup,
		OperationStatus_WaitExplorerTermination,
		OperationStatus_WaitRuntimeDisconnection,
		PasswordDialog_AdminPasswordRequired,
		PasswordDialog_AdminPasswordRequiredTitle,
		PasswordDialog_Cancel,
		PasswordDialog_Confirm,
		PasswordDialog_SettingsPasswordRequired,
		PasswordDialog_SettingsPasswordRequiredTitle,
		RuntimeWindow_ApplicationRunning,
		SystemControl_BatteryCharged,
		SystemControl_BatteryCharging,
		SystemControl_BatteryChargeCriticalWarning,
		SystemControl_BatteryChargeLowInfo,
		SystemControl_BatteryRemainingCharge,
		SystemControl_KeyboardLayoutTooltip,
		SystemControl_WirelessConnected,
		SystemControl_WirelessDisconnected,
		SystemControl_WirelessNotAvailable,
		Version
	}
}
