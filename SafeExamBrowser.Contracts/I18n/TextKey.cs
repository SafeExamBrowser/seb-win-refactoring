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
	/// Defines all text components of the user interface.
	/// </summary>
	public enum TextKey
	{
		Browser_ShowDeveloperConsole,
		LogWindow_Title,
		MessageBox_ConfigureClientSuccess,
		MessageBox_ConfigureClientSuccessTitle,
		MessageBox_ShutdownError,
		MessageBox_ShutdownErrorTitle,
		MessageBox_SingleInstance,
		MessageBox_SingleInstanceTitle,
		MessageBox_StartupError,
		MessageBox_StartupErrorTitle,
		Notification_AboutTooltip,
		Notification_LogTooltip,
		ProgressIndicator_CloseServiceConnection,
		ProgressIndicator_EmptyClipboard,
		ProgressIndicator_InitializeBrowser,
		ProgressIndicator_InitializeConfiguration,
		ProgressIndicator_InitializeKioskMode,
		ProgressIndicator_InitializeProcessMonitoring,
		ProgressIndicator_InitializeServiceConnection,
		ProgressIndicator_InitializeTaskbar,
		ProgressIndicator_InitializeWindowMonitoring,
		ProgressIndicator_InitializeWorkingArea,
		ProgressIndicator_RestartCommunicationHost,
		ProgressIndicator_RestoreWorkingArea,
		ProgressIndicator_RevertKioskMode,
		ProgressIndicator_ShutdownProcedure,
		ProgressIndicator_StartCommunicationHost,
		ProgressIndicator_StartEventHandling,
		ProgressIndicator_StartKeyboardInterception,
		ProgressIndicator_StartMouseInterception,
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
		RuntimeWindow_StartSession,
		RuntimeWindow_StopSession,
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
