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
	/// Defines all text elements of the user interface. Use the pattern "Location_Description" to allow for a better overview over all
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
		ProgressIndicator_CloseRuntimeConnection,
		ProgressIndicator_EmptyClipboard,
		ProgressIndicator_FinalizeServiceSession,
		ProgressIndicator_InitializeBrowser,
		ProgressIndicator_InitializeConfiguration,
		ProgressIndicator_InitializeKioskMode,
		ProgressIndicator_InitializeProcessMonitoring,
		ProgressIndicator_InitializeRuntimeConnection,
		ProgressIndicator_InitializeServiceSession,
		ProgressIndicator_InitializeTaskbar,
		ProgressIndicator_InitializeWindowMonitoring,
		ProgressIndicator_InitializeWorkingArea,
		ProgressIndicator_RestartCommunicationHost,
		ProgressIndicator_RestoreWorkingArea,
		ProgressIndicator_RevertKioskMode,
		ProgressIndicator_ShutdownProcedure,
		ProgressIndicator_StartClient,
		ProgressIndicator_StartCommunicationHost,
		ProgressIndicator_StartEventHandling,
		ProgressIndicator_StartKeyboardInterception,
		ProgressIndicator_StartMouseInterception,
		ProgressIndicator_InitializeSession,
		ProgressIndicator_StopClient,
		ProgressIndicator_StopCommunicationHost,
		ProgressIndicator_StopEventHandling,
		ProgressIndicator_StopKeyboardInterception,
		ProgressIndicator_StopMouseInterception,
		ProgressIndicator_StopProcessMonitoring,
		ProgressIndicator_StopWindowMonitoring,
		ProgressIndicator_TerminateBrowser,
		ProgressIndicator_TerminateTaskbar,
		ProgressIndicator_WaitExplorerStartup,
		ProgressIndicator_WaitExplorerTermination,
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
