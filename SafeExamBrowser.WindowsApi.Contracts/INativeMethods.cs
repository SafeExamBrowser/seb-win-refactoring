/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Defines and wraps the functionality available via the native Windows API.
	/// </summary>
	public interface INativeMethods
	{
		/// <summary>
		/// Brings the window with the given handle to the foreground and activates it.
		/// </summary>
		void ActivateWindow(IntPtr handle);

		/// <summary>
		/// Deregisters a previously registered keyboard hook.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the hook could not be successfully removed.
		/// </exception>
		void DeregisterKeyboardHook(Guid hookId);

		/// <summary>
		/// Deregisters a previously registered mouse hook.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the hook could not be successfully removed.
		/// </exception>
		void DeregisterMouseHook(Guid hookId);

		/// <summary>
		/// Deregisters a previously registered system event hook.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the event hook could not be successfully removed.
		/// </exception>
		void DeregisterSystemEventHook(Guid hookId);

		/// <summary>
		/// Empties the clipboard.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the emptying of the clipboard failed.
		/// </exception>
		void EmptyClipboard();

		/// <summary>
		/// Retrieves the current position of the mouse cursor.
		/// </summary>
		(int x, int y) GetCursorPosition();

		/// <summary>
		/// Retrieves a collection of handles to all currently open (i.e. visible) windows.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the open windows could not be retrieved.
		/// </exception>
		IEnumerable<IntPtr> GetOpenWindows();

		/// <summary>
		/// Retrieves the process identifier for the specified window handle.
		/// </summary>
		uint GetProcessIdFor(IntPtr window);

		/// <summary>
		/// Retrieves a window handle to the Windows taskbar. Returns <c>IntPtr.Zero</c> if the taskbar could not be found (i.e. if it isn't running).
		/// </summary>
		IntPtr GetShellWindowHandle();

		/// <summary>
		/// Retrieves the process identifier of the main Windows explorer instance controlling desktop and taskbar or <c>0</c>, if the process isn't running.
		/// </summary>
		uint GetShellProcessId();

		/// <summary>
		/// Retrieves the path of the currently configured wallpaper image, or an empty string, if there is no wallpaper set.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the wallpaper path could not be retrieved.
		/// </exception>
		string GetWallpaperPath();

		/// <summary>
		/// Attempts to retrieve the icon of the given window. Returns a handle to the icon, or <see cref="IntPtr.Zero"/> if the icon could not be retrieved.
		/// </summary>
		IntPtr GetWindowIcon(IntPtr window);

		/// <summary>
		/// Retrieves the title of the window with the given handle, or an empty string if the given window does not have a title.
		/// </summary>
		string GetWindowTitle(IntPtr window);

		/// <summary>
		/// Retrieves the currently configured working area of the primary screen.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the working area could not be retrieved.
		/// </exception>
		IBounds GetWorkingArea();

		/// <summary>
		/// Determines whether this computer is connected to the internet. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool HasInternetConnection();

		/// <summary>
		/// Hides the given window. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool HideWindow(IntPtr window);

		/// <summary>
		/// Minimizes all open windows.
		/// </summary>
		void MinimizeAllOpenWindows();

		/// <summary>
		/// Instructs the main Windows explorer process to shut down.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the message could not be successfully posted. Does not apply if the process isn't running!
		/// </exception>
		void PostCloseMessageToShell();

		/// <summary>
		/// Prevents Windows from entering sleep mode and keeps all displays powered on.
		/// </summary>
		void PreventSleepMode();

		/// <summary>
		/// Registers a keyboard hook for the given callback. Returns the identifier of the newly registered hook.
		/// </summary>
		Guid RegisterKeyboardHook(KeyboardHookCallback callback);

		/// <summary>
		/// Registers a mouse hook for the given callback. Returns the identifier of the newly registered hook.
		/// </summary>
		Guid RegisterMouseHook(MouseHookCallback callback);

		/// <summary>
		/// Registers a system event which will invoke the specified callback when a window has received mouse capture. Returns the identifier of
		/// the newly registered Windows event hook.
		/// </summary>
		Guid RegisterSystemCaptureStartEvent(SystemEventCallback callback);

		/// <summary>
		/// Registers a system event which will invoke the specified callback when the foreground window has changed. Returns the identifier of the
		/// newly registered Windows event hook.
		/// </summary>
		Guid RegisterSystemForegroundEvent(SystemEventCallback callback);

		/// <summary>
		/// Removes the currently configured desktop wallpaper.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the wallpaper could not be removed.
		/// </exception>
		void RemoveWallpaper();

		/// <summary>
		/// Restores the specified window to its original size and position.
		/// </summary>
		void RestoreWindow(IntPtr window);

		/// <summary>
		/// Attempts to resume the thread referenced by the given thread identifier. Returns <c>true</c> if the thread was successfully resumed,
		/// otherwise <c>false</c>.
		/// </summary>
		bool ResumeThread(int threadId);

		/// <summary>
		/// Sends a close message to the given window.
		/// </summary>
		void SendCloseMessageTo(IntPtr window);

		/// <summary>
		/// Sets the wallpaper to the image located at the specified file path.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the wallpaper could not be set.
		/// </exception>
		void SetWallpaper(string filePath);

		/// <summary>
		/// Sets the working area of the primary screen according to the given dimensions.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the working area could not be set.
		/// </exception>
		void SetWorkingArea(IBounds bounds);

		/// <summary>
		/// Attempts to suspend the thread referenced by the given thread identifier. Returns <c>true</c> if the thread was successfully suspended,
		/// otherwise <c>false</c>.
		/// </summary>
		bool SuspendThread(int threadId);
	}
}
