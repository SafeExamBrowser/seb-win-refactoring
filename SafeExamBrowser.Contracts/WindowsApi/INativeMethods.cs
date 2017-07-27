/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.WindowsApi.Types;

namespace SafeExamBrowser.Contracts.WindowsApi
{
	public interface INativeMethods
	{
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
		/// Retrieves a window handle to the Windows taskbar. Returns <c>IntPtr.Zero</c>
		/// if the taskbar could not be found (i.e. if it isn't running).
		/// </summary>
		IntPtr GetShellWindowHandle();

		/// <summary>
		/// Retrieves the process ID of the main Windows explorer instance controlling
		/// desktop and taskbar or <c>0</c>, if the process isn't running.
		/// </summary>
		uint GetShellProcessId();

		/// <summary>
		/// Retrieves the title of the specified window, or an empty string, if the
		/// given window does not have a title.
		/// </summary>
		string GetWindowTitle(IntPtr window);

		/// <summary>
		/// Retrieves the currently configured working area of the primary screen.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the working area could not be retrieved.
		/// </exception>
		RECT GetWorkingArea();

		/// <summary>
		/// Hides the given window.
		/// </summary>
		void HideWindow(IntPtr window);

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
		/// Registers a system event which will invoke the specified callback when the foreground window has changed.
		/// Returns a handle to the newly registered Windows event hook.
		/// </summary>
		IntPtr RegisterSystemForegroundEvent(Action<IntPtr> callback);

		/// <summary>
		/// Registers a system event which will invoke the specified callback when a window has received mouse capture.
		/// Returns a handle to the newly registered Windows event hook.
		/// </summary>
		IntPtr RegisterSystemCaptureStartEvent(Action<IntPtr> callback);

		/// <summary>
		/// Restores the specified window to its original size and position.
		/// </summary>
		void RestoreWindow(IntPtr window);

		/// <summary>
		/// Sets the working area of the primary screen according to the given dimensions.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the working area could not be set.
		/// </exception>
		void SetWorkingArea(RECT bounds);

		/// <summary>
		/// Unregisters a previously registered system event.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the event hook could not be successfully removed.
		/// </exception>
		void UnregisterSystemEvent(IntPtr handle);
	}
}
